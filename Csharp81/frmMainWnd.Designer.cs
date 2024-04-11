namespace Csharp81
{
    partial class frmMainWnd
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMainWnd));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.specifyTapeDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetZX81ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.displaySizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showToolbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowWritesToShadowRom816KAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideScreenInFastModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.zX81KeyboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutC81ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.picDisplay = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem2});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(276, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.specifyTapeDirectoryToolStripMenuItem,
            this.resetZX81ToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItem1.Text = "&File";
            // 
            // specifyTapeDirectoryToolStripMenuItem
            // 
            this.specifyTapeDirectoryToolStripMenuItem.Name = "specifyTapeDirectoryToolStripMenuItem";
            this.specifyTapeDirectoryToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.specifyTapeDirectoryToolStripMenuItem.Text = "&Specify Tape Directory";
            this.specifyTapeDirectoryToolStripMenuItem.Click += new System.EventHandler(this.specifyTapeDirectoryToolStripMenuItem_Click);
            // 
            // resetZX81ToolStripMenuItem
            // 
            this.resetZX81ToolStripMenuItem.Name = "resetZX81ToolStripMenuItem";
            this.resetZX81ToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.resetZX81ToolStripMenuItem.Text = "&Reset ZX81";
            this.resetZX81ToolStripMenuItem.Click += new System.EventHandler(this.resetZX81ToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displaySizeToolStripMenuItem,
            this.showToolbarToolStripMenuItem,
            this.allowWritesToShadowRom816KAreaToolStripMenuItem,
            this.hideScreenInFastModeToolStripMenuItem});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(61, 20);
            this.toolStripMenuItem3.Text = "&Options";
            // 
            // displaySizeToolStripMenuItem
            // 
            this.displaySizeToolStripMenuItem.Name = "displaySizeToolStripMenuItem";
            this.displaySizeToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.displaySizeToolStripMenuItem.Text = "Colours...";
            this.displaySizeToolStripMenuItem.Click += new System.EventHandler(this.displaySizeToolStripMenuItem_Click);
            // 
            // showToolbarToolStripMenuItem
            // 
            this.showToolbarToolStripMenuItem.Name = "showToolbarToolStripMenuItem";
            this.showToolbarToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.showToolbarToolStripMenuItem.Text = "&Show Toolbar";
            this.showToolbarToolStripMenuItem.Click += new System.EventHandler(this.showToolbarToolStripMenuItem_Click);
            // 
            // allowWritesToShadowRom816KAreaToolStripMenuItem
            // 
            this.allowWritesToShadowRom816KAreaToolStripMenuItem.Name = "allowWritesToShadowRom816KAreaToolStripMenuItem";
            this.allowWritesToShadowRom816KAreaToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.allowWritesToShadowRom816KAreaToolStripMenuItem.Text = "&Allow Writes to Shadow Rom (8-16K area)";
            // 
            // hideScreenInFastModeToolStripMenuItem
            // 
            this.hideScreenInFastModeToolStripMenuItem.Name = "hideScreenInFastModeToolStripMenuItem";
            this.hideScreenInFastModeToolStripMenuItem.Size = new System.Drawing.Size(293, 22);
            this.hideScreenInFastModeToolStripMenuItem.Text = "&Hide Screen in Fast Mode ";
            this.hideScreenInFastModeToolStripMenuItem.Click += new System.EventHandler(this.hideScreenInFastModeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zX81KeyboardToolStripMenuItem,
            this.aboutC81ToolStripMenuItem});
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(44, 20);
            this.toolStripMenuItem4.Text = "&Help";
            // 
            // zX81KeyboardToolStripMenuItem
            // 
            this.zX81KeyboardToolStripMenuItem.Name = "zX81KeyboardToolStripMenuItem";
            this.zX81KeyboardToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.zX81KeyboardToolStripMenuItem.Text = "&ZX81 Keyboard";
            this.zX81KeyboardToolStripMenuItem.Click += new System.EventHandler(this.zX81KeyboardToolStripMenuItem_Click);
            // 
            // aboutC81ToolStripMenuItem
            // 
            this.aboutC81ToolStripMenuItem.Name = "aboutC81ToolStripMenuItem";
            this.aboutC81ToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.aboutC81ToolStripMenuItem.Text = "&About C#81";
            this.aboutC81ToolStripMenuItem.Click += new System.EventHandler(this.aboutC81ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(12, 20);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(12, 20);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(280, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStrip1.Visible = false;
            // 
            // picDisplay
            // 
            this.picDisplay.Location = new System.Drawing.Point(10, 35);
            this.picDisplay.Name = "picDisplay";
            this.picDisplay.Size = new System.Drawing.Size(256, 192);
            this.picDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDisplay.TabIndex = 2;
            this.picDisplay.TabStop = false;
            // 
            // frmMainWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(276, 236);
            this.Controls.Add(this.picDisplay);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(292, 275);
            this.Name = "frmMainWnd";
            this.Text = "C#81";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMainWnd_FormClosed);
            this.Load += new System.EventHandler(this.frmMainWnd_Load);
            this.Shown += new System.EventHandler(this.frmMainWnd_Shown);
            this.SizeChanged += new System.EventHandler(this.frmMainWnd_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMainWnd_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmMainWnd_KeyUp);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        public PictureBox picDisplay;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem specifyTapeDirectoryToolStripMenuItem;
        private ToolStripMenuItem resetZX81ToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem displaySizeToolStripMenuItem;
        private ToolStripMenuItem showToolbarToolStripMenuItem;
        private ToolStripMenuItem allowWritesToShadowRom816KAreaToolStripMenuItem;
        private ToolStripMenuItem zX81KeyboardToolStripMenuItem;
        private ToolStripMenuItem aboutC81ToolStripMenuItem;
        private ToolStripMenuItem hideScreenInFastModeToolStripMenuItem;
    }
}