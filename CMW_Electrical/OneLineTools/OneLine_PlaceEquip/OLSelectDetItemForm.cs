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
        public OLSelectDetItemForm(List<Autodesk.Revit.DB.Element> elements, string nameRef, List<string> typeNames, string refCat)
        {
            InitializeComponent();

            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            this.Text = "Select Element to Reference";

            lblHeader.Text = "Select " + refCat + " Name";

            //collect element name information
            Autodesk.Revit.DB.BuiltInParameter bipFamily = Autodesk.Revit.DB.BuiltInParameter.ELEM_FAMILY_PARAM;
            Autodesk.Revit.DB.BuiltInParameter bipType = Autodesk.Revit.DB.BuiltInParameter.ELEM_TYPE_PARAM;

            string[] elementNames = (from el 
                                     in elements 
                                     select el.LookupParameter(nameRef).AsString() 
                                     + ", " 
                                     + el.get_Parameter(bipFamily).AsValueString() 
                                     + ": " 
                                     + el.get_Parameter(bipType).AsValueString())
                                     .ToArray();

            cboxDetailItemList.Items.AddRange(elementNames);
            cboxDetailItemList.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboxDetailItemList.AutoCompleteSource = AutoCompleteSource.CustomSource;

            AutoCompleteStringCollection autoComplete = new AutoCompleteStringCollection();

            foreach (var item in cboxDetailItemList.Items)
            {
                autoComplete.Add(item.ToString());
            }

            cboxDetailItemList.AutoCompleteCustomSource = autoComplete;

            cboxFamilyTypeSelection.Items.AddRange(typeNames.ToArray());
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

            if (compStr.Contains("E_DI_OL_"))
            {
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
            else
            {
                if (compStr.Contains("Branch"))
                {
                    cboxFamilyTypeSelection.SelectedIndex = 2;
                    cboxFamilyTypeSelection.Enabled = false;
                }
                else if (compStr.Contains("Transformer"))
                {
                    cboxFamilyTypeSelection.SelectedIndex = 3;
                    cboxFamilyTypeSelection.Enabled = false;
                }
                else if (compStr.Contains("Automatic"))
                {
                    cboxFamilyTypeSelection.SelectedIndex = 0;
                    cboxFamilyTypeSelection.Enabled = false;
                }
                else
                {
                    cboxFamilyTypeSelection.SelectedIndex = 1;
                    cboxFamilyTypeSelection.Enabled = false;
                }
            }
        }
    }
}
