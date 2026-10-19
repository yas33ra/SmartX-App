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
            colName = new ColumnHeader();
            colCategory = new ColumnHeader();
            colState = new ColumnHeader();
            colAlerts = new ColumnHeader();
            colMAC = new ColumnHeader();
            btnTurnOn = new Button();
            btnTurnOff = new Button();
            btnAllOff = new Button();
            btnUndo = new Button();
            lblStatus = new Label();
            lblAlerts = new Label();
            lstAlerts = new ListBox();
            lblHistory = new Label();
            lstHistory = new ListBox();
            gbSuggestions = new GroupBox();
            btnApplySuggestion = new Button();
            lblSuggestionWhy = new Label();
            lstSuggestions = new ListBox();
            gbInsights = new GroupBox();
            lstInsights = new ListBox();
            gbSuggestions.SuspendLayout();
            gbInsights.SuspendLayout();
            SuspendLayout();
            // 
            // btnBackToMenu
            // 
            btnBackToMenu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBackToMenu.Location = new Point(688, 30);
            btnBackToMenu.Name = "btnBackToMenu";
            btnBackToMenu.Size = new Size(284, 23);
            btnBackToMenu.TabIndex = 0;
            btnBackToMenu.Text = "Back to Menu";
            btnBackToMenu.UseVisualStyleBackColor = true;
            btnBackToMenu.Click += btnBackToMenu_Click;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(12, 34);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(275, 25);
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
            lvDevices.Columns.AddRange(new ColumnHeader[] { colName, colCategory, colState, colAlerts, colMAC });
            lvDevices.FullRowSelect = true;
            lvDevices.GridLines = true;
            lvDevices.Location = new Point(22, 114);
            lvDevices.Name = "lvDevices";
            lvDevices.Size = new Size(659, 368);
            lvDevices.TabIndex = 6;
            lvDevices.UseCompatibleStateImageBehavior = false;
            lvDevices.View = View.Details;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 100;
            // 
            // colCategory
            // 
            colCategory.Text = "Category";
            colCategory.Width = 80;
            // 
            // colState
            // 
            colState.Text = "State";
            colState.Width = 90;
            // 
            // colAlerts
            // 
            colAlerts.Text = "Alerts";
            colAlerts.Width = 90;
            // 
            // colMAC
            // 
            colMAC.Text = "MAC Address";
            colMAC.Width = 110;
            // 
            // btnTurnOn
            // 
            btnTurnOn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTurnOn.Location = new Point(22, 497);
            btnTurnOn.Name = "btnTurnOn";
            btnTurnOn.Size = new Size(75, 23);
            btnTurnOn.TabIndex = 7;
            btnTurnOn.Text = "Turn ON";
            btnTurnOn.UseVisualStyleBackColor = true;
            // 
            // btnTurnOff
            // 
            btnTurnOff.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnTurnOff.Location = new Point(104, 497);
            btnTurnOff.Name = "btnTurnOff";
            btnTurnOff.Size = new Size(75, 23);
            btnTurnOff.TabIndex = 8;
            btnTurnOff.Text = "Turn OFF";
            btnTurnOff.UseVisualStyleBackColor = true;
            // 
            // btnAllOff
            // 
            btnAllOff.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAllOff.Location = new Point(185, 497);
            btnAllOff.Name = "btnAllOff";
            btnAllOff.Size = new Size(75, 23);
            btnAllOff.TabIndex = 9;
            btnAllOff.Text = "ALL OFF";
            btnAllOff.UseVisualStyleBackColor = true;
            // 
            // btnUndo
            // 
            btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUndo.Location = new Point(275, 497);
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
            lblStatus.Location = new Point(22, 537);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Ready";
            // 
            // lblAlerts
            // 
            lblAlerts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblAlerts.AutoSize = true;
            lblAlerts.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAlerts.Location = new Point(703, 114);
            lblAlerts.Name = "lblAlerts";
            lblAlerts.Size = new Size(79, 15);
            lblAlerts.TabIndex = 12;
            lblAlerts.Text = "Active Alerts";
            // 
            // lstAlerts
            // 
            lstAlerts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lstAlerts.FormattingEnabled = true;
            lstAlerts.Location = new Point(692, 129);
            lstAlerts.Name = "lstAlerts";
            lstAlerts.Size = new Size(280, 154);
            lstAlerts.TabIndex = 13;
            // 
            // lblHistory
            // 
            lblHistory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblHistory.AutoSize = true;
            lblHistory.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHistory.Location = new Point(703, 310);
            lblHistory.Name = "lblHistory";
            lblHistory.Size = new Size(184, 15);
            lblHistory.TabIndex = 14;
            lblHistory.Text = "Command History (newest first)";
            // 
            // lstHistory
            // 
            lstHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lstHistory.FormattingEnabled = true;
            lstHistory.Location = new Point(692, 328);
            lstHistory.Name = "lstHistory";
            lstHistory.Size = new Size(280, 154);
            lstHistory.TabIndex = 15;
            // 
            // gbSuggestions
            // 
            gbSuggestions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            gbSuggestions.Controls.Add(btnApplySuggestion);
            gbSuggestions.Controls.Add(lblSuggestionWhy);
            gbSuggestions.Controls.Add(lstSuggestions);
            gbSuggestions.Location = new Point(22, 573);
            gbSuggestions.Name = "gbSuggestions";
            gbSuggestions.Size = new Size(538, 172);
            gbSuggestions.TabIndex = 16;
            gbSuggestions.TabStop = false;
            gbSuggestions.Text = "Suggested Actions";
            // 
            // btnApplySuggestion
            // 
            btnApplySuggestion.Location = new Point(376, 135);
            btnApplySuggestion.Name = "btnApplySuggestion";
            btnApplySuggestion.Size = new Size(132, 23);
            btnApplySuggestion.TabIndex = 2;
            btnApplySuggestion.Text = "Apply Selected";
            btnApplySuggestion.UseVisualStyleBackColor = true;
            // 
            // lblSuggestionWhy
            // 
            lblSuggestionWhy.Location = new Point(20, 139);
            lblSuggestionWhy.Name = "lblSuggestionWhy";
            lblSuggestionWhy.Size = new Size(350, 23);
            lblSuggestionWhy.TabIndex = 1;
            // 
            // lstSuggestions
            // 
            lstSuggestions.FormattingEnabled = true;
            lstSuggestions.Location = new Point(20, 35);
            lstSuggestions.Name = "lstSuggestions";
            lstSuggestions.Size = new Size(488, 94);
            lstSuggestions.TabIndex = 0;
            // 
            // gbInsights
            // 
            gbInsights.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            gbInsights.Controls.Add(lstInsights);
            gbInsights.Location = new Point(620, 513);
            gbInsights.Name = "gbInsights";
            gbInsights.Size = new Size(352, 222);
            gbInsights.TabIndex = 17;
            gbInsights.TabStop = false;
            gbInsights.Text = "Automated Insights";
            // 
            // lstInsights
            // 
            lstInsights.FormattingEnabled = true;
            lstInsights.HorizontalScrollbar = true;
            lstInsights.Location = new Point(6, 24);
            lstInsights.Name = "lstInsights";
            lstInsights.Size = new Size(323, 184);
            lstInsights.TabIndex = 0;
            // 
            // CommandStreamForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.commandbackground;
            ClientSize = new Size(984, 761);
            Controls.Add(gbInsights);
            Controls.Add(gbSuggestions);
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
            gbSuggestions.ResumeLayout(false);
            gbInsights.ResumeLayout(false);
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
        private ColumnHeader colName;
        private ColumnHeader colCategory;
        private ColumnHeader colState;
        private ColumnHeader colAlerts;
        private ColumnHeader colMAC;
        private GroupBox gbSuggestions;
        private Button btnApplySuggestion;
        private Label lblSuggestionWhy;
        private ListBox lstSuggestions;
        private GroupBox gbInsights;
        private ListBox lstInsights;
    }
}