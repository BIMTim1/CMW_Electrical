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

namespace OneLine_Associate
{
    public partial class OneLineAssociateForm : Form
    {
        //[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        //private static extern IntPtr CreateRoundRectRgn
        //    (
        //    int nLeftRect,
        //    int nTopRect,
        //    int nRightRect,
        //    int nBottomRect,
        //    int nWidthEllipse,
        //    int nHeightEllipse
        //    );
        public OneLineAssociateForm(List<string> equipNames, string labelInput)
        {
            InitializeComponent();

            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            //set lblInstruction Text based on model input
            if (labelInput == "Panel Name - Detail")
            {
                lblInstruction.Text = "Select Detail Item";
            }
            else
            {
                lblInstruction.Text = "Select Electrical Equipment";
            }

            //update ComboBox with Electrical Equipment Names
            cBoxEquipSelection.Items.AddRange(equipNames.ToArray());

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(cBoxEquipSelection, 
                "Only Electrical Equipment not already associated to Detail Item families in the Electrical Schematic will appear in this list.");
        }

        public void Btn_Click(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void SelectedIndex_Changed(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }
    }
}
