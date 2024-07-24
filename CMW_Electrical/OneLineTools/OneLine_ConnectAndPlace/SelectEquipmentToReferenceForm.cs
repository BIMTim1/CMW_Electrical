using Autodesk.Revit.DB.Macros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneLineConnectAndPlace
{
    public partial class SelectEquipmentToReferenceForm : System.Windows.Forms.Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
            (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );
        public SelectEquipmentToReferenceForm(List<string> equipNames)
        {
            InitializeComponent();

            //create rounded edges of Form
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

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

            List<string> voltages = new List<string>()
            {
                "208",
                "240",
                "480"
            };

            cboxNewEquipmentVoltage.Items.AddRange(voltages.ToArray());
            cboxNewEquipmentVoltage.SelectedIndex = 0;
        }

        public void RadBtnCheckChanged(object sender, EventArgs e)
        {
            if (rbtnUseEquipment.Checked)
            {
                cboxEquipNameSelect.Enabled = true;

                cboxNewEquipmentVoltage.Enabled = false;
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

                cboxNewEquipmentVoltage.Enabled = true;
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
