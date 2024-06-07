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

namespace CMW_Electrical.OneLineTools.OneLine_PlaceEquip
{
    public partial class OLSelectDetItemForm : Form
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
        public OLSelectDetItemForm(List<string> detailItemNames)
        {
            InitializeComponent();

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            cboxDetailItemList.Items.AddRange(detailItemNames.ToArray());

            //create objects for cbox
            List<string> equipTypeNames = new List<string>()
            {
                "Branch Panelboard",
                "Transformer-Dry Type",
                "Utility Transformer",
                "Automatic Transfer Switch",
                "Distribution Panelboard",
                "Switchboard"
            };

            cboxFamilyTypeSelection.Items.AddRange(equipTypeNames.ToArray());
            cboxFamilyTypeSelection.SelectedIndex = 0;
        }

        public void BtnClick(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void ItemSelected(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;

            string compStr = cboxDetailItemList.SelectedItem.ToString();//.Split(' ')[1];

            if (compStr.Contains("Panelboard"))
            {
                cboxFamilyTypeSelection.SelectedIndex = 0;
                cboxFamilyTypeSelection.Enabled = false;
            }
            else if (compStr.Contains("Bus"))
            {
                cboxFamilyTypeSelection.SelectedIndex = 4;
                cboxFamilyTypeSelection.Enabled = true;
            }
            else if (compStr.Contains("XFMR"))
            {
                cboxFamilyTypeSelection.SelectedIndex = 1;
                cboxFamilyTypeSelection.Enabled = true;
            }
            else
            {
                cboxFamilyTypeSelection.SelectedIndex = 3;
                cboxFamilyTypeSelection.Enabled = false;
            }
        }
    }
}
