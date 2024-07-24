namespace CMW_Electrical.OneLineTools.OneLine_Copy
{
    partial class CopySelectionReferenceForm
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.cBoxEquipSelect = new System.Windows.Forms.ComboBox();
            this.lblEquipSelect = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(225, 66);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnClick);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(144, 66);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnClick);
            // 
            // cBoxEquipSelect
            // 
            this.cBoxEquipSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBoxEquipSelect.FormattingEnabled = true;
            this.cBoxEquipSelect.Location = new System.Drawing.Point(15, 31);
            this.cBoxEquipSelect.Name = "cBoxEquipSelect";
            this.cBoxEquipSelect.Size = new System.Drawing.Size(285, 21);
            this.cBoxEquipSelect.TabIndex = 3;
            this.cBoxEquipSelect.SelectedIndexChanged += new System.EventHandler(this.SelectionChanged);
            this.cBoxEquipSelect.TextChanged += new System.EventHandler(this.SelectionChanged);
            // 
            // lblEquipSelect
            // 
            this.lblEquipSelect.AutoSize = true;
            this.lblEquipSelect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEquipSelect.Location = new System.Drawing.Point(12, 9);
            this.lblEquipSelect.Name = "lblEquipSelect";
            this.lblEquipSelect.Size = new System.Drawing.Size(230, 13);
            this.lblEquipSelect.TabIndex = 4;
            this.lblEquipSelect.Text = "Select Equipment from Model to Reference";
            // 
            // CopySelectionReferenceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 101);
            this.Controls.Add(this.lblEquipSelect);
            this.Controls.Add(this.cBoxEquipSelect);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CopySelectionReferenceForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assign Model Component";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblEquipSelect;
        public System.Windows.Forms.ComboBox cBoxEquipSelect;
    }
}