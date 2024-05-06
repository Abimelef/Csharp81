namespace Csharp81
{
    partial class frmMemoCalc
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
            groupBox1 = new GroupBox();
            rbRemove = new RadioButton();
            rbPageOnly = new RadioButton();
            rbPageAndStart = new RadioButton();
            btnOK = new Button();
            btnCancel = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(rbRemove);
            groupBox1.Controls.Add(rbPageOnly);
            groupBox1.Controls.Add(rbPageAndStart);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(473, 169);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "MemoCalc ROM Settings";
            // 
            // rbRemove
            // 
            rbRemove.AutoSize = true;
            rbRemove.Location = new Point(34, 119);
            rbRemove.Name = "rbRemove";
            rbRemove.Size = new Size(236, 29);
            rbRemove.TabIndex = 2;
            rbRemove.TabStop = true;
            rbRemove.Text = "Remove MemoCalc ROM";
            rbRemove.UseVisualStyleBackColor = true;
            // 
            // rbPageOnly
            // 
            rbPageOnly.AutoSize = true;
            rbPageOnly.Location = new Point(34, 84);
            rbPageOnly.Name = "rbPageOnly";
            rbPageOnly.Size = new Size(349, 29);
            rbPageOnly.TabIndex = 1;
            rbPageOnly.TabStop = true;
            rbPageOnly.Text = "Page MemoCalc ROM in (4KB at 3000h)";
            rbPageOnly.UseVisualStyleBackColor = true;
            // 
            // rbPageAndStart
            // 
            rbPageAndStart.AutoSize = true;
            rbPageAndStart.Location = new Point(34, 49);
            rbPageAndStart.Name = "rbPageAndStart";
            rbPageAndStart.Size = new Size(426, 29);
            rbPageAndStart.TabIndex = 0;
            rbPageAndStart.TabStop = true;
            rbPageAndStart.Text = "Page MemoCalc ROM in and autostart (JP 3600h)";
            rbPageAndStart.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(255, 187);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(112, 34);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(373, 187);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(112, 34);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmMemoCalc
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(527, 251);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(groupBox1);
            Name = "frmMemoCalc";
            Text = "MemoCalc";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private RadioButton rbRemove;
        private RadioButton rbPageOnly;
        private RadioButton rbPageAndStart;
        private Button btnOK;
        private Button btnCancel;
    }
}