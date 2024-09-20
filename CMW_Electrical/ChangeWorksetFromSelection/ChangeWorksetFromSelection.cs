using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeWorksetFromSelection
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ChangeWorksetFromSelection : IExternalCommand
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
            //check active view ViewType
            if (activeView.ViewType != ViewType.FloorPlan && activeView.ViewType != ViewType.ThreeD && activeView.ViewType != ViewType.Section)
            {
                errorReport = "Incorrect ViewType. Change the active view to a FloorPlan, 3D, or Section view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType check

            #region Selection
            //check if element already selected
            ICollection<ElementId> colElemIds = uidoc.Selection.GetElementIds();
            Element selElem = null;

            if (!colElemIds.Any() || colElemIds.Count() > 1)
            {
                try
                {
                    selElem = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select an Element to change the active Workset."));
                }
                catch (OperationCanceledException ex)
                {
                    //errorReport = "User canceled operation.";
                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;
                    return Result.Failed;
                }
            }
            else
            {
                selElem = doc.GetElement(colElemIds.First());
            }
            #endregion //Selection

            try
            {
                //get WorksetId of selected element
                WorksetId wId = new WorksetId(selElem.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).AsInteger());

                //set active Workset
                doc.GetWorksetTable().SetActiveWorksetId(wId);

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
