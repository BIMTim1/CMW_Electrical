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

namespace CMW_Electrical.OneLineTools.OneLine_Copy
{
    public partial class CopySelectionReferenceForm : Form
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
        public CopySelectionReferenceForm(List<Autodesk.Revit.DB.Element> elementList)
        {
            InitializeComponent();

            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            string[] elemArray = (from el 
                                  in elementList 
                                  select el.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() 
                                  + ", " 
                                  + el.LookupParameter("Family").AsValueString() 
                                  + ": " 
                                  + el.LookupParameter("Type").AsValueString())
                                  .ToArray();

            //assign elements to ComboBox
            cBoxEquipSelect.Items.AddRange(elemArray);

            //create autocomplete information
            cBoxEquipSelect.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cBoxEquipSelect.AutoCompleteSource = AutoCompleteSource.CustomSource;

            AutoCompleteStringCollection autoComplete = new AutoCompleteStringCollection();

            foreach (var item in cBoxEquipSelect.Items)
            {
                autoComplete.Add(item.ToString());
            }

            cBoxEquipSelect.AutoCompleteCustomSource = autoComplete;
        }

        public void BtnClick(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void SelectionChanged(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }
    }
}
