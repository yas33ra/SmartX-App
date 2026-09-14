namespace SmartX.Client
{
    partial class MainMenuForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenuForm));
            btnCommandStream = new Button();
            btnTopology = new Button();
            btnIngestion = new Button();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // btnCommandStream
            // 
            btnCommandStream.Location = new Point(338, 267);
            btnCommandStream.Name = "btnCommandStream";
            btnCommandStream.Size = new Size(139, 95);
            btnCommandStream.TabIndex = 0;
            btnCommandStream.Text = "Real-Time Command Stream and History";
            btnCommandStream.UseVisualStyleBackColor = true;
            // 
            // btnTopology
            // 
            btnTopology.Location = new Point(592, 267);
            btnTopology.Name = "btnTopology";
            btnTopology.Size = new Size(139, 95);
            btnTopology.TabIndex = 1;
            btnTopology.Text = "Network Topology and Mesh Routing";
            btnTopology.UseVisualStyleBackColor = true;
            // 
            // btnIngestion
            // 
            btnIngestion.Location = new Point(90, 267);
            btnIngestion.Name = "btnIngestion";
            btnIngestion.Size = new Size(139, 95);
            btnIngestion.TabIndex = 2;
            btnIngestion.Text = "Sensor Data Ingestion and Telemetry";
            btnIngestion.UseVisualStyleBackColor = true;
            btnIngestion.Click += btnIngestion_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.ControlDark;
            label1.Location = new Point(359, 333);
            label1.Name = "label1";
            label1.Size = new Size(96, 15);
            label1.TabIndex = 3;
            label1.Text = "Coming in part 2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = SystemColors.ControlDark;
            label2.Location = new Point(614, 333);
            label2.Name = "label2";
            label2.Size = new Size(96, 15);
            label2.TabIndex = 4;
            label2.Text = "Coming in Part 3";
            // 
            // MainMenuForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(800, 450);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnIngestion);
            Controls.Add(btnTopology);
            Controls.Add(btnCommandStream);
            Name = "MainMenuForm";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCommandStream;
        private Button btnTopology;
        private Button btnIngestion;
        private Label label1;
        private Label label2;
    }
}
