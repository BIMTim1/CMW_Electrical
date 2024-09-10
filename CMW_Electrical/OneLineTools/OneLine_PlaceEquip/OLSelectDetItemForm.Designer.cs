namespace CMW_Electrical.OneLineTools.OneLine_PlaceEquip
{
    partial class OLSelectDetItemForm
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
            this.cboxDetailItemList = new System.Windows.Forms.ComboBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.cboxFamilyTypeSelection = new System.Windows.Forms.ComboBox();
            this.lblSelectType = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(357, 119);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnClick);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(276, 119);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnClick);
            // 
            // cboxDetailItemList
            // 
            this.cboxDetailItemList.FormattingEnabled = true;
            this.cboxDetailItemList.Location = new System.Drawing.Point(15, 30);
            this.cboxDetailItemList.Name = "cboxDetailItemList";
            this.cboxDetailItemList.Size = new System.Drawing.Size(417, 21);
            this.cboxDetailItemList.TabIndex = 3;
            this.cboxDetailItemList.SelectedIndexChanged += new System.EventHandler(this.ItemSelected);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(12, 9);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(130, 13);
            this.lblHeader.TabIndex = 4;
            this.lblHeader.Text = "Select Detail Item Name";
            // 
            // cboxFamilyTypeSelection
            // 
            this.cboxFamilyTypeSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxFamilyTypeSelection.FormattingEnabled = true;
            this.cboxFamilyTypeSelection.Location = new System.Drawing.Point(15, 77);
            this.cboxFamilyTypeSelection.Name = "cboxFamilyTypeSelection";
            this.cboxFamilyTypeSelection.Size = new System.Drawing.Size(242, 21);
            this.cboxFamilyTypeSelection.TabIndex = 5;
            // 
            // lblSelectType
            // 
            this.lblSelectType.AutoSize = true;
            this.lblSelectType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectType.Location = new System.Drawing.Point(12, 60);
            this.lblSelectType.Name = "lblSelectType";
            this.lblSelectType.Size = new System.Drawing.Size(125, 13);
            this.lblSelectType.TabIndex = 4;
            this.lblSelectType.Text = "Select Equipment Type";
            // 
            // OLSelectDetItemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 154);
            this.Controls.Add(this.cboxFamilyTypeSelection);
            this.Controls.Add(this.lblSelectType);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.cboxDetailItemList);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "OLSelectDetItemForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Detail Item from Schematic";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.ComboBox cboxDetailItemList;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblSelectType;
        public System.Windows.Forms.ComboBox cboxFamilyTypeSelection;
    }
}