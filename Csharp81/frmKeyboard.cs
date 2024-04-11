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
    public partial class frmKeyboard : Form
    {
        public frmKeyboard()
        {
            InitializeComponent();
        }

        private void frmKeyboard_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size=new Size(this.Size.Width-40,this.Size.Height-63);
        }
    }
}
