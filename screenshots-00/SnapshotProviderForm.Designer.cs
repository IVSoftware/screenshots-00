namespace screenshots_00
{
    partial class SnapshotProviderForm
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
            labelElapsedTime = new Label();
            SuspendLayout();
            // 
            // labelElapsedTime
            // 
            labelElapsedTime.BackColor = Color.MidnightBlue;
            labelElapsedTime.Dock = DockStyle.Fill;
            labelElapsedTime.Font = new Font("Segoe UI", 50F);
            labelElapsedTime.ForeColor = Color.WhiteSmoke;
            labelElapsedTime.Location = new Point(5, 5);
            labelElapsedTime.Name = "labelElapsedTime";
            labelElapsedTime.Size = new Size(468, 234);
            labelElapsedTime.TabIndex = 0;
            labelElapsedTime.Text = "00:00:00";
            labelElapsedTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SnapshotProviderForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 244);
            Controls.Add(labelElapsedTime);
            Name = "SnapshotProviderForm";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.Manual;
            Text = "Snapshot Provider Form";
            ResumeLayout(false);
        }

        #endregion

        private Label labelElapsedTime;
    }
}