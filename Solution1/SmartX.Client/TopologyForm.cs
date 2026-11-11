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

        private readonly Label _lblTitle = new();
        private readonly Button _btnRefresh = new();
        private readonly Button _btnBack = new();
        private readonly MeshPanel _meshPanel = new();
        private readonly Panel _rightPanel = new();
        private readonly Label _lblSelected = new();
        private readonly Label _lblComingNext = new();
        private readonly Label _lblStatus = new();

        // Polls the API every 3 seconds, but only while this page is visible
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 3000 };
        private bool _refreshing;

        public TopologyForm()
        {
            Text = "Smart-X - Network Topology and Mesh Routing";
            ClientSize = new Size(1100, 720);
            MinimumSize = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;

            // Title and top buttons
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

            // The drawn mesh (left, grows with the window)
            _meshPanel.Location = new Point(10, 55);
            _meshPanel.Size = new Size(ClientSize.Width - 410, ClientSize.Height - 105);
            _meshPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _meshPanel.BorderStyle = BorderStyle.FixedSingle;
            _meshPanel.NodeClicked += OnNodeClicked;

            // Right side: selected node now, route lookup and history log in the next steps
            _rightPanel.Location = new Point(ClientSize.Width - 390, 55);
            _rightPanel.Size = new Size(380, ClientSize.Height - 105);
            _rightPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            _rightPanel.BorderStyle = BorderStyle.FixedSingle;

            _lblSelected.Location = new Point(10, 10);
            _lblSelected.Size = new Size(358, 60);
            _lblSelected.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblSelected.Text = "Click a node to select it.";

            _lblComingNext.Location = new Point(10, 90);
            _lblComingNext.Size = new Size(358, 60);
            _lblComingNext.ForeColor = Color.Gray;
            _lblComingNext.Text = "Route lookup and the history log are added in the next steps.";

            _rightPanel.Controls.Add(_lblSelected);
            _rightPanel.Controls.Add(_lblComingNext);

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

        private void OnNodeClicked(object? sender, MeshNodeInfo node) => ShowSelected(node);

        private void ShowSelected(MeshNodeInfo? node)
        {
            if (node is null)
            {
                _lblSelected.Text = "Click a node to select it.";
                return;
            }

            string address = node.IsGateway ? "Gateway (central server)" : node.MacAddress ?? "";
            _lblSelected.Text = $"Selected: {node.Name}\n{address}" +
                                (node.IsDown ? "\nSTATUS: DOWN" : "") +
                                $"\nLinks: {node.LinkCount}";
        }
    }
}