using System.Data;

namespace Csharp81
{
    public partial class frmMacros : Form
    {

        private List<string> macros;
        private List<string> macroDescriptions;
        private bool editingMacro = false;
        private int editingMacroNumber = 0;
        public string returnMacroString = "";
        public frmMacros()
        {
            InitializeComponent();
        }

        private void frmMacros_Load(object sender, EventArgs e)
        {
            fillMacroList();
        }

        private void fillMacroList()
        {
            if(Properties.Settings.Default.stgMacros is null)
            {
                Properties.Settings.Default.stgMacros = new System.Collections.Specialized.StringCollection();  
            }

            if (Properties.Settings.Default.stgMacroDescriptions is null)
            {
                Properties.Settings.Default.stgMacroDescriptions = new System.Collections.Specialized.StringCollection();
            }


            if (Properties.Settings.Default.stgMacros.Count>0)
            {
                lvMacros.Items.Clear();
                macros = Properties.Settings.Default.stgMacros.Cast<string>().ToList();
                macroDescriptions = Properties.Settings.Default.stgMacroDescriptions.Cast<string>().ToList();

                for (int n = 0; n < macros.Count; n++)
                {
                    ListViewItem item = new ListViewItem(new[] { macros[n], macroDescriptions[n] });
                    lvMacros.Items.Add(item);
                }

            }
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (tbMacro.Text != "")
            {
                if (editingMacro == false)
                {

                    Properties.Settings.Default.stgMacros.Add(tbMacro.Text);
                    if (tbMacroDescription.Text != "")
                    {
                        Properties.Settings.Default.stgMacroDescriptions.Add(tbMacroDescription.Text);
                    }
                    else
                    {
                        Properties.Settings.Default.stgMacroDescriptions.Add("");
                    }
                }
                else
                {
                    Properties.Settings.Default.stgMacros[editingMacroNumber] = tbMacro.Text;
                    if (tbMacroDescription.Text != "")
                    {
                        Properties.Settings.Default.stgMacroDescriptions[editingMacroNumber] = tbMacroDescription.Text;
                    }
                    else
                    {
                        Properties.Settings.Default.stgMacroDescriptions[editingMacroNumber] = "";
                    }
                    editButtonOnOff(false);
                    editingMacro = false;
                }


                Properties.Settings.Default.Save();

                fillMacroList();

                tbMacro.Clear();
                tbMacroDescription.Clear();

            }
        }

        private void btnDeleteMacro_Click(object sender, EventArgs e)
        {
            if (lvMacros.SelectedItems.Count > 0)
            {
                int n = lvMacros.SelectedItems[0].Index;
                macros.RemoveAt(n);
                macroDescriptions.RemoveAt(n);
                updateStoredMacrosinSettings();
                lvMacros.Items.Clear();
                fillMacroList();
            }
        }


        private void updateStoredMacrosinSettings()
        {
            Properties.Settings.Default.stgMacros.Clear();
            Properties.Settings.Default.stgMacroDescriptions.Clear();
            Properties.Settings.Default.stgMacros.AddRange(macros.ToArray());
            Properties.Settings.Default.stgMacroDescriptions.AddRange(macroDescriptions.ToArray());
            Properties.Settings.Default.Save();
        }


        private void btnRunMacro_Click(object sender, EventArgs e)
        {
            if (lvMacros.SelectedItems.Count > 0)
            {
                int n = lvMacros.SelectedItems[0].Index;
                returnMacroString = macros[n];
                this.Close();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvMacros.SelectedItems.Count > 0)
            {
                int n = lvMacros.SelectedItems[0].Index;
                if (lvMacros.SelectedItems[0] != null)
                {
                    tbMacro.Text = macros[n];
                    tbMacroDescription.Text = macroDescriptions[n];
                    editingMacro = true;
                    editingMacroNumber = n;
                    tbMacro.Focus();
                    int position = tbMacro.Text.Length;
                    tbMacro.Select(position, position);
                    editButtonOnOff(true);
                }
            }
        }

        private void tbMacro_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
               updateEditedMacro();
            }
        }

        private void updateEditedMacro()
        {
            if (editingMacro && tbMacro.Text != "")
            {
                macros[editingMacroNumber] = tbMacro.Text;
                macroDescriptions[editingMacroNumber] = tbMacroDescription.Text;
                updateStoredMacrosinSettings();
                lvMacros.Items.Clear();
                fillMacroList();
                editingMacro = false;
                tbMacro.Text = "";
                tbMacroDescription.Text = "";
                btnCancelEdit.Visible = false;
                editButtonOnOff(false);
            }
        }

        private void tbMacroDescription_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                updateEditedMacro();

            }
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            tbMacro.Text = "";
            tbMacroDescription.Text = "";
            editButtonOnOff(false);
            lvMacros.Focus();
    
        }

        private void editButtonOnOff(bool editOn)
        {
                btnCancelEdit.Visible = editOn;
                btnRunMacro.Visible = !editOn;
                btnDeleteMacro.Visible = !editOn;
            if (editOn)
            {
                btnAdd.Text = "Update";
            }
            else
            {
                btnAdd.Text = "Add";
            }
        }
    }
}
