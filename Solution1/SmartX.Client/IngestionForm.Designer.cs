namespace SmartX.Client
{
    partial class IngestionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            btnRegister = new Button();
            label5 = new Label();
            txtMacAddress = new TextBox();
            txtName = new TextBox();
            txtLocation = new TextBox();
            cmbCategory = new ComboBox();
            btnAttachFile = new Button();
            lvSensors = new ListView();
            lblCategoryHint = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(299, 20);
            label1.Name = "label1";
            label1.Size = new Size(171, 15);
            label1.TabIndex = 0;
            label1.Text = "Sensor Data and Live Telemetry";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(39, 165);
            label2.Name = "label2";
            label2.Size = new Size(79, 15);
            label2.TabIndex = 1;
            label2.Text = "MAC Address";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(39, 219);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 2;
            label3.Text = "Location";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(39, 269);
            label4.Name = "label4";
            label4.Size = new Size(55, 15);
            label4.TabIndex = 3;
            label4.Text = "Category";
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(72, 398);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(173, 23);
            btnRegister.TabIndex = 4;
            btnRegister.Text = "Register Sensor";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(39, 115);
            label5.Name = "label5";
            label5.Size = new Size(77, 15);
            label5.TabIndex = 5;
            label5.Text = "Sensor Name";
            // 
            // txtMacAddress
            // 
            txtMacAddress.Location = new Point(164, 162);
            txtMacAddress.Name = "txtMacAddress";
            txtMacAddress.Size = new Size(100, 23);
            txtMacAddress.TabIndex = 6;
            // 
            // txtName
            // 
            txtName.Location = new Point(164, 107);
            txtName.Name = "txtName";
            txtName.Size = new Size(100, 23);
            txtName.TabIndex = 7;
            // 
            // txtLocation
            // 
            txtLocation.Location = new Point(164, 216);
            txtLocation.Name = "txtLocation";
            txtLocation.Size = new Size(100, 23);
            txtLocation.TabIndex = 8;
            // 
            // cmbCategory
            // 
            cmbCategory.FormattingEnabled = true;
            cmbCategory.Location = new Point(164, 266);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(121, 23);
            cmbCategory.TabIndex = 9;
            cmbCategory.SelectedIndexChanged += cmbCategory_SelectedIndexChanged;
            // 
            // btnAttachFile
            // 
            btnAttachFile.Location = new Point(66, 337);
            btnAttachFile.Name = "btnAttachFile";
            btnAttachFile.Size = new Size(75, 23);
            btnAttachFile.TabIndex = 10;
            btnAttachFile.Text = "Attach Image";
            btnAttachFile.UseVisualStyleBackColor = true;
            // 
            // lvSensors
            // 
            lvSensors.Location = new Point(354, 107);
            lvSensors.Name = "lvSensors";
            lvSensors.Size = new Size(417, 177);
            lvSensors.TabIndex = 11;
            lvSensors.UseCompatibleStateImageBehavior = false;
            // 
            // lblCategoryHint
            // 
            lblCategoryHint.AutoSize = true;
            lblCategoryHint.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCategoryHint.ForeColor = SystemColors.ActiveBorder;
            lblCategoryHint.Location = new Point(207, 292);
            lblCategoryHint.Name = "lblCategoryHint";
            lblCategoryHint.Size = new Size(38, 13);
            lblCategoryHint.TabIndex = 12;
            lblCategoryHint.Text = "label6";
            // 
            // IngestionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblCategoryHint);
            Controls.Add(lvSensors);
            Controls.Add(btnAttachFile);
            Controls.Add(cmbCategory);
            Controls.Add(txtLocation);
            Controls.Add(txtName);
            Controls.Add(txtMacAddress);
            Controls.Add(label5);
            Controls.Add(btnRegister);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "IngestionForm";
            Text = "IngestionForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button btnRegister;
        private Label label5;
        private TextBox txtMacAddress;
        private TextBox txtName;
        private TextBox txtLocation;
        private ComboBox cmbCategory;
        private Button btnAttachFile;
        private ListView lvSensors;
        private Label lblCategoryHint;
    }
}