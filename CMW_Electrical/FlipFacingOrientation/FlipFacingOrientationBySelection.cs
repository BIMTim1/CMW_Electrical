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

namespace FlipFacingOrientation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class FlipFacingOrientationBySelection: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            List<Element> user_selection;

            try
            {
                ISelectionFilter selection_filter = new CMWElecSelectionFilter.LightingSelectionFilter();

                string selection_prompt = "Select Elements by Rectangle to Flip Host";

                user_selection = uidoc.Selection.PickElementsByRectangle(selection_filter, selection_prompt).ToList();
            }

            catch (OperationCanceledException ex)
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
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Flip Work Plane of Selected Lighting Fixtures");

                    int count = 0;

                    foreach (FamilyInstance lighting_fixture in user_selection)
                    {
                        if (lighting_fixture.CanFlipWorkPlane & lighting_fixture.IsWorkPlaneFlipped)
                        {
                            lighting_fixture.IsWorkPlaneFlipped = false;
                            count++;
                        }
                    }

                    trac.Commit();

                    TaskDialog.Show("Lighting Fixtures Modification Complete", $"{count} Lighting Fixtures have had their Host Orientation Flipped.");

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
    }
}
