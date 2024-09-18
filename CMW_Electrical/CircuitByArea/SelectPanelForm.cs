using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.CircuitByArea
{
    public partial class SelectPanelForm : Form
    {
        public SelectPanelForm(List<Autodesk.Revit.DB.Element> panelList)
        {
            InitializeComponent();

            //collect ElectricalEquipment information
            string[] formInput = (from eq 
                                  in panelList 
                                  select eq.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() 
                                  + ", " 
                                  + eq.LookupParameter("Family").AsValueString() 
                                  + ": " 
                                  + eq.LookupParameter("Type").AsValueString())
                                  .ToArray();

            comboBox1.Items.AddRange(formInput);

            //set combobox customsource
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

            AutoCompleteStringCollection autoComplete = new AutoCompleteStringCollection();

            foreach (string eqVal in formInput)
            {
                autoComplete.Add(eqVal);
            }

            comboBox1.AutoCompleteCustomSource = autoComplete;
        }

        public void Button_Clicked(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void SelectIndex_Changed(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }
    }
}
