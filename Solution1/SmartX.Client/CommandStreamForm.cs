using SmartX.Client.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartX.Client.Services;

namespace SmartX.Client
{
    public partial class CommandStreamForm : Form
    {
        private readonly ApiClient _apiClient = new();

        // Polls the API every 3 seconds while this page is visible
        private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 3000 };

        // Waits 300 ms after the last keystroke before searching, so we don't call the API per letter
        private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 300 };

        private bool _isRefreshing;

        public CommandStreamForm()
        {
            InitializeComponent();

            cmbCategory.Items.AddRange(new object[] { "All", "Climate", "Electrical", "Control" });
            cmbCategory.SelectedIndex = 0;

            // Page visible = start polling; hidden = stop (the Navigator hides forms instead of closing them)
            VisibleChanged += CommandStreamForm_VisibleChanged;
            _refreshTimer.Tick += async (s, e) => await RefreshAllSafeAsync();

            // Filters
            txtSearch.TextChanged += (s, e) => { _searchTimer.Stop(); _searchTimer.Start(); };
            _searchTimer.Tick += async (s, e) =>
            {
                _searchTimer.Stop();
                await RecordSearchSafeAsync(txtSearch.Text);   // tell the engine the USER searched
                await RefreshAllSafeAsync();
            };
            cmbCategory.SelectedIndexChanged += async (s, e) => await RefreshAllSafeAsync();
            chkAlertsOnly.CheckedChanged += async (s, e) => await RefreshAllSafeAsync();
            btnRefresh.Click += async (s, e) => await RefreshAllSafeAsync();

            // Commands
            btnTurnOn.Click += async (s, e) => await SendToSelectedAsync(on: true);
            btnTurnOff.Click += async (s, e) => await SendToSelectedAsync(on: false);
            btnAllOff.Click += async (s, e) => await ShutDownAllAsync();
            btnUndo.Click += async (s, e) => await UndoAsync();

            // Suggestions
            lstSuggestions.SelectedIndexChanged += (s, e) =>
                lblSuggestionWhy.Text = (lstSuggestions.SelectedItem as SuggestionInfo)?.Evidence ?? "";
            btnApplySuggestion.Click += async (s, e) => await ApplySuggestionAsync();

            FormClosed += (s, e) => { _refreshTimer.Dispose(); _searchTimer.Dispose(); };
        }

        //-----------------------------------------------------------------

        private async void CommandStreamForm_VisibleChanged(object? sender, EventArgs e)
        {
            if (Visible)
            {
                _refreshTimer.Start();
                await RefreshAllSafeAsync();
            }
            else
            {
                _refreshTimer.Stop();
            }
        }

        //-----------------------------------------------------------------
        // Loading data

        // Never throws: a down API shows a status message instead of crashing the page
        private async Task RefreshAllSafeAsync()
        {
            if (_isRefreshing) return;      // skip if the previous refresh is still running
            _isRefreshing = true;

            try
            {
                string? category = cmbCategory.SelectedIndex <= 0 ? null : cmbCategory.Text;
                bool? hasAlerts = chkAlertsOnly.Checked ? true : null;

                // Ask for everything at once
                var devicesTask = _apiClient.GetDevicesAsync(txtSearch.Text, category, hasAlerts);
                var alertsTask = _apiClient.GetAlertsAsync();
                var historyTask = _apiClient.GetCommandHistoryAsync();
                var suggestionsTask = _apiClient.GetSuggestionsAsync();
                var insightsTask = _apiClient.GetDeviceInsightsAsync();
                await Task.WhenAll(devicesTask, alertsTask, historyTask, suggestionsTask, insightsTask);

                ShowDevices(devicesTask.Result);
                ShowAlerts(alertsTask.Result);
                ShowHistory(historyTask.Result);
                ShowSuggestions(suggestionsTask.Result);
                ShowInsights(insightsTask.Result);
            }
            catch (HttpRequestException)
            {
                ShowStatus("Cannot reach the API. Is it running?", isError: true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Refresh failed: {ex.Message}", isError: true);
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void ShowDevices(List<DeviceInfo> devices)
        {
            // Remember what was selected so a refresh doesn't wipe the user's selection
            var selectedIds = lvDevices.SelectedItems.Cast<ListViewItem>()
                                       .Select(i => (Guid)i.Tag!).ToHashSet();

            lvDevices.BeginUpdate();
            lvDevices.Items.Clear();

            foreach (var d in devices)
            {
                string state = d.IsOn is null ? "-" : d.IsOn.Value ? "ON" : "OFF";
                string alerts = string.Join(", ", d.ActiveAlerts);

                var item = new ListViewItem(new[] { d.Name, d.Category, state, alerts, d.MacAddress })
                {
                    Tag = d.Id                                  // keep the ID for sending commands
                };

                if (d.ActiveAlerts.Count > 0)
                {
                    item.ForeColor = Color.Firebrick;           // alerting devices stand out
                }

                lvDevices.Items.Add(item);
                item.Selected = selectedIds.Contains(d.Id);     // set AFTER adding to the list
            }

            lvDevices.EndUpdate();
        }

        private void ShowAlerts(AlertsResponse alerts)
        {
            lblAlerts.Text = $"Active Alerts ({alerts.Active.Count}) - {alerts.DuplicatesSuppressed} duplicates blocked";

            lstAlerts.BeginUpdate();
            lstAlerts.Items.Clear();
            foreach (var a in alerts.Active)
            {
                lstAlerts.Items.Add($"{a.SensorName} - {a.Kind}");
            }
            lstAlerts.EndUpdate();
        }

        private void ShowHistory(List<CommandInfo> history)
        {
            lstHistory.BeginUpdate();
            lstHistory.Items.Clear();
            foreach (var c in history)
            {
                lstHistory.Items.Add($"{c.IssuedAt.ToLocalTime():HH:mm:ss}  {c.Description}");
            }
            lstHistory.EndUpdate();
        }

        private void ShowSuggestions(List<SuggestionInfo> suggestions)
        {
            // Remember the selection so the 3-second refresh doesn't deselect while the user reads
            var selectedText = (lstSuggestions.SelectedItem as SuggestionInfo)?.Text;

            lstSuggestions.BeginUpdate();
            lstSuggestions.Items.Clear();
            foreach (var s in suggestions)
            {
                lstSuggestions.Items.Add(s);
            }
            lstSuggestions.EndUpdate();

            gbSuggestions.Text = $"Suggested Actions ({suggestions.Count})";

            int match = suggestions.FindIndex(s => s.Text == selectedText);
            if (match >= 0)
            {
                lstSuggestions.SelectedIndex = match;     // also refreshes the "why" text
            }
            else
            {
                lblSuggestionWhy.Text = suggestions.Count == 0
                    ? "No suggestions yet. They appear as the system learns your habits."
                    : "Select a suggestion to see why it was made.";
            }
        }

        private void ShowInsights(List<InsightInfo> insights)
        {
            gbInsights.Text = $"Automated Insights ({insights.Count})";

            lstInsights.BeginUpdate();
            lstInsights.Items.Clear();
            foreach (var i in insights)
            {
                lstInsights.Items.Add(i);
            }
            lstInsights.EndUpdate();
        }

        //-----------------------------------------------------------------
        // Commands

        private async Task SendToSelectedAsync(bool on)
        {
            var ids = lvDevices.SelectedItems.Cast<ListViewItem>().Select(i => (Guid)i.Tag!).ToList();

            // An empty list would mean "every device" to the API, so require an explicit selection here
            if (ids.Count == 0)
            {
                ShowStatus("Select one or more Control devices first.", isError: true);
                return;
            }

            await RunCommandAsync(async () =>
            {
                var command = await _apiClient.IssueCommandAsync(on, ids);
                return command.Description;
            });
        }

        private async Task ShutDownAllAsync()
        {
            var confirm = MessageBox.Show("Turn OFF every control device?", "Confirm shutdown",
                                          MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            await RunCommandAsync(async () =>
            {
                var command = await _apiClient.IssueCommandAsync(on: false, sensorIds: null);
                return command.Description;
            });
        }

        private async Task UndoAsync()
        {
            await RunCommandAsync(async () =>
            {
                var undone = await _apiClient.UndoLastCommandAsync();
                return $"Undid: {undone?.Description}";
            });
        }

        private async Task ApplySuggestionAsync()
        {
            if (lstSuggestions.SelectedItem is not SuggestionInfo suggestion)
            {
                ShowStatus("Select a suggestion first.", isError: true);
                return;
            }

            if (suggestion.Kind == "Search")
            {
                txtSearch.Text = suggestion.SearchTerm ?? "";     // the normal search flow takes over
                ShowStatus($"Searching for \"{suggestion.SearchTerm}\"", isError: false);
                return;
            }

            if (suggestion.On is null || suggestion.SensorIds is null) return;

            await RunCommandAsync(async () =>
            {
                var command = await _apiClient.IssueCommandAsync(suggestion.On.Value, suggestion.SensorIds);
                return $"Applied suggestion: {command.Description}";
            });
        }

        // Learning is best-effort: if it fails, searching must still work
        private async Task RecordSearchSafeAsync(string term)
        {
            if (term.Trim().Length < 3) return;       // ignore half-typed words

            try
            {
                await _apiClient.RecordSearchAsync(term);
            }
            catch
            {
                // deliberately ignored
            }
        }

        // Runs a command, shows the result (or the error), then refreshes the screen
        private async Task RunCommandAsync(Func<Task<string>> action)
        {
            try
            {
                string message = await action();
                ShowStatus(message, isError: false);
                await RefreshAllSafeAsync();
            }
            catch (InvalidOperationException ex)       // the API's validation messages
            {
                ShowStatus(ex.Message, isError: true);
            }
            catch (HttpRequestException)
            {
                ShowStatus("Cannot reach the API. Is it running?", isError: true);
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
        }

        //-----------------------------------------------------------------

        // Already wired in the Designer; keep this name exactly
        private void btnBackToMenu_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.MainMenu);
        }
    }
}