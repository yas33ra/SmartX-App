using System.Drawing.Drawing2D;
using SmartX.Client.Services;

namespace SmartX.Client
{
    // Draws the mesh: the gateway and sensors as circles, links as lines with their cost.
    //   green line  = link used by the cheapest tree (MST)
    //   orange line = the route currently being tracked
    //   red circle  = node that is down
    public class MeshPanel : Panel
    {
        private const float NodeRadius = 24f;

        private MeshInfo _mesh = new();
        private HashSet<string> _mstPairs = new();
        private HashSet<string> _routePairs = new();
        private readonly Dictionary<Guid, PointF> _positions = new();

        public Guid? SelectedId { get; private set; }

        // Raised when the user clicks a node
        public event EventHandler<MeshNodeInfo>? NodeClicked;

        public MeshPanel()
        {
            DoubleBuffered = true;   // no flicker while redrawing
            ResizeRedraw = true;     // redraw when the window is resized
            BackColor = Color.White;
        }

        public void SetData(MeshInfo mesh, MstInfo mst)
        {
            _mesh = mesh;
            _mstPairs = mst.Edges.Select(e => PairKey(e.From, e.To)).ToHashSet();
            Invalidate();
        }

        // path looks like "Pump A → Mains Meter → Gateway"
        public void SetRoute(string? path)
        {
            _routePairs = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(path))
            {
                var names = path.Split(" → ");
                for (int i = 0; i < names.Length - 1; i++)
                {
                    _routePairs.Add(PairKey(names[i], names[i + 1]));
                }
            }

            Invalidate();
        }

        public void ClearRoute() => SetRoute(null);

        // A link has no direction, so A-B and B-A give the same key
        private static string PairKey(string a, string b) =>
            string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";

        // Gateway in the middle, sensors spread around it in an ellipse
        private void ComputePositions()
        {
            _positions.Clear();

            float centreX = Width / 2f;
            float centreY = Height / 2f - 10;

            var gateway = _mesh.Nodes.FirstOrDefault(n => n.IsGateway);
            if (gateway is not null)
            {
                _positions[gateway.Id] = new PointF(centreX, centreY);
            }

            var sensors = _mesh.Nodes.Where(n => !n.IsGateway).ToList();
            float radiusX = Math.Max(60, Width / 2f - 90);
            float radiusY = Math.Max(60, Height / 2f - 80);

            for (int i = 0; i < sensors.Count; i++)
            {
                double angle = -Math.PI / 2 + 2 * Math.PI * i / sensors.Count;
                _positions[sensors[i].Id] = new PointF(
                    centreX + radiusX * (float)Math.Cos(angle),
                    centreY + radiusY * (float)Math.Sin(angle));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_mesh.Nodes.Count == 0)
            {
                using var emptyFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("No mesh data yet. Is the API running?", Font, Brushes.Gray,
                    new PointF(Width / 2f, Height / 2f), emptyFormat);
                return;
            }

            ComputePositions();
            var nodesById = _mesh.Nodes.ToDictionary(n => n.Id);

            // Links: normal ones first, then the tracked route on top
            DrawLinks(g, nodesById, routeLinks: false);
            DrawLinks(g, nodesById, routeLinks: true);

            // Cost labels
            foreach (var link in _mesh.Links)
            {
                if (!_positions.TryGetValue(link.FromId, out var a) ||
                    !_positions.TryGetValue(link.ToId, out var b)) continue;

                var middle = new PointF((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);
                string text = link.Cost.ToString("0.##");
                var size = g.MeasureString(text, Font);
                var box = new RectangleF(middle.X - size.Width / 2f, middle.Y - size.Height / 2f,
                                         size.Width, size.Height);

                g.FillRectangle(Brushes.White, box);
                g.DrawString(text, Font, Brushes.Black, box.Location);
            }

            // Nodes
            using var labelFormat = new StringFormat { Alignment = StringAlignment.Center };
            using var centreFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var boldFont = new Font(Font, FontStyle.Bold);

            foreach (var node in _mesh.Nodes)
            {
                if (!_positions.TryGetValue(node.Id, out var p)) continue;

                var fill = node.IsGateway ? Color.FromArgb(41, 98, 255)
                         : node.IsDown ? Color.FromArgb(220, 53, 69)
                         : Color.FromArgb(32, 160, 120);

                var circle = new RectangleF(p.X - NodeRadius, p.Y - NodeRadius, NodeRadius * 2, NodeRadius * 2);

                using (var brush = new SolidBrush(fill))
                {
                    g.FillEllipse(brush, circle);
                }

                bool selected = SelectedId == node.Id;
                using (var outline = new Pen(selected ? Color.Black : Color.White, selected ? 3 : 2))
                {
                    g.DrawEllipse(outline, circle);
                }

                if (node.IsGateway)
                {
                    g.DrawString("GW", boldFont, Brushes.White, p, centreFormat);
                }

                string label = node.Name + (node.IsDown ? " (DOWN)" : "");
                g.DrawString(label, Font, Brushes.Black, p.X, p.Y + NodeRadius + 4, labelFormat);
            }

            DrawLegend(g);
        }

        private void DrawLinks(Graphics g, Dictionary<Guid, MeshNodeInfo> nodesById, bool routeLinks)
        {
            foreach (var link in _mesh.Links)
            {
                string key = PairKey(link.FromName, link.ToName);
                bool onRoute = _routePairs.Contains(key);

                if (onRoute != routeLinks) continue;

                if (!_positions.TryGetValue(link.FromId, out var a) ||
                    !_positions.TryGetValue(link.ToId, out var b)) continue;

                bool touchesDown =
                    (nodesById.TryGetValue(link.FromId, out var from) && from.IsDown) ||
                    (nodesById.TryGetValue(link.ToId, out var to) && to.IsDown);

                Pen pen;
                if (onRoute)
                    pen = new Pen(Color.FromArgb(230, 126, 34), 5);
                else if (touchesDown)
                    pen = new Pen(Color.FromArgb(200, 200, 200), 1) { DashStyle = DashStyle.Dash };
                else if (_mstPairs.Contains(key))
                    pen = new Pen(Color.FromArgb(39, 174, 96), 3);
                else
                    pen = new Pen(Color.FromArgb(170, 170, 170), 1.5f);

                using (pen)
                {
                    g.DrawLine(pen, a, b);
                }
            }
        }

        private void DrawLegend(Graphics g)
        {
            float y = Height - 90;

            void Line(Color colour, float width, string text, DashStyle dash = DashStyle.Solid)
            {
                using var pen = new Pen(colour, width) { DashStyle = dash };
                g.DrawLine(pen, 10, y + 8, 40, y + 8);
                g.DrawString(text, Font, Brushes.DimGray, 46, y);
                y += 20;
            }

            Line(Color.FromArgb(39, 174, 96), 3, "cheapest tree (MST)");
            Line(Color.FromArgb(230, 126, 34), 5, "tracked route");
            Line(Color.FromArgb(200, 200, 200), 1, "link to a down node", DashStyle.Dash);

            using var red = new SolidBrush(Color.FromArgb(220, 53, 69));
            g.FillEllipse(red, 18, y + 1, 14, 14);
            g.DrawString("node down", Font, Brushes.DimGray, 46, y);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            ComputePositions();

            foreach (var node in _mesh.Nodes)
            {
                if (!_positions.TryGetValue(node.Id, out var p)) continue;

                double dx = e.X - p.X;
                double dy = e.Y - p.Y;

                if (Math.Sqrt(dx * dx + dy * dy) <= NodeRadius + 4)
                {
                    SelectedId = node.Id;
                    Invalidate();
                    NodeClicked?.Invoke(this, node);
                    return;
                }
            }
        }
    }
}