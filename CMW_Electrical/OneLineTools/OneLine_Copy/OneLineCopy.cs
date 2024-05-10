using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Selection;
using OLUpdateInfo;
using Autodesk.Revit.DB.Electrical;
using OneLineTools;
using CMW_Electrical;

namespace OneLineCopy
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineCopy : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //cancel tool if Current View is not a DraftingView
            if (activeView.ViewType != ViewType.DraftingView)
            {
                TaskDialog.Show("Incorrect View Type",
                    "This tool can only be run in a Drafting View. Change the current view to a Drafting View and rerun the tool.");
                return Result.Cancelled;
            }

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            ICollection<Element> selectedElems = new List<Element>();

            //prompt user to select multiple objects in a rectangular selection if no objects already selected
            if (selectedIds.Count() == 0)
            {
                try
                {
                    //ISelectionFilter selFilter = new DetailItemSelectionFilter();
                    //selectedElems = uidoc.Selection.PickElementsByRectangle(selFilter, "Select One Line elements to copy.");
                    selectedElems = uidoc.Selection.PickElementsByRectangle("Select One Line elements to copy."); //debug only
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error occurred", "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.");
                    return Result.Failed;
                }
            }
            else
            {
                foreach (ElementId elemId in selectedIds)
                {
                    Element elem = doc.GetElement(elemId);
                    selectedElems.Add(elem);
                }
            }

            XYZ startPoint = new XYZ(0, 0, 0);
            XYZ endPoint = new XYZ(0, 0, 0);
            //prompt user to select points to start for copy location
            try
            {
                startPoint = uidoc.Selection.PickPoint(ObjectSnapTypes.Midpoints
                            | ObjectSnapTypes.Endpoints
                            | ObjectSnapTypes.Intersections
                            | ObjectSnapTypes.Nearest
                            | ObjectSnapTypes.Quadrants
                            | ObjectSnapTypes.Perpendicular
                            | ObjectSnapTypes.Points,
                            "Select basepoint to copy from.");

                endPoint = uidoc.Selection.PickPoint(ObjectSnapTypes.Midpoints
                            | ObjectSnapTypes.Endpoints
                            | ObjectSnapTypes.Intersections
                            | ObjectSnapTypes.Nearest
                            | ObjectSnapTypes.Quadrants
                            | ObjectSnapTypes.Perpendicular
                            | ObjectSnapTypes.Points,
                            "Select destination point");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error occurred", "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.");
                return Result.Failed;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Copy One Line Elements");

                    ICollection<ElementId> elementsToCopy = (from elem in selectedElems select elem.Id).ToList();

                    //create translation XYZ to adjust elements
                    XYZ translationPoint = new XYZ(0, 0, 0);
                    double startX = startPoint.X;
                    double startY = startPoint.Y;
                    double startZ = startPoint.Z;

                    double endX = endPoint.X;
                    double endY = endPoint.Y;
                    double endZ = endPoint.Z;

                    double transX;

                    //if (startX < 0)
                    //{
                    //    if (endX < 0)
                    //    {
                    //        transX = -((-startX) - (-endX));
                    //    }
                    //    else
                    //    {
                    //        transX = (-startX) + startX + endX;
                    //    }
                    //}
                    //else
                    //{
                    //    if (endX < 0)
                    //    {
                    //        transX = -(startX - startX + (-endX));
                    //    }
                    //    else
                    //    {
                    //        transX = 
                    //    }
                    //}

                    ICollection<ElementId> copiedElems = ElementTransformUtils.CopyElements(doc, elementsToCopy, endPoint);

                    trac.Commit();
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error occurred", "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.");
                    return Result.Failed;
                }
            }
        }

        public class DetailItemSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Detail Items")
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
