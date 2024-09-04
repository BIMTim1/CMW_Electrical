using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCircuitsAndPlaceText
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetCircuitsAndPlaceText : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region AutodeskInfo
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //AutodeskInfo

            //collect BuiltInCategories for tool
            BuiltInCategory bicElecSys = BuiltInCategory.OST_ElectricalCircuit;
            BuiltInCategory bicSpaces = BuiltInCategory.OST_MEPSpaces;

            //collect ActiveView
            View activeView = doc.ActiveView;

            #region ViewType check
            //check if activeView is not a FloorPlan
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                errorReport = "Incorrect View Type. Change the active view to a FloorPlan view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType check

            //collect ElectricalSystems in active document
            List<ElectricalSystem> all_circuits = 
                new FilteredElementCollector(doc)
                .OfCategory(bicElecSys)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ElectricalSystem>()
                .ToList();

            #region ElectricalSystem check
            //cancel if no ElectricalSystems in active document
            if (!all_circuits.Any())
            {
                errorReport = "No Electrical Circuits in active document. The tool will now cancel.";

                return Result.Cancelled;
            }
            #endregion //ElectricalSystem check

            //get current view level name
            string lvlName = activeView.LookupParameter("Associated Level").AsString();

            //collect spaces created on the activeView level
            List<Element> lvl_spaces = 
                new FilteredElementCollector(doc)
                .OfCategory(bicSpaces)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.get_Parameter(BuiltInParameter.LEVEL_NAME).AsString() == lvlName)
                .ToList();

            #region Check Spaces on Associated Level
            //check if any Spaces exist on current view level
            if (!lvl_spaces.Any())
            {
                errorReport = "No Spaces have been created on the current level. The tool will now cancel.";

                return Result.Cancelled;
            }
            #endregion //Check Spaces on Associated Level

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Place Text of Selected Spaces");



                    trac.Commit();
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }
    }
}
