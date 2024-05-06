namespace Csharp81
{
    partial class FrmOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmOptions));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.lblFastModeCol = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.picFastmodeCol = new System.Windows.Forms.PictureBox();
            this.picBorderCol = new System.Windows.Forms.PictureBox();
            this.picForegroundCol = new System.Windows.Forms.PictureBox();
            this.picBackgroundCol = new System.Windows.Forms.PictureBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFastmodeCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBorderCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picForegroundCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundCol)).BeginInit();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.Controls.Add(this.lblFastModeCol);
            this.Panel1.Controls.Add(this.Label3);
            this.Panel1.Controls.Add(this.Label2);
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.picFastmodeCol);
            this.Panel1.Controls.Add(this.picBorderCol);
            this.Panel1.Controls.Add(this.picForegroundCol);
            this.Panel1.Controls.Add(this.picBackgroundCol);
            this.Panel1.Location = new System.Drawing.Point(12, 12);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(204, 142);
            this.Panel1.TabIndex = 1;
            // 
            // lblFastModeCol
            // 
            this.lblFastModeCol.AutoSize = true;
            this.lblFastModeCol.Location = new System.Drawing.Point(14, 101);
            this.lblFastModeCol.Name = "lblFastModeCol";
            this.lblFastModeCol.Size = new System.Drawing.Size(99, 15);
            this.lblFastModeCol.TabIndex = 7;
            this.lblFastModeCol.Text = "Fast mode colour";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(14, 72);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(79, 15);
            this.Label3.TabIndex = 6;
            this.Label3.Text = "Border colour";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(14, 43);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(106, 15);
            this.Label2.TabIndex = 5;
            this.Label2.Text = "Foreground colour";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(14, 14);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(108, 15);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Background colour";
            // 
            // picFastmodeCol
            // 
            this.picFastmodeCol.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picFastmodeCol.Location = new System.Drawing.Point(128, 101);
            this.picFastmodeCol.Name = "picFastmodeCol";
            this.picFastmodeCol.Size = new System.Drawing.Size(62, 23);
            this.picFastmodeCol.TabIndex = 3;
            this.picFastmodeCol.TabStop = false;
            this.picFastmodeCol.Click += new System.EventHandler(this.PicFastmodeCol_Click);
            // 
            // picBorderCol
            // 
            this.picBorderCol.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picBorderCol.Location = new System.Drawing.Point(128, 72);
            this.picBorderCol.Name = "picBorderCol";
            this.picBorderCol.Size = new System.Drawing.Size(62, 23);
            this.picBorderCol.TabIndex = 2;
            this.picBorderCol.TabStop = false;
            this.picBorderCol.Click += new System.EventHandler(this.PicBorderCol_Click);
            // 
            // picForegroundCol
            // 
            this.picForegroundCol.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picForegroundCol.Location = new System.Drawing.Point(128, 43);
            this.picForegroundCol.Name = "picForegroundCol";
            this.picForegroundCol.Size = new System.Drawing.Size(62, 23);
            this.picForegroundCol.TabIndex = 1;
            this.picForegroundCol.TabStop = false;
            this.picForegroundCol.Click += new System.EventHandler(this.PicForegroundCol_Click);
            // 
            // picBackgroundCol
            // 
            this.picBackgroundCol.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picBackgroundCol.Location = new System.Drawing.Point(128, 14);
            this.picBackgroundCol.Name = "picBackgroundCol";
            this.picBackgroundCol.Size = new System.Drawing.Size(62, 23);
            this.picBackgroundCol.TabIndex = 0;
            this.picBackgroundCol.TabStop = false;
            this.picBackgroundCol.Click += new System.EventHandler(this.PicBackgroundCol_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 160);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(64, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(82, 160);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(64, 23);
            this.btnPreview.TabIndex = 4;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.BtnPreview_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(152, 160);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // frmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 196);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.Panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmOptions";
            this.Text = "Screen Colours";
            this.Load += new System.EventHandler(this.FrmOptions_Load);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFastmodeCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBorderCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picForegroundCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBackgroundCol)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal Panel Panel1;
        internal Label lblFastModeCol;
        internal Label Label3;
        internal Label Label2;
        internal Label Label1;
        internal PictureBox picFastmodeCol;
        internal PictureBox picBorderCol;
        internal PictureBox picForegroundCol;
        internal PictureBox picBackgroundCol;
        internal Button btnOK;
        internal Button btnPreview;
        internal Button btnCancel;
        private ColorDialog colorDialog1;
    }
}