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
            cmbCategory = new ComboBox();
            btnAttachFile = new Button();
            lvSensors = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            lblCategoryHint = new Label();
            txtFacility = new TextBox();
            txtZone = new TextBox();
            txtSubZone = new TextBox();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            lblSelectedFile = new Label();
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
            label4.Location = new Point(39, 281);
            label4.Name = "label4";
            label4.Size = new Size(55, 15);
            label4.TabIndex = 3;
            label4.Text = "Category";
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(187, 399);
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
            // cmbCategory
            // 
            cmbCategory.FormattingEnabled = true;
            cmbCategory.Location = new Point(164, 281);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(121, 23);
            cmbCategory.TabIndex = 9;
            cmbCategory.SelectedIndexChanged += cmbCategory_SelectedIndexChanged;
            // 
            // btnAttachFile
            // 
            btnAttachFile.Location = new Point(43, 349);
            btnAttachFile.Name = "btnAttachFile";
            btnAttachFile.Size = new Size(75, 23);
            btnAttachFile.TabIndex = 10;
            btnAttachFile.Text = "Attach Image";
            btnAttachFile.UseVisualStyleBackColor = true;
            btnAttachFile.Click += btnAttachFile_Click;
            // 
            // lvSensors
            // 
            lvSensors.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            lvSensors.Location = new Point(357, 82);
            lvSensors.Name = "lvSensors";
            lvSensors.Size = new Size(417, 82);
            lvSensors.TabIndex = 11;
            lvSensors.UseCompatibleStateImageBehavior = false;
            lvSensors.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Name";
            columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "MAC Address";
            columnHeader2.Width = 90;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Location";
            columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Category";
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Registered at";
            columnHeader5.Width = 80;
            // 
            // lblCategoryHint
            // 
            lblCategoryHint.AutoSize = true;
            lblCategoryHint.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCategoryHint.ForeColor = SystemColors.ActiveBorder;
            lblCategoryHint.Location = new Point(164, 307);
            lblCategoryHint.Name = "lblCategoryHint";
            lblCategoryHint.Size = new Size(38, 13);
            lblCategoryHint.TabIndex = 12;
            lblCategoryHint.Text = "label6";
            // 
            // txtFacility
            // 
            txtFacility.Location = new Point(164, 229);
            txtFacility.Name = "txtFacility";
            txtFacility.Size = new Size(100, 23);
            txtFacility.TabIndex = 13;
            // 
            // txtZone
            // 
            txtZone.Location = new Point(299, 229);
            txtZone.Name = "txtZone";
            txtZone.Size = new Size(100, 23);
            txtZone.TabIndex = 14;
            // 
            // txtSubZone
            // 
            txtSubZone.Location = new Point(434, 229);
            txtSubZone.Name = "txtSubZone";
            txtSubZone.Size = new Size(100, 23);
            txtSubZone.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.Location = new Point(164, 213);
            label6.Name = "label6";
            label6.Size = new Size(42, 13);
            label6.TabIndex = 16;
            label6.Text = "Facility";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label7.Location = new Point(299, 213);
            label7.Name = "label7";
            label7.Size = new Size(33, 13);
            label7.TabIndex = 17;
            label7.Text = "Zone";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.Location = new Point(434, 213);
            label8.Name = "label8";
            label8.Size = new Size(57, 13);
            label8.TabIndex = 18;
            label8.Text = "Sub-Zone";
            // 
            // lblSelectedFile
            // 
            lblSelectedFile.AutoSize = true;
            lblSelectedFile.ForeColor = SystemColors.ControlDarkDark;
            lblSelectedFile.Location = new Point(39, 375);
            lblSelectedFile.Name = "lblSelectedFile";
            lblSelectedFile.Size = new Size(88, 15);
            lblSelectedFile.TabIndex = 19;
            lblSelectedFile.Text = "No file selected";
            // 
            // IngestionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblSelectedFile);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(txtSubZone);
            Controls.Add(txtZone);
            Controls.Add(txtFacility);
            Controls.Add(lblCategoryHint);
            Controls.Add(lvSensors);
            Controls.Add(btnAttachFile);
            Controls.Add(cmbCategory);
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
        private ComboBox cmbCategory;
        private Button btnAttachFile;
        private ListView lvSensors;
        private Label lblCategoryHint;
        private TextBox txtFacility;
        private TextBox txtZone;
        private TextBox txtSubZone;
        private Label label6;
        private Label label7;
        private Label label8;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private Label lblSelectedFile;
    }
}