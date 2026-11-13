using SmartX.Client.Navigation;
using SmartX.Client.Services;

namespace SmartX.Client
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            
            foreach (var b in new[] { btnIngestion, btnCommandStream, btnTopology })
            {
                b.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            Resize += (s, e) => LayoutMenuButtons();
            Shown += (s, e) => LayoutMenuButtons();
        }

        private void btnIngestion_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.Ingestion);
        }

        private void btnCommandStream_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.CommandStream);
        }

        private void btnTopology_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.Topology);
        }

        // Three equal buttons in a row, centred, sized to the window
        private void LayoutMenuButtons()
        {
            if (ClientSize.Width == 0 || ClientSize.Height == 0) return;   // minimised

            var buttons = new[] { btnIngestion, btnCommandStream, btnTopology };

            int gap = Math.Max(20, ClientSize.Width / 40);
            int width = Math.Clamp((ClientSize.Width - gap * 4) / 3, 150, 320);
            int height = Math.Clamp(ClientSize.Height / 6, 90, 150);

            int totalWidth = width * 3 + gap * 2;
            int left = (ClientSize.Width - totalWidth) / 2;
            int top = (int)(ClientSize.Height * 0.6) - height / 2;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetBounds(left + i * (width + gap), top, width, height);
            }
        }
    }
}
