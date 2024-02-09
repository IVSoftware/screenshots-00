namespace screenshots_00
{
    partial class MainForm
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
            tableLayoutPanel1 = new TableLayoutPanel();
            buttonSingle = new Button();
            checkBoxAuto = new CheckBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(buttonSingle, 0, 0);
            tableLayoutPanel1.Controls.Add(checkBoxAuto, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Bottom;
            tableLayoutPanel1.Location = new Point(0, 199);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(478, 45);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonSingle
            // 
            buttonSingle.Anchor = AnchorStyles.None;
            buttonSingle.Location = new Point(63, 5);
            buttonSingle.Name = "buttonSingle";
            buttonSingle.Size = new Size(112, 34);
            buttonSingle.TabIndex = 0;
            buttonSingle.Text = "Single";
            buttonSingle.UseVisualStyleBackColor = true;
            // 
            // checkBoxAuto
            // 
            checkBoxAuto.Anchor = AnchorStyles.None;
            checkBoxAuto.Appearance = Appearance.Button;
            checkBoxAuto.Location = new Point(302, 5);
            checkBoxAuto.Name = "checkBoxAuto";
            checkBoxAuto.Size = new Size(112, 34);
            checkBoxAuto.TabIndex = 1;
            checkBoxAuto.Text = "Auto";
            checkBoxAuto.TextAlign = ContentAlignment.MiddleCenter;
            checkBoxAuto.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 244);
            Controls.Add(tableLayoutPanel1);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button buttonSingle;
        private CheckBox checkBoxAuto;
    }
}
