﻿namespace CMW_Electrical.AddinInformation
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLogo)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picBoxLogo
            // 
            this.picBoxLogo.Image = global::CMW_Electrical.Properties.Resources.CMTALogo;
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
            this.lblVersionNumberDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersionNumberDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersionNumberDisplay.Location = new System.Drawing.Point(11, 0);
            this.lblVersionNumberDisplay.Name = "lblVersionNumberDisplay";
            this.lblVersionNumberDisplay.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.lblVersionNumberDisplay.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblVersionNumberDisplay.Size = new System.Drawing.Size(130, 28);
            this.lblVersionNumberDisplay.TabIndex = 3;
            this.lblVersionNumberDisplay.Text = "x.x.x";
            this.lblVersionNumberDisplay.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblReleaseDateDisplay
            // 
            this.lblReleaseDateDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReleaseDateDisplay.Location = new System.Drawing.Point(11, 28);
            this.lblReleaseDateDisplay.Name = "lblReleaseDateDisplay";
            this.lblReleaseDateDisplay.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblReleaseDateDisplay.Size = new System.Drawing.Size(130, 16);
            this.lblReleaseDateDisplay.TabIndex = 4;
            this.lblReleaseDateDisplay.Text = "March 2025";
            this.lblReleaseDateDisplay.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            this.btnReportBug.Click += new System.EventHandler(this.BtnClicked);
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
            this.btnBIMSite.Click += new System.EventHandler(this.BtnClicked);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.lblVersionNumberDisplay);
            this.flowLayoutPanel1.Controls.Add(this.lblReleaseDateDisplay);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(125, 118);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(144, 53);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // cmwElecAddinInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 237);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.btnBIMSite);
            this.Controls.Add(this.btnReportBug);
            this.Controls.Add(this.lblReleaseDate);
            this.Controls.Add(this.lblVersionNum);
            this.Controls.Add(this.picBoxLogo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "cmwElecAddinInfo";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CMW - Electrical Add-in";
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLogo)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
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
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}