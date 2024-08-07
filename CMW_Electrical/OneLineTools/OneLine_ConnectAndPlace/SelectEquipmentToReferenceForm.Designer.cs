﻿namespace OneLineConnectAndPlace
{
    partial class SelectEquipmentToReferenceForm
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
            this.rbtnDontUseEquipment = new System.Windows.Forms.RadioButton();
            this.cboxEquipNameSelect = new System.Windows.Forms.ComboBox();
            this.tboxNewEquipmentName = new System.Windows.Forms.TextBox();
            this.lblNewEquipName = new System.Windows.Forms.Label();
            this.lblNewEquipmentVoltage = new System.Windows.Forms.Label();
            this.lblEquipTypeSelect = new System.Windows.Forms.Label();
            this.cboxDetailItemType = new System.Windows.Forms.ComboBox();
            this.cboxNewEquipmentVoltage = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(330, 273);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnClose);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(249, 273);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnClose);
            // 
            // rbtnUseEquipment
            // 
            this.rbtnUseEquipment.AutoSize = true;
            this.rbtnUseEquipment.Checked = true;
            this.rbtnUseEquipment.Location = new System.Drawing.Point(12, 38);
            this.rbtnUseEquipment.Name = "rbtnUseEquipment";
            this.rbtnUseEquipment.Size = new System.Drawing.Size(97, 17);
            this.rbtnUseEquipment.TabIndex = 2;
            this.rbtnUseEquipment.TabStop = true;
            this.rbtnUseEquipment.Text = "Use Equipment";
            this.rbtnUseEquipment.UseVisualStyleBackColor = true;
            this.rbtnUseEquipment.CheckedChanged += new System.EventHandler(this.RadBtnCheckChanged);
            // 
            // rbtnDontUseEquipment
            // 
            this.rbtnDontUseEquipment.AutoSize = true;
            this.rbtnDontUseEquipment.Location = new System.Drawing.Point(12, 110);
            this.rbtnDontUseEquipment.Name = "rbtnDontUseEquipment";
            this.rbtnDontUseEquipment.Size = new System.Drawing.Size(134, 17);
            this.rbtnDontUseEquipment.TabIndex = 3;
            this.rbtnDontUseEquipment.Text = "Create New Reference";
            this.rbtnDontUseEquipment.UseVisualStyleBackColor = true;
            this.rbtnDontUseEquipment.CheckedChanged += new System.EventHandler(this.RadBtnCheckChanged);
            // 
            // cboxEquipNameSelect
            // 
            this.cboxEquipNameSelect.FormattingEnabled = true;
            this.cboxEquipNameSelect.Location = new System.Drawing.Point(45, 61);
            this.cboxEquipNameSelect.Name = "cboxEquipNameSelect";
            this.cboxEquipNameSelect.Size = new System.Drawing.Size(221, 21);
            this.cboxEquipNameSelect.TabIndex = 4;
            this.cboxEquipNameSelect.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
            // 
            // tboxNewEquipmentName
            // 
            this.tboxNewEquipmentName.Enabled = false;
            this.tboxNewEquipmentName.Location = new System.Drawing.Point(199, 188);
            this.tboxNewEquipmentName.Name = "tboxNewEquipmentName";
            this.tboxNewEquipmentName.Size = new System.Drawing.Size(81, 20);
            this.tboxNewEquipmentName.TabIndex = 5;
            // 
            // lblNewEquipName
            // 
            this.lblNewEquipName.AutoSize = true;
            this.lblNewEquipName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewEquipName.Location = new System.Drawing.Point(38, 191);
            this.lblNewEquipName.Name = "lblNewEquipName";
            this.lblNewEquipName.Size = new System.Drawing.Size(131, 13);
            this.lblNewEquipName.TabIndex = 6;
            this.lblNewEquipName.Text = "New Equipment Name";
            // 
            // lblNewEquipmentVoltage
            // 
            this.lblNewEquipmentVoltage.AutoSize = true;
            this.lblNewEquipmentVoltage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewEquipmentVoltage.Location = new System.Drawing.Point(38, 228);
            this.lblNewEquipmentVoltage.Name = "lblNewEquipmentVoltage";
            this.lblNewEquipmentVoltage.Size = new System.Drawing.Size(142, 13);
            this.lblNewEquipmentVoltage.TabIndex = 7;
            this.lblNewEquipmentVoltage.Text = "New Equipment Voltage";
            // 
            // lblEquipTypeSelect
            // 
            this.lblEquipTypeSelect.AutoSize = true;
            this.lblEquipTypeSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEquipTypeSelect.Location = new System.Drawing.Point(38, 155);
            this.lblEquipTypeSelect.Name = "lblEquipTypeSelect";
            this.lblEquipTypeSelect.Size = new System.Drawing.Size(100, 13);
            this.lblEquipTypeSelect.TabIndex = 9;
            this.lblEquipTypeSelect.Text = "Detail Item Type";
            // 
            // cboxDetailItemType
            // 
            this.cboxDetailItemType.Enabled = false;
            this.cboxDetailItemType.FormattingEnabled = true;
            this.cboxDetailItemType.Location = new System.Drawing.Point(199, 152);
            this.cboxDetailItemType.Name = "cboxDetailItemType";
            this.cboxDetailItemType.Size = new System.Drawing.Size(201, 21);
            this.cboxDetailItemType.TabIndex = 10;
            this.cboxDetailItemType.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
            // 
            // cboxNewEquipmentVoltage
            // 
            this.cboxNewEquipmentVoltage.Enabled = false;
            this.cboxNewEquipmentVoltage.FormattingEnabled = true;
            this.cboxNewEquipmentVoltage.Location = new System.Drawing.Point(199, 225);
            this.cboxNewEquipmentVoltage.Name = "cboxNewEquipmentVoltage";
            this.cboxNewEquipmentVoltage.Size = new System.Drawing.Size(81, 21);
            this.cboxNewEquipmentVoltage.TabIndex = 11;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(149)))), ((int)(((byte)(69)))));
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(417, 26);
            this.panel1.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Select Equipment to Place in One-Line";
            // 
            // SelectEquipmentToReferenceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 308);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cboxNewEquipmentVoltage);
            this.Controls.Add(this.cboxDetailItemType);
            this.Controls.Add(this.lblEquipTypeSelect);
            this.Controls.Add(this.lblNewEquipmentVoltage);
            this.Controls.Add(this.lblNewEquipName);
            this.Controls.Add(this.tboxNewEquipmentName);
            this.Controls.Add(this.cboxEquipNameSelect);
            this.Controls.Add(this.rbtnDontUseEquipment);
            this.Controls.Add(this.rbtnUseEquipment);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SelectEquipmentToReferenceForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Equipment to Place in One-Line";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblNewEquipName;
        private System.Windows.Forms.Label lblNewEquipmentVoltage;
        public System.Windows.Forms.RadioButton rbtnUseEquipment;
        public System.Windows.Forms.RadioButton rbtnDontUseEquipment;
        public System.Windows.Forms.ComboBox cboxEquipNameSelect;
        public System.Windows.Forms.TextBox tboxNewEquipmentName;
        private System.Windows.Forms.Label lblEquipTypeSelect;
        public System.Windows.Forms.ComboBox cboxDetailItemType;
        public System.Windows.Forms.ComboBox cboxNewEquipmentVoltage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
    }
}