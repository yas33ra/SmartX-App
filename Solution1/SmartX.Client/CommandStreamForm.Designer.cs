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
            SuspendLayout();
            // 
            // btnBackToMenu
            // 
            btnBackToMenu.Location = new Point(125, 132);
            btnBackToMenu.Name = "btnBackToMenu";
            btnBackToMenu.Size = new Size(75, 23);
            btnBackToMenu.TabIndex = 0;
            btnBackToMenu.Text = "back";
            btnBackToMenu.UseVisualStyleBackColor = true;
            btnBackToMenu.Click += btnBackToMenu_Click;
            // 
            // CommandStreamForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnBackToMenu);
            Name = "CommandStreamForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Real-time Command Sream";
            ResumeLayout(false);
        }

        #endregion

        private Button btnBackToMenu;
    }
}