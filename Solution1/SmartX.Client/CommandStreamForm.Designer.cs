namespace SmartX.Client
{
    partial class CommandStreamForm
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
            btnBackToMenu = new Button();
            lblTitle = new Label();
            txtSearch = new TextBox();
            cmbCategory = new ComboBox();
            chkAlertsOnly = new CheckBox();
            btnRefresh = new Button();
            lvDevices = new ListView();
            btnTurnOn = new Button();
            btnTurnOff = new Button();
            btnAllOff = new Button();
            btnUndo = new Button();
            lblStatus = new Label();
            lblAlerts = new Label();
            lstAlerts = new ListBox();
            lblHistory = new Label();
            lstHistory = new ListBox();
            Name = new ColumnHeader();
            Category = new ColumnHeader();
            State = new ColumnHeader();
            Alerts = new ColumnHeader();
            MAC = new ColumnHeader();
            SuspendLayout();
            // 
            // btnBackToMenu
            // 
            btnBackToMenu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBackToMenu.Location = new Point(688, 30);
            btnBackToMenu.Name = "btnBackToMenu";
            btnBackToMenu.Size = new Size(100, 23);
            btnBackToMenu.TabIndex = 0;
            btnBackToMenu.Text = "Back to Menu";
            btnBackToMenu.UseVisualStyleBackColor = true;
            btnBackToMenu.Click += btnBackToMenu_Click;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(12, 34);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(167, 15);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Real - Time Command Stream";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(22, 85);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search name or MAC";
            txtSearch.Size = new Size(130, 23);
            txtSearch.TabIndex = 2;
            // 
            // cmbCategory
            // 
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.FormattingEnabled = true;
            cmbCategory.Location = new Point(198, 85);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(121, 23);
            cmbCategory.TabIndex = 3;
            // 
            // chkAlertsOnly
            // 
            chkAlertsOnly.AutoSize = true;
            chkAlertsOnly.Location = new Point(334, 87);
            chkAlertsOnly.Name = "chkAlertsOnly";
            chkAlertsOnly.Size = new Size(82, 19);
            chkAlertsOnly.TabIndex = 4;
            chkAlertsOnly.Text = "Alerts only";
            chkAlertsOnly.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(422, 83);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // lvDevices
            // 
            lvDevices.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvDevices.Columns.AddRange(new ColumnHeader[] { Name, Category, State, Alerts, MAC });
            lvDevices.FullRowSelect = true;
            lvDevices.GridLines = true;
            lvDevices.Location = new Point(22, 114);
            lvDevices.Name = "lvDevices";
            lvDevices.Size = new Size(475, 245);
            lvDevices.TabIndex = 6;
            lvDevices.UseCompatibleStateImageBehavior = false;
            lvDevices.View = View.Details;
            // 
            // btnTurnOn
            // 
            btnTurnOn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTurnOn.Location = new Point(25, 375);
            btnTurnOn.Name = "btnTurnOn";
            btnTurnOn.Size = new Size(75, 23);
            btnTurnOn.TabIndex = 7;
            btnTurnOn.Text = "Turn ON";
            btnTurnOn.UseVisualStyleBackColor = true;
            // 
            // btnTurnOff
            // 
            btnTurnOff.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTurnOff.Location = new Point(104, 375);
            btnTurnOff.Name = "btnTurnOff";
            btnTurnOff.Size = new Size(75, 23);
            btnTurnOff.TabIndex = 8;
            btnTurnOff.Text = "Turn OFF";
            btnTurnOff.UseVisualStyleBackColor = true;
            // 
            // btnAllOff
            // 
            btnAllOff.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAllOff.Location = new Point(185, 375);
            btnAllOff.Name = "btnAllOff";
            btnAllOff.Size = new Size(75, 23);
            btnAllOff.TabIndex = 9;
            btnAllOff.Text = "ALL OFF";
            btnAllOff.UseVisualStyleBackColor = true;
            // 
            // btnUndo
            // 
            btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUndo.Location = new Point(266, 375);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(75, 23);
            btnUndo.TabIndex = 10;
            btnUndo.Text = "Undo last";
            btnUndo.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(25, 413);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Ready";
            // 
            // lblAlerts
            // 
            lblAlerts.AutoSize = true;
            lblAlerts.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAlerts.Location = new Point(508, 126);
            lblAlerts.Name = "lblAlerts";
            lblAlerts.Size = new Size(79, 15);
            lblAlerts.TabIndex = 12;
            lblAlerts.Text = "Active Alerts";
            // 
            // lstAlerts
            // 
            lstAlerts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lstAlerts.FormattingEnabled = true;
            lstAlerts.Location = new Point(508, 144);
            lstAlerts.Name = "lstAlerts";
            lstAlerts.Size = new Size(280, 94);
            lstAlerts.TabIndex = 13;
            // 
            // lblHistory
            // 
            lblHistory.AutoSize = true;
            lblHistory.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHistory.Location = new Point(508, 269);
            lblHistory.Name = "lblHistory";
            lblHistory.Size = new Size(184, 15);
            lblHistory.TabIndex = 14;
            lblHistory.Text = "Command History (newest first)";
            // 
            // lstHistory
            // 
            lstHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lstHistory.FormattingEnabled = true;
            lstHistory.Location = new Point(508, 287);
            lstHistory.Name = "lstHistory";
            lstHistory.Size = new Size(280, 94);
            lstHistory.TabIndex = 15;
            // 
            // Name
            // 
            Name.Text = "Name";
            Name.Width = 100;
            // 
            // Category
            // 
            Category.Text = "Category";
            Category.Width = 80;
            // 
            // State
            // 
            State.Text = "State";
            State.Width = 90;
            // 
            // Alerts
            // 
            Alerts.Text = "Alerts";
            Alerts.Width = 90;
            // 
            // MAC
            // 
            MAC.Text = "MAC Address";
            MAC.Width = 110;
            // 
            // CommandStreamForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lstHistory);
            Controls.Add(lblHistory);
            Controls.Add(lstAlerts);
            Controls.Add(lblAlerts);
            Controls.Add(lblStatus);
            Controls.Add(btnUndo);
            Controls.Add(btnAllOff);
            Controls.Add(btnTurnOff);
            Controls.Add(btnTurnOn);
            Controls.Add(lvDevices);
            Controls.Add(btnRefresh);
            Controls.Add(chkAlertsOnly);
            Controls.Add(cmbCategory);
            Controls.Add(txtSearch);
            Controls.Add(lblTitle);
            Controls.Add(btnBackToMenu);
            Name = "CommandStreamForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Real-time Command Sream";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBackToMenu;
        private Label lblTitle;
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private CheckBox chkAlertsOnly;
        private Button btnRefresh;
        private ListView lvDevices;
        private Button btnTurnOn;
        private Button btnTurnOff;
        private Button btnAllOff;
        private Button btnUndo;
        private Label lblStatus;
        private Label lblAlerts;
        private ListBox lstAlerts;
        private Label lblHistory;
        private ListBox lstHistory;
        private ColumnHeader Name;
        private ColumnHeader Category;
        private ColumnHeader State;
        private ColumnHeader Alerts;
        private ColumnHeader MAC;
    }
}