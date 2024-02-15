namespace OneLineUpdateDesignations
{
    partial class dialogSelectUpdateMethod
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
            this.rbtnUseEquipment = new System.Windows.Forms.RadioButton();
            this.lblUserSelectReference = new System.Windows.Forms.Label();
            this.rbtnUseDetailItems = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(285, 138);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btn_Pressed);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(204, 138);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btn_Pressed);
            // 
            // rbtnUseEquipment
            // 
            this.rbtnUseEquipment.AutoSize = true;
            this.rbtnUseEquipment.Checked = true;
            this.rbtnUseEquipment.Location = new System.Drawing.Point(38, 80);
            this.rbtnUseEquipment.Name = "rbtnUseEquipment";
            this.rbtnUseEquipment.Size = new System.Drawing.Size(143, 17);
            this.rbtnUseEquipment.TabIndex = 2;
            this.rbtnUseEquipment.TabStop = true;
            this.rbtnUseEquipment.Text = "Use Electrical Equipment";
            this.rbtnUseEquipment.UseVisualStyleBackColor = true;
            // 
            // lblUserSelectReference
            // 
            this.lblUserSelectReference.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserSelectReference.Location = new System.Drawing.Point(12, 37);
            this.lblUserSelectReference.Name = "lblUserSelectReference";
            this.lblUserSelectReference.Size = new System.Drawing.Size(315, 30);
            this.lblUserSelectReference.TabIndex = 3;
            this.lblUserSelectReference.Text = "Select One of the Options Below to Determine How to Update Equipment Information";
            // 
            // rbtnUseDetailItems
            // 
            this.rbtnUseDetailItems.AutoSize = true;
            this.rbtnUseDetailItems.Location = new System.Drawing.Point(38, 103);
            this.rbtnUseDetailItems.Name = "rbtnUseDetailItems";
            this.rbtnUseDetailItems.Size = new System.Drawing.Size(155, 17);
            this.rbtnUseDetailItems.TabIndex = 4;
            this.rbtnUseDetailItems.Text = "Use Schematic Detail Items";
            this.rbtnUseDetailItems.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(149)))), ((int)(((byte)(69)))));
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(372, 26);
            this.panel1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select Update Method";
            // 
            // dialogSelectUpdateMethod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 173);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.rbtnUseDetailItems);
            this.Controls.Add(this.lblUserSelectReference);
            this.Controls.Add(this.rbtnUseEquipment);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "dialogSelectUpdateMethod";
            this.ShowIcon = false;
            this.Text = "Select Update Method";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblUserSelectReference;
        public System.Windows.Forms.RadioButton rbtnUseEquipment;
        public System.Windows.Forms.RadioButton rbtnUseDetailItems;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
    }
}