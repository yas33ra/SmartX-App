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
            var sensorDashboardForm = new SensorDashboardForm();
            sensorDashboardForm.Show();
            this.Hide();
        }
    }
}
