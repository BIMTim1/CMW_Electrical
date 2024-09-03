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
    public partial class SelectNewSourceForm : Form
    {
        public SelectNewSourceForm(List<Autodesk.Revit.DB.FamilyInstance> elemList, string formName)
        {
            InitializeComponent();

            this.Text = formName;

            lblSelectNewSource.Text = formName + ":";

            //collect string information from elemList
            string[] equipInfo = (from fi 
                                  in elemList 
                                  select fi.Symbol.FamilyName + ": " + fi.Symbol.Name + ", " + fi.LookupParameter("Panel Name").AsString())
                                  .ToArray();

            //add items to ComboBox
            cboxSelectSource.Items.AddRange(equipInfo);
        }

        public void Button_Click(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        public void Selection_Changed(object sender, EventArgs eventArgs)
        {
            btnOK.Enabled = true;
        }
    }
}
