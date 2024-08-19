namespace CMW_Electrical.OneLineTools.OneLine_Remove
{
    partial class OneLineRemoveForm
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
            this.radBtnRemove = new System.Windows.Forms.RadioButton();
            this.radBtnKeep = new System.Windows.Forms.RadioButton();
            this.lblUserSelection = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(242, 89);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.Btn_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(161, 89);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.Btn_Click);
            // 
            // radBtnRemove
            // 
            this.radBtnRemove.AutoSize = true;
            this.radBtnRemove.Checked = true;
            this.radBtnRemove.Location = new System.Drawing.Point(32, 35);
            this.radBtnRemove.Name = "radBtnRemove";
            this.radBtnRemove.Size = new System.Drawing.Size(166, 17);
            this.radBtnRemove.TabIndex = 2;
            this.radBtnRemove.TabStop = true;
            this.radBtnRemove.Text = "Remove Associated Elements";
            this.radBtnRemove.UseVisualStyleBackColor = true;
            // 
            // radBtnKeep
            // 
            this.radBtnKeep.AutoSize = true;
            this.radBtnKeep.Location = new System.Drawing.Point(32, 58);
            this.radBtnKeep.Name = "radBtnKeep";
            this.radBtnKeep.Size = new System.Drawing.Size(234, 17);
            this.radBtnKeep.TabIndex = 3;
            this.radBtnKeep.Text = "Keep Associated Elements and Unassociate";
            this.radBtnKeep.UseVisualStyleBackColor = true;
            // 
            // lblUserSelection
            // 
            this.lblUserSelection.AutoSize = true;
            this.lblUserSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserSelection.Location = new System.Drawing.Point(12, 9);
            this.lblUserSelection.Name = "lblUserSelection";
            this.lblUserSelection.Size = new System.Drawing.Size(206, 13);
            this.lblUserSelection.TabIndex = 4;
            this.lblUserSelection.Text = "Select Results of Element Removal";
            // 
            // OneLineRemoveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 124);
            this.Controls.Add(this.lblUserSelection);
            this.Controls.Add(this.radBtnKeep);
            this.Controls.Add(this.radBtnRemove);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "OneLineRemoveForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Associated Element Results";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblUserSelection;
        public System.Windows.Forms.RadioButton radBtnRemove;
        public System.Windows.Forms.RadioButton radBtnKeep;
    }
}