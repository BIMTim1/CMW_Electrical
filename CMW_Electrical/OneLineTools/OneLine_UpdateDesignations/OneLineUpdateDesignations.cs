using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLineUpdateDesignations
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLineUpdateDesignations : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //create an instance of the WindowsForm for the user to select which update method to use
            dialogSelectUpdateMethod dialogSelectUpdateMethod = new dialogSelectUpdateMethod();
            dialogSelectUpdateMethod.ShowDialog();

            //if user canceled out of WindowsForm, close tool
            if (dialogSelectUpdateMethod.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                TaskDialog.Show("User Canceled", "The tool was canceled.");
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
