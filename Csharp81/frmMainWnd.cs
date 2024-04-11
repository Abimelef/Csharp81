using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Csharp81
{
    public partial class frmMainWnd : Form
    {
        private ZX81 _zx81;
        private Z80 _z80;
    
        public frmMainWnd()
        {
            InitializeComponent();
    
        }

        private void frmMainWnd_SizeChanged(object sender, EventArgs e)
        {

            int TitlebarHeight = (this.Height - this.ClientSize.Height);
            int Borders = (this.Width-this.ClientSize.Width);
            int NewPanelWidth; 
            int NewPanelHeight;
       

            if (toolStrip1.Visible)
            {
                NewPanelWidth = this.Width - 36;
                NewPanelHeight = (int)(NewPanelWidth * 0.75);
                this.Height = NewPanelHeight + 83+24;
                picDisplay.Size = new Size(NewPanelWidth, NewPanelHeight);
                picDisplay.Location = new Point(10, 35+24);
            }
            else
            {
                NewPanelWidth = this.Width - 36;
                NewPanelHeight = (int)(NewPanelWidth * 0.75);
                this.Height = NewPanelHeight + 83;
                picDisplay.Size = new Size(NewPanelWidth, NewPanelHeight);
                picDisplay.Location = new Point(10, 35);
            }

            Properties.Settings.Default.stgFormSize = this.Size;
            Properties.Settings.Default.Save();

        }

        private void frmMainWnd_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;

            toolStrip1.Visible = !Properties.Settings.Default.stgHideTB;
            showToolbarToolStripMenuItem.Checked = !Properties.Settings.Default.stgHideTB;
            hideScreenInFastModeToolStripMenuItem.Checked = Properties.Settings.Default.stgHideScreenInFastMode;
            this.Size = Properties.Settings.Default.stgFormSize;
            frmMainWnd_SizeChanged(sender, e);
        }

     //   private void button1_Click(object sender, EventArgs e)
     /// <summary>
     ///   {
     /// </summary>
      //      int a = 1;
            
     //   }

        private void frmMainWnd_KeyDown(object sender, KeyEventArgs e)
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
            else if (keyData == (Keys.Enter|Keys.Shift))
           {
                _zx81.doEnterKey(true);
                return true;
            }
            else 
            { 
                return base.ProcessCmdKey(ref msg, keyData);
            }
      }

        private void frmMainWnd_KeyUp(object sender, KeyEventArgs e)
        {
            _zx81.doKey(false, e);
        }

        private void frmMainWnd_Shown(object sender, EventArgs e)
        {
            this.Refresh();
            KeyPresses keys = new KeyPresses();
            Memory memory = new Memory();
            _zx81 = new ZX81(memory, picDisplay, keys, this);
            _z80 = new Z80(memory, keys, _zx81);

            

            _z80.Execute();
        }

        private void zX81KeyboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmKeyboard _frm = new frmKeyboard();
            _frm.Show();
        }

        private void frmMainWnd_FormClosed(object sender, FormClosedEventArgs e)
        {
   
            Environment.Exit(1);
        }

        private void specifyTapeDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void resetZX81ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _z80.Z80Reset();
        }

        private void aboutC81ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout _frmAbout = new frmAbout   ();
            _frmAbout.Show();
        }

        private void displaySizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmOptions _frmAbout = new frmOptions(this,_zx81);
            _frmAbout.Show();

        }

        private void showToolbarToolStripMenuItem_Click(object sender, EventArgs e)
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
            frmMainWnd_SizeChanged(sender, e);
            this.Refresh();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void hideScreenInFastModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideScreenInFastModeToolStripMenuItem.Checked = !hideScreenInFastModeToolStripMenuItem.Checked; 
            Properties.Settings.Default.stgHideScreenInFastMode = hideScreenInFastModeToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
            _zx81.bHideInFastMode = hideScreenInFastModeToolStripMenuItem.Checked;
        }
    }
}
