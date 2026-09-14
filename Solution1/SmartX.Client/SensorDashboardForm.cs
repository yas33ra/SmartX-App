using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SmartX.Client
{
    public partial class SensorDashboardForm : Form
    {
        public SensorDashboardForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ingestionForm = new IngestionForm();
            ingestionForm.Show();
            this.Hide();
        }
    }
}
