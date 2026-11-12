using SmartX.Client.Navigation;
using SmartX.Client.Services;

namespace SmartX.Client
{
    // The Network Topology and Mesh Routing page.
    // Built in code (no Designer file), so there are no Designer names to fix.
    public class TopologyForm : Form
    {
        // IMPORTANT: create the ApiClient the same way CommandStreamForm does.
        private readonly ApiClient _api = new();

        private const int MaxRowsShown = 1000;

        private enum HistoryMode { Range, Latest, LastHour }

        // Top bar and drawing
        private readonly Label _lblTitle = new();
        private readonly Button _btnRefresh = new();
        private readonly Button _btnBack = new();
        private readonly MeshPanel _meshPanel = new();
        private readonly Panel _rightPanel = new();
        private readonly Label _lblSelected = new();
        private readonly Label _lblStatus = new();

        // Tabs
        private readonly TabControl _tabs = new();
        private readonly TabPage _tabRoute = new("Route");
        private readonly TabPage _tabHistory = new("History log");

        // Route tab
        private readonly Label _lblTrack = new();
        private readonly TextBox _txtNode = new();
        private readonly Button _btnFindRoute = new();
        private readonly CheckBox _chkFewest = new();
        private readonly Button _btnDown = new();
        private readonly Button _btnUp = new();
        private readonly TextBox _txtRoute = new();

        // History tab
        private readonly Label _lblFrom = new();
        private readonly Label _lblTo = new();
        private readonly DateTimePicker _dtFrom = new();
        private readonly DateTimePicker _dtTo = new();
        private readonly Button _btnLoadRange = new();
        private readonly Button _btnLatest = new();
        private readonly Button _btnLastHour = new();
        private readonly Label _lblHistoryInfo = new();
        private readonly ListView _lvHistory = new();

        // Polls the API every 3 seconds, but only while this page is visible
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 3000 };
        private bool _refreshing;

        private MeshNodeInfo? _selected;
        private RouteComparison? _lastRoute;

        public TopologyForm()
        {
            Text = "Smart-X - Network Topology and Mesh Routing";
            ClientSize = new Size(1100, 720);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;

            BuildTopBar();
            BuildRightPanel();
            BuildRouteTab();
            BuildHistoryTab();

            // The drawn mesh (left, grows with the window)
            _meshPanel.Location = new Point(10, 55);
            _meshPanel.Size = new Size(ClientSize.Width - 410, ClientSize.Height - 105);
            _meshPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _meshPanel.BorderStyle = BorderStyle.FixedSingle;
            _meshPanel.NodeClicked += OnNodeClicked;

            // Status bar along the bottom
            _lblStatus.Location = new Point(10, ClientSize.Height - 40);
            _lblStatus.Size = new Size(ClientSize.Width - 20, 28);
            _lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            _lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblStatus.Text = "Loading...";

            Controls.Add(_lblTitle);
            Controls.Add(_btnBack);
            Controls.Add(_btnRefresh);
            Controls.Add(_meshPanel);
            Controls.Add(_rightPanel);
            Controls.Add(_lblStatus);

            _timer.Tick += async (s, e) => await RefreshAsync();
            VisibleChanged += OnVisibleChanged;
        }

        // ---------- building the screen ----------

        private void BuildTopBar()
        {
            _lblTitle.Text = "Network Topology and Mesh Routing";
            _lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(12, 12);

            _btnBack.Text = "Back to Menu";
            _btnBack.Size = new Size(120, 32);
            _btnBack.Location = new Point(ClientSize.Width - 132, 10);
            _btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnBack.Click += (s, e) => Navigator.NavigateTo(AppModule.MainMenu);

            _btnRefresh.Text = "Refresh";
            _btnRefresh.Size = new Size(90, 32);
            _btnRefresh.Location = new Point(ClientSize.Width - 132 - 98, 10);
            _btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnRefresh.Click += async (s, e) => await RefreshAsync();
        }

        private void BuildRightPanel()
        {
            _rightPanel.Location = new Point(ClientSize.Width - 390, 55);
            _rightPanel.Size = new Size(380, ClientSize.Height - 105);
            _rightPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            _rightPanel.BorderStyle = BorderStyle.FixedSingle;

            _lblSelected.Location = new Point(10, 8);
            _lblSelected.Size = new Size(358, 62);
            _lblSelected.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblSelected.Text = "Click a node to select it.";

            _tabs.Location = new Point(6, 74);
            _tabs.Size = new Size(366, _rightPanel.Height - 82);
            _tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _tabs.TabPages.Add(_tabRoute);
            _tabs.TabPages.Add(_tabHistory);

            _rightPanel.Controls.Add(_lblSelected);
            _rightPanel.Controls.Add(_tabs);
        }

        private void BuildRouteTab()
        {
            _lblTrack.Location = new Point(6, 8);
            _lblTrack.Size = new Size(340, 20);
            _lblTrack.Text = "Track a route (MAC address, sensor id or 'gateway'):";

            _txtNode.Location = new Point(6, 32);
            _txtNode.Size = new Size(238, 27);
            _txtNode.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await FindRouteAsync();
                }
            };

            _btnFindRoute.Text = "Find route";
            _btnFindRoute.Location = new Point(250, 30);
            _btnFindRoute.Size = new Size(92, 30);
            _btnFindRoute.Click += async (s, e) => await FindRouteAsync();

            _chkFewest.Text = "Highlight the fewest-hops route instead";
            _chkFewest.Location = new Point(6, 66);
            _chkFewest.Size = new Size(336, 24);
            _chkFewest.CheckedChanged += (s, e) =>
            {
                if (_lastRoute is not null) ShowRoute(_lastRoute);
            };

            _btnDown.Text = "Mark node DOWN";
            _btnDown.Location = new Point(6, 96);
            _btnDown.Size = new Size(166, 32);
            _btnDown.Enabled = false;
            _btnDown.Click += async (s, e) => await SetSelectedNodeDownAsync(true);

            _btnUp.Text = "Mark node UP";
            _btnUp.Location = new Point(176, 96);
            _btnUp.Size = new Size(166, 32);
            _btnUp.Enabled = false;
            _btnUp.Click += async (s, e) => await SetSelectedNodeDownAsync(false);

            _txtRoute.Location = new Point(6, 136);
            _txtRoute.Size = new Size(336, 250);
            _txtRoute.Multiline = true;
            _txtRoute.ReadOnly = true;
            _txtRoute.ScrollBars = ScrollBars.Vertical;
            _txtRoute.BackColor = Color.White;
            _txtRoute.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _txtRoute.Text = "Click a node, or type a MAC address and press Enter.";

            _tabRoute.Controls.Add(_lblTrack);
            _tabRoute.Controls.Add(_txtNode);
            _tabRoute.Controls.Add(_btnFindRoute);
            _tabRoute.Controls.Add(_chkFewest);
            _tabRoute.Controls.Add(_btnDown);
            _tabRoute.Controls.Add(_btnUp);
            _tabRoute.Controls.Add(_txtRoute);
        }

        private void BuildHistoryTab()
        {
            _lblFrom.Text = "From";
            _lblFrom.Location = new Point(6, 8);
            _lblFrom.Size = new Size(160, 20);

            _lblTo.Text = "To";
            _lblTo.Location = new Point(176, 8);
            _lblTo.Size = new Size(160, 20);

            _dtFrom.Format = DateTimePickerFormat.Custom;
            _dtFrom.CustomFormat = "yyyy-MM-dd HH:mm";
            _dtFrom.Location = new Point(6, 30);
            _dtFrom.Size = new Size(166, 27);
            _dtFrom.Value = DateTime.Now.AddHours(-1);

            _dtTo.Format = DateTimePickerFormat.Custom;
            _dtTo.CustomFormat = "yyyy-MM-dd HH:mm";
            _dtTo.Location = new Point(176, 30);
            _dtTo.Size = new Size(166, 27);
            _dtTo.Value = DateTime.Now.AddMinutes(1);

            _btnLoadRange.Text = "Load range";
            _btnLoadRange.Location = new Point(6, 64);
            _btnLoadRange.Size = new Size(108, 30);
            _btnLoadRange.Click += async (s, e) => await LoadHistoryAsync(HistoryMode.Range);

            _btnLatest.Text = "Latest 50";
            _btnLatest.Location = new Point(118, 64);
            _btnLatest.Size = new Size(108, 30);
            _btnLatest.Click += async (s, e) => await LoadHistoryAsync(HistoryMode.Latest);

            _btnLastHour.Text = "Last hour";
            _btnLastHour.Location = new Point(230, 64);
            _btnLastHour.Size = new Size(112, 30);
            _btnLastHour.Click += async (s, e) => await LoadHistoryAsync(HistoryMode.LastHour);

            _lblHistoryInfo.Location = new Point(6, 100);
            _lblHistoryInfo.Size = new Size(336, 40);
            _lblHistoryInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _lblHistoryInfo.ForeColor = Color.DimGray;
            _lblHistoryInfo.Text = "Select a sensor to see its readings.";

            _lvHistory.Location = new Point(6, 144);
            _lvHistory.Size = new Size(336, 240);
            _lvHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _lvHistory.View = View.Details;
            _lvHistory.FullRowSelect = true;
            _lvHistory.GridLines = true;
            _lvHistory.HideSelection = false;
            _lvHistory.Columns.Add("Time", 170);
            _lvHistory.Columns.Add("Value", 130);

            _tabHistory.Controls.Add(_lblFrom);
            _tabHistory.Controls.Add(_lblTo);
            _tabHistory.Controls.Add(_dtFrom);
            _tabHistory.Controls.Add(_dtTo);
            _tabHistory.Controls.Add(_btnLoadRange);
            _tabHistory.Controls.Add(_btnLatest);
            _tabHistory.Controls.Add(_btnLastHour);
            _tabHistory.Controls.Add(_lblHistoryInfo);
            _tabHistory.Controls.Add(_lvHistory);
        }

        // ---------- refreshing ----------

        private async void OnVisibleChanged(object? sender, EventArgs e)
        {
            if (Visible)
            {
                _timer.Start();
                await RefreshAsync();
            }
            else
            {
                _timer.Stop();
            }
        }

        private async Task RefreshAsync()
        {
            if (_refreshing) return;   // do not start a second refresh while one is running
            _refreshing = true;

            try
            {
                var meshTask = _api.GetMeshAsync();
                var statusTask = _api.GetMeshStatusAsync();
                var mstTask = _api.GetMstAsync();
                await Task.WhenAll(meshTask, statusTask, mstTask);

                var mesh = meshTask.Result;
                var status = statusTask.Result;
                var mst = mstTask.Result;

                _meshPanel.SetData(mesh, mst);
                ShowSelected(mesh.Nodes.FirstOrDefault(n => n.Id == _meshPanel.SelectedId));

                _lblStatus.ForeColor = Color.Black;
                _lblStatus.Text =
                    $"Connected: {status.Connected.Count}   |   Down: {status.Down.Count}   |   " +
                    $"Unreachable: {status.CutOff.Count}   |   Cheapest tree cost: {mst.TotalCost:0.##}";

                // Keep the tracked route up to date when nodes go down or come back
                await FindRouteAsync(quiet: true);
            }
            catch (Exception ex)
            {
                _lblStatus.ForeColor = Color.Firebrick;
                _lblStatus.Text = ex is HttpRequestException
                    ? "Cannot reach the API. Is SmartX.API running?"
                    : "Could not load the mesh: " + ex.Message;
            }
            finally
            {
                _refreshing = false;
            }
        }

        // ---------- selecting a node ----------

        private async void OnNodeClicked(object? sender, MeshNodeInfo node)
        {
            ShowSelected(node);

            // Fill the box and track this node straight away
            _txtNode.Text = node.IsGateway ? "gateway" : node.MacAddress ?? node.Id.ToString();
            await FindRouteAsync();

            // Also load its latest readings into the history tab
            await LoadHistoryAsync(HistoryMode.Latest);
        }

        private void ShowSelected(MeshNodeInfo? node)
        {
            _selected = node;

            if (node is null)
            {
                _lblSelected.Text = "Click a node to select it.";
                _btnDown.Enabled = false;
                _btnUp.Enabled = false;
                return;
            }

            string address = node.IsGateway ? "Gateway (central server)" : node.MacAddress ?? "";
            _lblSelected.Text = $"Selected: {node.Name}\n{address}" +
                                (node.IsDown ? "\nSTATUS: DOWN" : "") +
                                $"\nLinks: {node.LinkCount}";

            // The gateway can never go down
            _btnDown.Enabled = !node.IsGateway && !node.IsDown;
            _btnUp.Enabled = !node.IsGateway && node.IsDown;
        }

        // ---------- route tracking ----------

        private async Task FindRouteAsync(bool quiet = false)
        {
            string node = _txtNode.Text.Trim();

            if (node.Length == 0)
            {
                if (!quiet)
                {
                    _txtRoute.Text = "Enter a MAC address, sensor id or 'gateway', or click a node.";
                }
                return;
            }

            try
            {
                var result = await _api.GetRouteComparisonAsync(node);
                _lastRoute = result;
                ShowRoute(result);
            }
            catch (InvalidOperationException ex)
            {
                if (quiet) return;
                _lastRoute = null;
                _meshPanel.ClearRoute();
                _txtRoute.Text = "Error: " + ex.Message;
            }
            catch (HttpRequestException)
            {
                if (quiet) return;
                _txtRoute.Text = "Cannot reach the API. Is SmartX.API running?";
            }
        }

        private void ShowRoute(RouteComparison result)
        {
            var nl = Environment.NewLine;

            if (!result.Reachable)
            {
                _meshPanel.ClearRoute();
                _txtRoute.Text = $"Node: {result.Node}{nl}{nl}{result.Message}";
                return;
            }

            var cheapest = result.PowerEfficientRoute;
            var fewest = result.FewestHopsRoute;

            _txtRoute.Text =
                $"Node: {result.Node}{nl}{nl}" +
                $"Cheapest route (least power):{nl}" +
                $"  {cheapest?.Path}{nl}" +
                $"  {cheapest?.Hops} hops, cost {cheapest?.Cost:0.##}{nl}{nl}" +
                $"Fewest-hops route:{nl}" +
                $"  {fewest?.Path}{nl}" +
                $"  {fewest?.Hops} hops, cost {fewest?.Cost:0.##}";

            // Orange line on the drawing
            _meshPanel.SetRoute(_chkFewest.Checked ? fewest?.Path : cheapest?.Path);
        }

        // ---------- take the selected node down / bring it back ----------

        private async Task SetSelectedNodeDownAsync(bool down)
        {
            if (_selected is null || _selected.IsGateway) return;

            try
            {
                await _api.SetNodeDownAsync(_selected.Id.ToString(), down);
                await RefreshAsync();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Smart-X", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Cannot reach the API. Is SmartX.API running?", "Smart-X",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------- history log ----------

        private async Task LoadHistoryAsync(HistoryMode mode)
        {
            var sensor = _selected;

            if (sensor is null || sensor.IsGateway)
            {
                _lvHistory.Items.Clear();
                _lblHistoryInfo.Text = "Select a sensor first (the gateway has no readings).";
                return;
            }

            try
            {
                if (mode == HistoryMode.LastHour)
                {
                    _dtTo.Value = DateTime.Now.AddMinutes(1);
                    _dtFrom.Value = DateTime.Now.AddHours(-1);
                    mode = HistoryMode.Range;
                }

                List<HistoryPointInfo> points;

                if (mode == HistoryMode.Range)
                {
                    if (_dtFrom.Value > _dtTo.Value)
                    {
                        _lblHistoryInfo.Text = "'From' must be earlier than 'To'.";
                        return;
                    }

                    // The pickers show local time, the API works in UTC
                    points = await _api.GetHistoryAsync(
                        sensor.Id, _dtFrom.Value.ToUniversalTime(), _dtTo.Value.ToUniversalTime());
                }
                else
                {
                    points = await _api.GetHistoryAsync(sensor.Id, limit: 50);
                }

                var stats = await _api.GetHistoryStatsAsync(sensor.Id);

                // Newest first, and never put more than a few thousand rows in the list
                string unit = sensor.Category switch
                {
                    "Electrical" => " W",
                    "Climate" => " °C",
                    _ => ""
                };

                var newestFirst = points.AsEnumerable().Reverse().Take(MaxRowsShown).ToList();

                _lvHistory.BeginUpdate();
                _lvHistory.Items.Clear();

                foreach (var p in newestFirst)
                {
                    // JSON times with Z or an offset become local; plain times are shown as they are
                    var shown = p.TimeStamp.Kind == DateTimeKind.Unspecified ? p.TimeStamp : p.TimeStamp.ToLocalTime();

                    var item = new ListViewItem(shown.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.SubItems.Add(p.Value.ToString("0.##") + unit);
                    _lvHistory.Items.Add(item);
                }

                _lvHistory.EndUpdate();

                string what = mode == HistoryMode.Range ? "in this range" : "(latest)";
                string listed = points.Count > newestFirst.Count ? $" (newest {newestFirst.Count} listed)" : "";

                _lblHistoryInfo.Text = points.Count == 0
                    ? $"{sensor.Name}: no readings {what}.\nAVL tree: {stats.Count} stored in total."
                    : $"{sensor.Name}: {points.Count} reading(s) {what}{listed}\n" +
                      $"AVL tree: {stats.Count} stored, height {stats.TreeHeight} (perfectly balanced: {stats.IdealHeight})";
            }
            catch (HttpRequestException)
            {
                _lblHistoryInfo.Text = "Cannot reach the API. Is SmartX.API running?";
            }
            catch (Exception ex)
            {
                _lblHistoryInfo.Text = "Could not load the history: " + ex.Message;
            }
        }
    }
}