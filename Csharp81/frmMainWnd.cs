using System.Runtime.InteropServices;

namespace Csharp81
{

    public partial class FrmMainWnd : Form
    {
        private ZX81 _zx81;
        private Z80 _z80;
        private Memory _zx81Memory;
        //replace with live checks on sizes

        public FrmMainWnd()
        {
            InitializeComponent();
        }

        private void FrmMainWnd_SizeChanged(object sender, EventArgs e)
        {
            SetSizes();
        }


        private void SetSizes()
        {
            int titleBarHeight = (this.Height - this.ClientSize.Height);

            const int innerBorder = 10;

            int newPanelWidth = this.ClientSize.Width - 2 * innerBorder;
            int newPanelHeight = (int)(newPanelWidth * 0.75);
            picDisplay.Size = new Size(newPanelWidth, newPanelHeight);

            if (toolStrip1.Visible)
            {
                this.Height = newPanelHeight + titleBarHeight + menuStrip1.Height + toolStrip1.Height + 2 * innerBorder;
                picDisplay.Location = new Point(innerBorder, menuStrip1.Height + toolStrip1.Height + innerBorder);
            }
            else
            {
                this.Height = newPanelHeight + titleBarHeight + menuStrip1.Height + 2 * innerBorder;
                picDisplay.Location = new Point(innerBorder, menuStrip1.Height + innerBorder);
            }
        }



        private void FrmMainWnd_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            toolStrip1.Visible = !Properties.Settings.Default.stgHideTB;
            showToolbarToolStripMenuItem.Checked = !Properties.Settings.Default.stgHideTB;
            hideScreenInFastModeToolStripMenuItem.Checked = Properties.Settings.Default.stgHideScreenInFastMode;
            this.Size = Properties.Settings.Default.stgFormSize;

        }



        private void FrmMainWnd_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Alt)
            {
                _zx81.doKey(true, e);
            }
        }

        //need separate event handler to capture Enter key
        //and Shift Enter
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                _zx81.doEnterKey(false);
                return true;
            }
            else if (keyData == (Keys.Enter | Keys.Shift))
            {
                _zx81.doEnterKey(true);
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void FrmMainWnd_KeyUp(object sender, KeyEventArgs e)
        {
            _zx81.doKey(false, e);
        }

        private void FrmMainWnd_Shown(object sender, EventArgs e)
        {

            _zx81Memory = new();
            _zx81 = new ZX81(_zx81Memory, picDisplay, this);
            _z80 = new Z80(_zx81Memory, _zx81);
            _z80.Execute();
        }

        private void ZX81KeyboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmKeyboard _frm = new();
            _frm.Show();
        }

        private void FrmMainWnd_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.stgFormSize = this.Size;
            Properties.Settings.Default.Save();
            Environment.Exit(1);
        }

        private void SpecifyTapeDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog
            {
                Description = "Specify Tape Directory"
            };

            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK)
            {
                _zx81.sTapeDir = fbd.SelectedPath;
            }

            if (_zx81.sTapeDir != "")
            {
                Properties.Settings.Default.stgTapeDir = _zx81.sTapeDir;
                Properties.Settings.Default.Save();
            }

        }

        private void ResetZX81ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _z80.Z80Reset();
        }

        private void AboutC81ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout _frmAbout = new();
            _frmAbout.Show();
        }

        private void DisplaySizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmOptions _frmOptions = new(this, _zx81);
            _frmOptions.Show();

        }

        private void ShowToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showToolbarToolStripMenuItem.Checked = !showToolbarToolStripMenuItem.Checked;

            if (showToolbarToolStripMenuItem.Checked)
            {
                toolStrip1.Visible = true;
                _zx81.bHideTB = false;
                Properties.Settings.Default.stgHideTB = false;

            }
            else
            {
                toolStrip1.Visible = false;
                _zx81.bHideTB = true;
                Properties.Settings.Default.stgHideTB = true;

            }
            Properties.Settings.Default.Save();
            FrmMainWnd_SizeChanged(sender, e);
            this.Refresh();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //Properties.Settings.Default.stgFormSize = this.Size;
            //Properties.Settings.Default.Save(); 
            Environment.Exit(0);
        }

        private void HideScreenInFastModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideScreenInFastModeToolStripMenuItem.Checked = !hideScreenInFastModeToolStripMenuItem.Checked;
            Properties.Settings.Default.stgHideScreenInFastMode = hideScreenInFastModeToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
            _zx81.bHideInFastMode = hideScreenInFastModeToolStripMenuItem.Checked;
        }



        private void tsbReset_Click(object sender, EventArgs e)
        {
            _z80.Z80Reset();
        }

        private void tsbMemoCalc_Click(object sender, EventArgs e)
        {
            frmMemoCalc _frmMemoCalc = new frmMemoCalc(_zx81, _z80, _zx81Memory);
            _frmMemoCalc.ShowDialog();

        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _zx81.SimulateKeyPresses("O16389^.100#A#");  //POKE 16389,100 Newline NEW Newline.


        }

        private void loadMazogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _zx81.SimulateKeyPresses("J^PMAZOGS^P#");
        }

        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Collections.Specialized.StringCollection myCol = new System.Collections.Specialized.StringCollection();

            // Add a range of elements from an array to the end of the StringCollection.
            String[] myArr = new String[] { "RED", "orange", "yellow", "RED", "green", "blue", "RED", "indigo", "violet", "RED","abc","bnm","jkl" };
            myCol.AddRange(myArr);

            Properties.Settings.Default.stgMacros = myCol;
            Properties.Settings.Default.stgMacroDesriptions  = myCol;
            Properties.Settings.Default.Save();

        }

        private void stgsToArrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.stgMacros.Clear();
            Properties.Settings.Default.stgMacroDesriptions.Clear();
            Properties.Settings.Default.Save();
        }

        private void macrosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMacros frmMacros = new frmMacros();
            frmMacros.ShowDialog();
        }

        private void macrosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmMacros frmMacros = new frmMacros();
            frmMacros.ShowDialog();
            if (frmMacros.returnMacroString != "")
            {
                _zx81.SimulateKeyPresses(frmMacros.returnMacroString);
            }
        }
    }
}
