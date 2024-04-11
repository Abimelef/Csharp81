namespace Csharp81
{
    partial class frmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblStatic = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox1
            // 
            this.PictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox1.Image")));
            this.PictureBox1.Location = new System.Drawing.Point(12, 12);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(48, 48);
            this.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox1.TabIndex = 1;
            this.PictureBox1.TabStop = false;
            // 
            // lblStatic
            // 
            this.lblStatic.AutoSize = true;
            this.lblStatic.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblStatic.Location = new System.Drawing.Point(90, 26);
            this.lblStatic.Name = "lblStatic";
            this.lblStatic.Size = new System.Drawing.Size(416, 25);
            this.lblStatic.TabIndex = 2;
            this.lblStatic.Text = "C#81 - Sinclair ZX81 Emulator Ported from VB81";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Label2.Location = new System.Drawing.Point(90, 136);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(346, 68);
            this.Label2.TabIndex = 5;
            this.Label2.Text = resources.GetString("Label2.Text");
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Label1.Location = new System.Drawing.Point(90, 77);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(468, 25);
            this.Label1.TabIndex = 6;
            this.Label1.Text = "VB81 Copyright © 1999-2002  Grok Developments Ltd.";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Label3.Location = new System.Drawing.Point(90, 228);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(307, 51);
            this.Label3.TabIndex = 7;
            this.Label3.Text = "(Chris Cowley original ported to run in C# .NET 6.0 \r\nby Allan Macpherson 2024\r\na" +
    "llan.macpherson@btinternet.com)";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(252, 304);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 339);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.lblStatic);
            this.Controls.Add(this.PictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(604, 378);
            this.MinimumSize = new System.Drawing.Size(604, 378);
            this.Name = "frmAbout";
            this.Text = "About C#81";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal PictureBox PictureBox1;
        internal Label lblStatic;
        internal Label Label2;
        internal Label Label1;
        internal Label Label3;
        private Button button1;
    }
}