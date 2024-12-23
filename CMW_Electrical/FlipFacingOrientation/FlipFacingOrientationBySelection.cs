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
using CMW_Electrical;
using System.Windows.Media.Converters;

namespace FlipFacingOrientation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class FlipFacingOrientationBySelection: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;
            #endregion //Autodesk Info

            #region ActiveView check
            //check for correct activeView
            if (activeView.ViewType != ViewType.FloorPlan && activeView.ViewType != ViewType.CeilingPlan && activeView.ViewType != ViewType.Section)
            {
                errorReport = "Incorrect view type. Change your active view to a Floor Plan, Ceiling Plan, or Section view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ActiveView check

            #region Pre-Selection Check
            bool userSelect = false;
            List<FamilyInstance> user_selection = new List<FamilyInstance>();

            //ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();
            List<FamilyInstance> selectedElementIds = (from e 
                                                       in uidoc.Selection.GetElementIds() 
                                                       select doc.GetElement(e))
                                                       .Cast<FamilyInstance>()
                                                       .ToList();

            if (!selectedElementIds.Any())
            {
                userSelect = true;
            }
            else
            {
                List<FamilyInstance> filtered_selected =
                CheckRotatedHostPlane(
                    selectedElementIds,
                    doc);

                if (!filtered_selected.Any())
                {
                    userSelect = true;
                }
                else
                {
                    user_selection = filtered_selected;
                }
            }
            #endregion //Pre-Selection Check

            #region User Selection
            if (userSelect)
            {
                try
                {
                    ISelectionFilter selection_filter = new CMWElecSelectionFilter.ElectricalCategoryFilter();

                    string selection_prompt = "Select Elements by Rectangle to Flip Host";

                    List<FamilyInstance> selection = uidoc.Selection.PickElementsByRectangle(
                        selection_filter, 
                        selection_prompt)
                        .Cast<FamilyInstance>()
                        .ToList();

                    user_selection = CheckRotatedHostPlane(selection, doc);

                    if (!user_selection.Any())
                    {
                        throw new OperationCanceledException();
                    }
                }

                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    TaskDialog.Show("User Canceled Operation", "The tool has been canceled. Rerun the tool to start the process again.");

                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;
                    elementSet.Insert(doc.ActiveView);

                    return Result.Failed;
                }
            }
            #endregion //User Selection

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Flip Work Plane of Selected Electrical Elements");

                    int count = 0;

                    foreach(FamilyInstance famInst in user_selection)
                    {
                        famInst.IsWorkPlaneFlipped = false;
                        count++;
                    }

                    trac.Commit();

                    TaskDialog results = new TaskDialog("CMW-Elec - Results")
                    {
                        TitleAutoPrefix = false,
                        CommonButtons = TaskDialogCommonButtons.Ok,
                        MainInstruction = "Results:",
                        MainContent = $"{count} family instances have had their Host Orientation Flipped."
                    };

                    results.Show();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    foreach (Element lf in user_selection)
                    {
                        elementSet.Insert(lf);
                    }

                    return Result.Failed;
                }
            }
        }

        #region CheckRotatedHostPlane method
        public List<FamilyInstance> CheckRotatedHostPlane(List<FamilyInstance> selectedObjects, Document document)
        {
            List<FamilyInstance> outputList = new List<FamilyInstance>();

            foreach (FamilyInstance familyInstance in selectedObjects)
            {
                if (familyInstance.CanFlipWorkPlane & familyInstance.IsWorkPlaneFlipped)
                {
                    outputList.Add(familyInstance);
                }
            }

            return outputList;
        }
        #endregion //CheckRotatedHostPlane method
    }
}
