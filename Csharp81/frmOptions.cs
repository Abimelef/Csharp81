using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Csharp81
{
    public partial class FrmOptions : Form
    {

        public Color oldBackColour;
        public Color oldForeColour;
        public Color oldBorderColour;
        public Color oldFastmodeCol;

        private Boolean _bHideInFastMode;
        private ZX81 _zx81;
        private FrmMainWnd _frmMainWnd;

        public FrmOptions(FrmMainWnd frmMainWnd, ZX81 zx81)
        {
            _zx81 = zx81;
            _frmMainWnd = frmMainWnd;   
            InitializeComponent();
           
        }

        private void PicBackgroundCol_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.BackColor;
            if(colorDialog1.ShowDialog() == DialogResult.OK)
             
            {
                picBackgroundCol.BackColor = colorDialog1.Color;
            }
        }

        private void FrmOptions_Load(object sender, EventArgs e)
        {
            oldBackColour = Properties.Settings.Default.stgBackColour;
            oldForeColour = Properties.Settings.Default.stgForeColour;
            oldBorderColour = Properties.Settings.Default.stgBorderColour;
            oldFastmodeCol = Properties.Settings.Default.stgFastModeColour;


            picBackgroundCol.BackColor = Properties.Settings.Default.stgBackColour;
            picForegroundCol.BackColor = Properties.Settings.Default.stgForeColour;
            picBorderCol.BackColor = Properties.Settings.Default.stgBorderColour;
            picFastmodeCol.BackColor = Properties.Settings.Default.stgFastModeColour;

           
        }

        private void PicForegroundCol_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                picForegroundCol.BackColor = colorDialog1.Color;
            }
        }

        private void PicBorderCol_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.BackColor;
            if(colorDialog1.ShowDialog() == DialogResult.OK) 
            {
                picBorderCol.BackColor = colorDialog1.Color;
            }
        }

        private void PicFastmodeCol_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = this.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                picFastmodeCol.BackColor = colorDialog1.Color;
            }
        }

     

        private void BtnOK_Click(object sender, EventArgs e)
        {
           
            _zx81.backGroundColour = picBackgroundCol.BackColor;
            _zx81.foreGroundColour = picForegroundCol.BackColor;
            _zx81.borderColour = picBorderCol.BackColor;
            _zx81.fastmodeColour = picFastmodeCol.BackColor;

            Properties.Settings.Default.stgBackColour = _zx81.backGroundColour;
            Properties.Settings.Default.stgForeColour = _zx81.foreGroundColour;
            Properties.Settings.Default.stgBorderColour = _zx81.borderColour;
            Properties.Settings.Default.stgFastModeColour = _zx81.fastmodeColour;
            Properties.Settings.Default.Save();

            ColorPalette colours = _zx81.displayBitmap.Palette;
            colours.Entries[0] = _zx81.backGroundColour;
            colours.Entries[1] = _zx81.foreGroundColour;
            _zx81.displayBitmap.Palette = colours;
            _frmMainWnd.BackColor = _zx81.borderColour;

            // Reinitialise display

            _frmMainWnd.Refresh();
            this.Close();
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            _zx81.backGroundColour = picBackgroundCol.BackColor;
            _zx81.foreGroundColour = picForegroundCol.BackColor;
            _zx81.borderColour = picBorderCol.BackColor;
            _zx81.fastmodeColour = picFastmodeCol.BackColor;

            ColorPalette colours = _zx81.displayBitmap.Palette;
            colours.Entries[0] = _zx81.backGroundColour;
            colours.Entries[1] = _zx81.foreGroundColour;
            _zx81.displayBitmap.Palette = colours;
            _frmMainWnd.BackColor = _zx81.borderColour;

            // Reinitialise display

            _frmMainWnd.Refresh();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _zx81.backGroundColour = oldBackColour;
            _zx81.foreGroundColour = oldForeColour;
            _zx81.borderColour = oldBorderColour;
            _zx81.fastmodeColour = oldFastmodeCol;

            ColorPalette colours = _zx81.displayBitmap.Palette;
            colours.Entries[0] = _zx81.backGroundColour;
            colours.Entries[1] = _zx81.foreGroundColour;
            _zx81.displayBitmap.Palette = colours;
            _frmMainWnd.BackColor = _zx81.borderColour;
            _frmMainWnd.Refresh();
            this.Close();


        }
    }
}
