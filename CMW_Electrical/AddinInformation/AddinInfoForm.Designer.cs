namespace CMW_Electrical.AddinInformation
{
    partial class cmwElecAddinInfo
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
            this.picBoxLogo = new System.Windows.Forms.PictureBox();
            this.lblVersionNum = new System.Windows.Forms.Label();
            this.lblReleaseDate = new System.Windows.Forms.Label();
            this.lblVersionNumberDisplay = new System.Windows.Forms.Label();
            this.lblReleaseDateDisplay = new System.Windows.Forms.Label();
            this.btnReportBug = new System.Windows.Forms.Button();
            this.btnBIMSite = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // picBoxLogo
            // 
            this.picBoxLogo.Location = new System.Drawing.Point(12, 12);
            this.picBoxLogo.Name = "picBoxLogo";
            this.picBoxLogo.Size = new System.Drawing.Size(260, 88);
            this.picBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBoxLogo.TabIndex = 0;
            this.picBoxLogo.TabStop = false;
            // 
            // lblVersionNum
            // 
            this.lblVersionNum.AutoSize = true;
            this.lblVersionNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersionNum.Location = new System.Drawing.Point(12, 117);
            this.lblVersionNum.Name = "lblVersionNum";
            this.lblVersionNum.Size = new System.Drawing.Size(107, 16);
            this.lblVersionNum.TabIndex = 1;
            this.lblVersionNum.Text = "Version Number:";
            // 
            // lblReleaseDate
            // 
            this.lblReleaseDate.AutoSize = true;
            this.lblReleaseDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReleaseDate.Location = new System.Drawing.Point(12, 146);
            this.lblReleaseDate.Name = "lblReleaseDate";
            this.lblReleaseDate.Size = new System.Drawing.Size(94, 16);
            this.lblReleaseDate.TabIndex = 2;
            this.lblReleaseDate.Text = "Release Date:";
            // 
            // lblVersionNumberDisplay
            // 
            this.lblVersionNumberDisplay.AutoSize = true;
            this.lblVersionNumberDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersionNumberDisplay.Location = new System.Drawing.Point(236, 117);
            this.lblVersionNumberDisplay.Name = "lblVersionNumberDisplay";
            this.lblVersionNumberDisplay.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblVersionNumberDisplay.Size = new System.Drawing.Size(36, 16);
            this.lblVersionNumberDisplay.TabIndex = 3;
            this.lblVersionNumberDisplay.Text = "x.x.x";
            // 
            // lblReleaseDateDisplay
            // 
            this.lblReleaseDateDisplay.AutoSize = true;
            this.lblReleaseDateDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReleaseDateDisplay.Location = new System.Drawing.Point(187, 146);
            this.lblReleaseDateDisplay.Name = "lblReleaseDateDisplay";
            this.lblReleaseDateDisplay.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblReleaseDateDisplay.Size = new System.Drawing.Size(85, 16);
            this.lblReleaseDateDisplay.TabIndex = 4;
            this.lblReleaseDateDisplay.Text = "March 2023";
            // 
            // btnReportBug
            // 
            this.btnReportBug.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReportBug.Location = new System.Drawing.Point(15, 176);
            this.btnReportBug.Name = "btnReportBug";
            this.btnReportBug.Size = new System.Drawing.Size(254, 23);
            this.btnReportBug.TabIndex = 5;
            this.btnReportBug.Text = "Report Bug";
            this.btnReportBug.UseVisualStyleBackColor = true;
            this.btnReportBug.Click += new System.EventHandler(this.BtnBugClicked);
            // 
            // btnBIMSite
            // 
            this.btnBIMSite.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBIMSite.Location = new System.Drawing.Point(15, 205);
            this.btnBIMSite.Name = "btnBIMSite";
            this.btnBIMSite.Size = new System.Drawing.Size(254, 23);
            this.btnBIMSite.TabIndex = 6;
            this.btnBIMSite.Text = "BIM SharePoint Site";
            this.btnBIMSite.UseVisualStyleBackColor = true;
            this.btnBIMSite.Click += new System.EventHandler(this.BtnBIMSiteClicked);
            // 
            // cmwElecAddinInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 237);
            this.Controls.Add(this.btnBIMSite);
            this.Controls.Add(this.btnReportBug);
            this.Controls.Add(this.lblReleaseDateDisplay);
            this.Controls.Add(this.lblVersionNumberDisplay);
            this.Controls.Add(this.lblReleaseDate);
            this.Controls.Add(this.lblVersionNum);
            this.Controls.Add(this.picBoxLogo);
            this.Name = "cmwElecAddinInfo";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CMW - Electrical Add-in";
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picBoxLogo;
        private System.Windows.Forms.Label lblVersionNum;
        private System.Windows.Forms.Label lblReleaseDate;
        private System.Windows.Forms.Label lblVersionNumberDisplay;
        private System.Windows.Forms.Label lblReleaseDateDisplay;
        private System.Windows.Forms.Button btnReportBug;
        private System.Windows.Forms.Button btnBIMSite;
    }
}