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
        public DistributionSelectionForm(List<Autodesk.Revit.DB.Electrical.DistributionSysType> elecDisSysList)
        {
            InitializeComponent();

            //add items to combobox
            string[] disSysNames = (from ds 
                                    in elecDisSysList 
                                    select ds.Name)
                                    .ToArray();

            cboxSelectDisSys.Items.AddRange(disSysNames);
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
