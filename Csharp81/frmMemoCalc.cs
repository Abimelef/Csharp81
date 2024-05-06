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
   

    public partial class frmMemoCalc : Form
    {
        private ZX81 _zx81;
        private Memory _mem;
        private Z80 _z80;


        public frmMemoCalc(ZX81 zx81,Z80 z80, Memory mem)
        {
            InitializeComponent();
            _zx81 = zx81;
            _mem = mem;
            _z80= z80;  
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

            if (rbPageOnly.Checked)
            {
                _zx81.LoadMemoCalcROM();
            }
            else if (rbPageAndStart.Checked)
            {
                _zx81.LoadMemoCalcROM();
                if (_mem.Peekb(13824) == 24)
                {
                    _z80.SetPC(13824);
                }
            }
            else
            {
                _zx81.CopyROM();
            }

            this.Close();




        }
    }
}
