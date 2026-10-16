using SmartX.Client.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SmartX.Client
{
    public partial class CommandStreamForm : Form
    {
        public CommandStreamForm()
        {
            InitializeComponent();
        }

        private void btnBackToMenu_Click(object sender, EventArgs e)
        {
            Navigator.NavigateTo(AppModule.MainMenu);
        }
    }
}
