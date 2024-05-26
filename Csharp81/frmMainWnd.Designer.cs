namespace Csharp81
{
    partial class FrmMainWnd
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMainWnd));
            menuStrip1 = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            specifyTapeDirectoryToolStripMenuItem = new ToolStripMenuItem();
            resetZX81ToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            displaySizeToolStripMenuItem = new ToolStripMenuItem();
            showToolbarToolStripMenuItem = new ToolStripMenuItem();
            allowWritesToShadowRom816KAreaToolStripMenuItem = new ToolStripMenuItem();
            hideScreenInFastModeToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem5 = new ToolStripMenuItem();
            macrosToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            zX81KeyboardToolStripMenuItem = new ToolStripMenuItem();
            aboutC81ToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            picDisplay = new PictureBox();
            toolStrip1 = new ToolStrip();
            tsbReset = new ToolStripButton();
            tsbMemoCalc = new ToolStripButton();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picDisplay).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem3, toolStripMenuItem5, toolStripMenuItem4, toolStripMenuItem2 });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(286, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { specifyTapeDirectoryToolStripMenuItem, resetZX81ToolStripMenuItem, exitToolStripMenuItem });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(37, 20);
            toolStripMenuItem1.Text = "&File";
            // 
            // specifyTapeDirectoryToolStripMenuItem
            // 
            specifyTapeDirectoryToolStripMenuItem.Name = "specifyTapeDirectoryToolStripMenuItem";
            specifyTapeDirectoryToolStripMenuItem.Size = new Size(190, 22);
            specifyTapeDirectoryToolStripMenuItem.Text = "&Specify Tape Directory";
            specifyTapeDirectoryToolStripMenuItem.Click += SpecifyTapeDirectoryToolStripMenuItem_Click;
            // 
            // resetZX81ToolStripMenuItem
            // 
            resetZX81ToolStripMenuItem.Name = "resetZX81ToolStripMenuItem";
            resetZX81ToolStripMenuItem.Size = new Size(190, 22);
            resetZX81ToolStripMenuItem.Text = "&Reset ZX81";
            resetZX81ToolStripMenuItem.Click += ResetZX81ToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(190, 22);
            exitToolStripMenuItem.Text = "&Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[] { displaySizeToolStripMenuItem, showToolbarToolStripMenuItem, allowWritesToShadowRom816KAreaToolStripMenuItem, hideScreenInFastModeToolStripMenuItem });
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(61, 20);
            toolStripMenuItem3.Text = "&Options";
            // 
            // displaySizeToolStripMenuItem
            // 
            displaySizeToolStripMenuItem.Name = "displaySizeToolStripMenuItem";
            displaySizeToolStripMenuItem.Size = new Size(293, 22);
            displaySizeToolStripMenuItem.Text = "Colours...";
            displaySizeToolStripMenuItem.Click += DisplaySizeToolStripMenuItem_Click;
            // 
            // showToolbarToolStripMenuItem
            // 
            showToolbarToolStripMenuItem.Name = "showToolbarToolStripMenuItem";
            showToolbarToolStripMenuItem.Size = new Size(293, 22);
            showToolbarToolStripMenuItem.Text = "&Show Toolbar";
            showToolbarToolStripMenuItem.Click += ShowToolbarToolStripMenuItem_Click;
            // 
            // allowWritesToShadowRom816KAreaToolStripMenuItem
            // 
            allowWritesToShadowRom816KAreaToolStripMenuItem.Name = "allowWritesToShadowRom816KAreaToolStripMenuItem";
            allowWritesToShadowRom816KAreaToolStripMenuItem.Size = new Size(293, 22);
            allowWritesToShadowRom816KAreaToolStripMenuItem.Text = "&Allow Writes to Shadow Rom (8-16K area)";
            // 
            // hideScreenInFastModeToolStripMenuItem
            // 
            hideScreenInFastModeToolStripMenuItem.Name = "hideScreenInFastModeToolStripMenuItem";
            hideScreenInFastModeToolStripMenuItem.Size = new Size(293, 22);
            hideScreenInFastModeToolStripMenuItem.Text = "&Hide Screen in Fast Mode ";
            hideScreenInFastModeToolStripMenuItem.Click += HideScreenInFastModeToolStripMenuItem_Click;
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.DropDownItems.AddRange(new ToolStripItem[] { macrosToolStripMenuItem1 });
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(46, 20);
            toolStripMenuItem5.Text = "Tools";
            // 
            // macrosToolStripMenuItem1
            // 
            macrosToolStripMenuItem1.Name = "macrosToolStripMenuItem1";
            macrosToolStripMenuItem1.Size = new Size(113, 22);
            macrosToolStripMenuItem1.Text = "Macros";
            macrosToolStripMenuItem1.Click += macrosToolStripMenuItem1_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.DropDownItems.AddRange(new ToolStripItem[] { zX81KeyboardToolStripMenuItem, aboutC81ToolStripMenuItem });
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(44, 20);
            toolStripMenuItem4.Text = "&Help";
            // 
            // zX81KeyboardToolStripMenuItem
            // 
            zX81KeyboardToolStripMenuItem.Name = "zX81KeyboardToolStripMenuItem";
            zX81KeyboardToolStripMenuItem.Size = new Size(180, 22);
            zX81KeyboardToolStripMenuItem.Text = "&ZX81 Keyboard";
            zX81KeyboardToolStripMenuItem.Click += ZX81KeyboardToolStripMenuItem_Click;
            // 
            // aboutC81ToolStripMenuItem
            // 
            aboutC81ToolStripMenuItem.Name = "aboutC81ToolStripMenuItem";
            aboutC81ToolStripMenuItem.Size = new Size(180, 22);
            aboutC81ToolStripMenuItem.Text = "&About C#81";
            aboutC81ToolStripMenuItem.Click += AboutC81ToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(12, 20);
            // 
            // picDisplay
            // 
            picDisplay.Location = new Point(32, 67);
            picDisplay.Name = "picDisplay";
            picDisplay.Size = new Size(179, 115);
            picDisplay.SizeMode = PictureBoxSizeMode.StretchImage;
            picDisplay.TabIndex = 2;
            picDisplay.TabStop = false;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbReset, tsbMemoCalc });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(286, 31);
            toolStrip1.TabIndex = 3;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbReset
            // 
            tsbReset.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbReset.Image = (Image)resources.GetObject("tsbReset.Image");
            tsbReset.ImageTransparentColor = Color.Magenta;
            tsbReset.Name = "tsbReset";
            tsbReset.Size = new Size(28, 28);
            tsbReset.Text = "ToolStripButton1";
            tsbReset.ToolTipText = "Reset Z80";
            tsbReset.Click += tsbReset_Click;
            // 
            // tsbMemoCalc
            // 
            tsbMemoCalc.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbMemoCalc.Image = (Image)resources.GetObject("tsbMemoCalc.Image");
            tsbMemoCalc.ImageTransparentColor = Color.Magenta;
            tsbMemoCalc.Name = "tsbMemoCalc";
            tsbMemoCalc.Size = new Size(28, 28);
            tsbMemoCalc.Text = "ToolStripButton1";
            tsbMemoCalc.Click += tsbMemoCalc_Click;
            // 
            // FrmMainWnd
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            ClientSize = new Size(286, 222);
            Controls.Add(toolStrip1);
            Controls.Add(picDisplay);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(213, 197);
            Name = "FrmMainWnd";
            Text = "C#81";
            FormClosed += FrmMainWnd_FormClosed;
            Load += FrmMainWnd_Load;
            Shown += FrmMainWnd_Shown;
            SizeChanged += FrmMainWnd_SizeChanged;
            KeyDown += FrmMainWnd_KeyDown;
            KeyUp += FrmMainWnd_KeyUp;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picDisplay).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
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
        private ToolStrip toolStrip1;
        internal ToolStripButton tsbReset;
        internal ToolStripButton tsbMemoCalc;
        private ToolStripMenuItem macrosToolStripMenuItem1;
    }
}