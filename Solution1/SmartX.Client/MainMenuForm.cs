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
            IngestionForm ingestionForm = new IngestionForm();
            ingestionForm.Show();
            this.Hide();
        }
    }
}
