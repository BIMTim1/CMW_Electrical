using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneLineConnectAndPlace
{
    public partial class SelectEquipmentToReferenceForm : System.Windows.Forms.Form
    {
        public SelectEquipmentToReferenceForm(List<string> equipNames)
        {
            InitializeComponent();

            cboxEquipNameSelect.Items.AddRange(equipNames.ToArray());

            List<string> detailItemTypes = new List<string>(equipNames)
            {
                "ATS",
                "Bus",
                "CT Cabinet",
                "Panelboard",
                "Disconnect",
                "Motor",
                "Meter",
                "XFMR"
            };

            cboxDetailItemType.Items.AddRange(detailItemTypes.ToArray());
            cboxDetailItemType.SelectedIndex = 3;
        }

        public void RadBtnCheckChanged(object sender, EventArgs e)
        {
            if (rbtnUseEquipment.Checked)
            {
                cboxEquipNameSelect.Enabled = true;

                tboxNewEquipmentAmperage.Enabled = false;
                tboxNewEquipmentName.Enabled = false;

                if (cboxEquipNameSelect.SelectedItem != null)
                {
                    btnOK.Enabled = true;
                }
                else
                {
                    btnOK.Enabled = false;
                }
            }
            else if (rbtnDontUseEquipment.Checked)
            {
                cboxEquipNameSelect.SelectedItem = null;
                cboxEquipNameSelect.Enabled = false;

                tboxNewEquipmentAmperage.Enabled = true;
                tboxNewEquipmentName.Enabled = true;

                btnOK.Enabled = false;
            }
        }

        public void SelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }

        public void BtnClose(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
