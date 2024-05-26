namespace Csharp81
{
    partial class frmMacros
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
            btnRunMacro = new Button();
            btnDeleteMacro = new Button();
            lvMacros = new ListView();
            chKeypresses = new ColumnHeader();
            chDescription = new ColumnHeader();
            btnAdd = new Button();
            tbMacro = new TextBox();
            tbMacroDescription = new TextBox();
            btnEdit = new Button();
            btnCancelEdit = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // btnRunMacro
            // 
            btnRunMacro.Location = new Point(502, 12);
            btnRunMacro.Name = "btnRunMacro";
            btnRunMacro.Size = new Size(75, 23);
            btnRunMacro.TabIndex = 1;
            btnRunMacro.Text = "Run";
            btnRunMacro.UseVisualStyleBackColor = true;
            btnRunMacro.Click += btnRunMacro_Click;
            // 
            // btnDeleteMacro
            // 
            btnDeleteMacro.Location = new Point(502, 115);
            btnDeleteMacro.Name = "btnDeleteMacro";
            btnDeleteMacro.Size = new Size(75, 23);
            btnDeleteMacro.TabIndex = 2;
            btnDeleteMacro.Text = "Delete";
            btnDeleteMacro.UseVisualStyleBackColor = true;
            btnDeleteMacro.Click += btnDeleteMacro_Click;
            // 
            // lvMacros
            // 
            lvMacros.Columns.AddRange(new ColumnHeader[] { chKeypresses, chDescription });
            lvMacros.Location = new Point(12, 12);
            lvMacros.MultiSelect = false;
            lvMacros.Name = "lvMacros";
            lvMacros.Size = new Size(472, 221);
            lvMacros.TabIndex = 3;
            lvMacros.UseCompatibleStateImageBehavior = false;
            lvMacros.View = View.Details;
            // 
            // chKeypresses
            // 
            chKeypresses.Text = "Keypresses";
            chKeypresses.Width = 150;
            // 
            // chDescription
            // 
            chDescription.Text = "Description";
            chDescription.Width = 250;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(502, 238);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // tbMacro
            // 
            tbMacro.Location = new Point(12, 239);
            tbMacro.MaxLength = 100;
            tbMacro.Name = "tbMacro";
            tbMacro.Size = new Size(148, 23);
            tbMacro.TabIndex = 5;
            tbMacro.KeyDown += tbMacro_KeyDown;
            // 
            // tbMacroDescription
            // 
            tbMacroDescription.Location = new Point(166, 239);
            tbMacroDescription.MaxLength = 200;
            tbMacroDescription.Name = "tbMacroDescription";
            tbMacroDescription.Size = new Size(318, 23);
            tbMacroDescription.TabIndex = 6;
            tbMacroDescription.KeyDown += tbMacroDescription_KeyDown;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(504, 142);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 23);
            btnEdit.TabIndex = 7;
            btnEdit.Text = "Edit";
            btnEdit.TextImageRelation = TextImageRelation.TextBeforeImage;
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnCancelEdit
            // 
            btnCancelEdit.Location = new Point(504, 171);
            btnCancelEdit.Name = "btnCancelEdit";
            btnCancelEdit.Size = new Size(75, 23);
            btnCancelEdit.TabIndex = 8;
            btnCancelEdit.Text = "Cancel Edit";
            btnCancelEdit.UseVisualStyleBackColor = true;
            btnCancelEdit.Visible = false;
            btnCancelEdit.Click += btnCancelEdit_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 265);
            label1.Name = "label1";
            label1.Size = new Size(218, 15);
            label1.TabIndex = 9;
            label1.Text = "Use ^ for SHIFT and # for NEWLINE keys";
            // 
            // frmMacros
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(642, 305);
            Controls.Add(label1);
            Controls.Add(btnCancelEdit);
            Controls.Add(btnEdit);
            Controls.Add(tbMacroDescription);
            Controls.Add(tbMacro);
            Controls.Add(btnAdd);
            Controls.Add(lvMacros);
            Controls.Add(btnDeleteMacro);
            Controls.Add(btnRunMacro);
            Name = "frmMacros";
            Text = "Macros";
            Load += frmMacros_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnRunMacro;
        private Button btnDeleteMacro;
        private ListView lvMacros;
        private Button btnAdd;
        private TextBox tbMacro;
        private ColumnHeader chKeypresses;
        private ColumnHeader chDescription;
        private TextBox tbMacroDescription;
        private Button btnEdit;
        private Button btnCancelEdit;
        private Label label1;
    }
}