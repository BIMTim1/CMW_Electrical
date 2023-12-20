using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
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
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //collect Detail Items and Electrical Equipment with an EqConId
            List<FamilyInstance> allDetailItems = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .OfClass(typeof(FamilyInstance))
                .Where(x => x.LookupParameter("EqConId").AsString() != "")
                .Cast<FamilyInstance>()
                .ToList();

            List<FamilyInstance> allElectricalEquip = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilyInstance))
                .Where(x => x.LookupParameter("EqConId").AsString() != "")
                .Cast<FamilyInstance>()
                .ToList();

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
