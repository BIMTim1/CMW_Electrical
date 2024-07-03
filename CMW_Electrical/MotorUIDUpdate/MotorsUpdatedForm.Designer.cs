namespace CMW_Electrical.MotorUIDUpdate
{
    partial class MotorsUpdatedForm
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
            this.listViewMotors = new System.Windows.Forms.ListView();
            this.lblMotorsUpdated = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewMotors
            // 
            this.listViewMotors.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewMotors.GridLines = true;
            this.listViewMotors.HideSelection = false;
            this.listViewMotors.Location = new System.Drawing.Point(12, 38);
            this.listViewMotors.Name = "listViewMotors";
            this.listViewMotors.Size = new System.Drawing.Size(315, 200);
            this.listViewMotors.TabIndex = 0;
            this.listViewMotors.UseCompatibleStateImageBehavior = false;
            this.listViewMotors.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView_DrawColumnHeader);
            this.listViewMotors.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView_DrawItem);
            // 
            // lblMotorsUpdated
            // 
            this.lblMotorsUpdated.AutoSize = true;
            this.lblMotorsUpdated.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotorsUpdated.Location = new System.Drawing.Point(12, 9);
            this.lblMotorsUpdated.Name = "lblMotorsUpdated";
            this.lblMotorsUpdated.Size = new System.Drawing.Size(156, 15);
            this.lblMotorsUpdated.TabIndex = 1;
            this.lblMotorsUpdated.Text = "List of Updated Motors:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(252, 248);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.Btn_Click);
            // 
            // MotorsUpdatedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 277);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMotorsUpdated);
            this.Controls.Add(this.listViewMotors);
            this.Name = "MotorsUpdatedForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Motors Updated";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewMotors;
        private System.Windows.Forms.Label lblMotorsUpdated;
        private System.Windows.Forms.Button btnOK;
    }
}