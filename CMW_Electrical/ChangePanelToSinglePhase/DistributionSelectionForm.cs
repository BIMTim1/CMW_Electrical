using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.ChangePanelToSinglePhase
{
    public partial class DistributionSelectionForm : Form
    {
        public DistributionSelectionForm(List<Autodesk.Revit.DB.FamilySymbol> elecFamSymList)
        {
            InitializeComponent();

            //add items to combobox
            List<string> famInfoList = new List<string>();
            foreach (Autodesk.Revit.DB.FamilySymbol fs in elecFamSymList)
            {
                string display = fs.FamilyName + ": " + fs.Name;
                famInfoList.Add(display);
            }

            famInfoList.Sort();

            cboxSelectDisSys.Items.AddRange(famInfoList.ToArray());
        }

        public void Button_Clicked(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void Selection_Changed(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }
    }
}
