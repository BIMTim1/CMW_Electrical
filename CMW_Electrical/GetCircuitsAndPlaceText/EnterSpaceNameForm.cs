using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.GetCircuitsAndPlaceText
{
    public partial class EnterSpaceNameForm : Form
    {
        public EnterSpaceNameForm(List<Autodesk.Revit.DB.Element> ordered_spaces)
        {
            InitializeComponent();

            this.Width = 277;

            string[] spaceInfo = (from sp 
                                  in ordered_spaces 
                                  select sp.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NAME).AsString() 
                                  + ", " 
                                  + sp.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NUMBER).AsString())
                                  .ToArray();

            listBox1.Items.AddRange(spaceInfo);
        }

        public void Button_Click(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void Text_Changed(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }

        public void Show_Spaces(object sender, EventArgs eventArgs)
        {
            bool collapsed = splitContainer1.Panel2Collapsed;
            splitContainer1.Panel2Collapsed = !collapsed;

            if (splitContainer1.Panel2Collapsed)
            {
                this.Width = 277;
            }
            else
            {
                this.Width = 567;
            }
        }
    }
}
