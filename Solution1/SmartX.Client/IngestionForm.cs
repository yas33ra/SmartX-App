using SmartX.Client.Models;
using SmartX.Client.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SmartX.Client
{
    public partial class IngestionForm : Form
    {
        private readonly ApiClient _apiClient = new(); //ApiClient added as a field variable to allow access across all methods in the form
        private string? _selectedFilePath;

        public IngestionForm()
        {
            InitializeComponent();
            cmbCategory.DataSource = Enum.GetValues(typeof(SensorCategory));
            Load += async (s, e) => await RefreshSensorListAsync();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {

            var sensor = new Sensor
            {
                Name = txtName.Text,
                MacAddress = txtMacAddress.Text,
                Facility = txtFacility.Text,
                Zone = txtZone.Text,
                SubZone = txtSubZone.Text,
                Category = (SensorCategory)cmbCategory.SelectedItem
            };


            try
            {
                var registered = await _apiClient.RegisterSensorAsync(sensor);

                if (registered is not null && _selectedFilePath is not null)
                {
                    await _apiClient.UploadFileAsync(registered.Id, _selectedFilePath);
                }

                _selectedFilePath = null;
                lblSelectedFile.Text = "No file selected";
                ClearForm();
                await RefreshSensorListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register sensor: {ex.Message}");
            }

        }

        private void ClearForm()
        {
            txtName.Clear();
            txtMacAddress.Clear();
            txtFacility.Clear();
            txtZone.Clear();
            txtSubZone.Clear();
            cmbCategory.SelectedIndex = -1;
        }

        private async Task RefreshSensorListAsync()
        {
            var sensors = await _apiClient.GetAllSensorsAsync();

            lvSensors.Items.Clear();

            foreach (var sensor in sensors)
            {
                var item = new ListViewItem(sensor.Name);
                item.SubItems.Add(sensor.MacAddress);
                item.SubItems.Add($"{sensor.Facility} / {sensor.Zone} / {sensor.SubZone}");
                item.SubItems.Add(sensor.Category.ToString());
                item.SubItems.Add(sensor.RegisteredAt.ToLocalTime().ToString("g"));
                lvSensors.Items.Add(item);
            }
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategory.SelectedItem is not SensorCategory selected)
            {
                lblCategoryHint.Text = string.Empty;
                return;
            }

            lblCategoryHint.Text = selected switch
            {
                SensorCategory.Climate => "Measures surroundings, e.g. temperature, humidity, moisture",
                SensorCategory.Electrical => "Tracks electrical usage, e.g. power, voltage, battery level",
                SensorCategory.Control => "A controllable device, e.g. valves, relays",
                _ => string.Empty
            };
        }

        private void btnAttachFile_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Select a configuration file, photo, or log",
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _selectedFilePath = dialog.FileName;
                lblSelectedFile.Text = Path.GetFileName(dialog.FileName);
            }
        }

        private void btnBacktoDash_Click(object sender, EventArgs e)
        {
            MainMenuForm mainMenu = new MainMenuForm();
            mainMenu.Show();
            this.Hide();
        }
    }
}
