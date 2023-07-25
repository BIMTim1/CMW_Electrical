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
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            List<Element> user_selection;

            try
            {
                ISelectionFilter selection_filter = new LightingSelectionFilter();

                string selection_prompt = "Select Elements by Rectangle to Flip Host";

                user_selection = uidoc.Selection.PickElementsByRectangle(selection_filter, selection_prompt).ToList();
            }

            catch (OperationCanceledException ex)
            {
                TaskDialog.Show("User Canceled Operation", "The tool has been canceled. Rerun the tool to start the process again.");
                
                return Result.Failed;
            }

            Transaction trac = new Transaction(doc);

            trac.Start("Flip Work Plane of Selected Lighting Fixtures");

            int count = 0;

            foreach (FamilyInstance lighting_fixture in user_selection)
            {
                if (lighting_fixture.CanFlipWorkPlane)
                {
                    if (lighting_fixture.IsWorkPlaneFlipped)
                    {
                        lighting_fixture.IsWorkPlaneFlipped = false;
                        count++;
                    }
                    else
                    {
                        lighting_fixture.IsWorkPlaneFlipped = true;
                        count++;
                    }
                }
            }

            trac.Commit();

            TaskDialog.Show("Lighting Fixtures Modification Complete", $"{count} Lighting Fixtures have had their Host Orientation Flipped.");
            
            return Result.Succeeded;
        }

        public class LightingSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Lighting Fixtures")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
    }
}
