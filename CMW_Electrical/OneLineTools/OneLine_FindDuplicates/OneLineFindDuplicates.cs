using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneLineTools;

namespace OneLine_FindDuplicates
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineFindDuplicates : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport =
                    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Failed;
            }

            //check if activeView is not a DraftingView
            if (activeView.ViewType != ViewType.DraftingView)
            {
                errorReport = "Incorrect active view. Change your Active View to your One-Line Drafting View and rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //begin transaction and wrap in a using statement
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Find Duplicates");

                    List<FamilyInstance> all_detailItems = 
                        new FilteredElementCollector(doc, activeView.Id)
                        .OfCategory(BuiltInCategory.OST_DetailComponents)
                        .ToElements()
                        .Cast<FamilyInstance>()
                        .Where(x => !x.LookupParameter("Family").AsValueString().Contains("Circuit"))
                        .Where(x=> !x.LookupParameter("Family").AsValueString().Contains("Feeder"))
                        .Where(x=>x.LookupParameter("EqConId").AsString() != null || x.LookupParameter("EqConId").AsString() != "")
                        .ToList();

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
        }
    }
}
