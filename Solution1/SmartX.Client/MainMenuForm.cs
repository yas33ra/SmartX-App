using SmartX.Client.Navigation;

namespace SmartX.Client
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void btnIngestion_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.Ingestion);
        }

        private void btnCommandStream_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.CommandStream);
        }
    }
}
