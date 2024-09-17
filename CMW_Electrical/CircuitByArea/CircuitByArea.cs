using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;

namespace CircuitByArea
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CircuitByArea : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            View activeView = doc.ActiveView;

            #region ViewType check
            //check ViewType
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                errorReport = "Incorrect ViewType. Change the active view to a FloorPlan view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType check

            IList<Element> selElements = new List<Element>();

            try
            {
                ISelectionFilter selFilter = new CMWElecSelectionFilter.ElecFixtureSelectionFilter();

                selElements = uidoc.Selection.PickElementsByRectangle(selFilter, "Select elements by rectangle.");
            }
            catch (OperationCanceledException ex)
            {
                errorReport = "User canceled operation.";

                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;

                return Result.Failed;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    List<Element> circuitList = new List<Element>();

                    foreach (Element ef in selElements)
                    {

                    }
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
