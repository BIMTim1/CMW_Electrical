using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace MotorMOCPUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class UpdateMotorMOCP: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //define BuiltInCategory to collect
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalFixtures;

            //collect all Motors in model
            List<Element> all_motors = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.LookupParameter("Family").AsValueString().Contains("Motor"))
                .ToList();

            //check if the tool collected any Motor Elements
            //if no, close the tool.
            if (all_motors.Count == 0)
            {
                TaskDialog.Show("No Motors in Project", "There are no Motor Families placed in the Active Project. The tool will now close.");
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public bool CorrectMOCP(Element mtr, Document document)
        {
            return false;
        }
    }
}
