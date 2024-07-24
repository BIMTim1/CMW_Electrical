using Autodesk.Revit.DB.Macros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneLineUpdateDesignations
{
    public partial class dialogSelectUpdateMethod : System.Windows.Forms.Form
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
        public dialogSelectUpdateMethod()
        {
            InitializeComponent();

            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            ToolTip useEquipToolTip = new ToolTip();
            useEquipToolTip.SetToolTip(rbtnUseEquipment, "This option is used to update all Detail Item families in the One-Line / Riser schematic that have the same EqConId value as an Electrical Equipment family located in the Revit model.");

            ToolTip useDetailItemToolTip = new ToolTip();
            useDetailItemToolTip.SetToolTip(rbtnUseDetailItems, "This option is used to update all Electrical Equipment families in the Revit model that have the same EqConId value as a Detail Item in the project's One-Line / Riser schematic.");
        }

        public void btn_Pressed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
