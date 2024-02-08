using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TagAlignTop
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TagAlignTop : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            try
            {
                ISelectionFilter tagFilter = new MySelectionFilter();
                //Reference selRefTag = uidoc.Selection.PickObject(ObjectType.Element, tagFilter, "Select Reference Tag to Align to");
                Reference selRefTag = uidoc.Selection.PickObject(ObjectType.Element, "Select Reference Tag to Align to"); //debug only

                Element refTag = doc.GetElement(selRefTag);

                IList<Reference> selAdjustTags = uidoc.Selection.PickObjects(ObjectType.Element, tagFilter, "Select Tags to be Aligned");

                List<Element> adjustTags = (from x in selAdjustTags select doc.GetElement(x)).ToList();

                //collect ActiveView to be used for tag references / movement
                View activeView = doc.ActiveView;

                BoundingBoxXYZ refTagBB = refTag.get_BoundingBox(activeView);
            }

            catch (OperationCanceledException ex)
            {
                TaskDialog.Show("User Canceled", "Tool canceled.");
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public class MySelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name.Contains("Tags"))
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
