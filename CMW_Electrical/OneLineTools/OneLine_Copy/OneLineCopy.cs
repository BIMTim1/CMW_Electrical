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
using CMW_Electrical.OneLineTools.OneLine_Copy;

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
                    double startX = Math.Round(startPoint.X, 3);
                    double startY = Math.Round(startPoint.Y, 3);
                    double startZ = startPoint.Z;

                    double endX = Math.Round(endPoint.X, 3);
                    double endY = Math.Round(endPoint.Y, 3);
                    double endZ = endPoint.Z;

                    double transX;
                    double transY;

                    //get translation distance of X coordinate in DraftingView
                    if (startX == endX)
                    {
                        transX = 0.0;
                    }
                    else
                    {
                        if (startX < 0) //check if negative double
                        {
                            if (endX < 0) //check if a negative double
                            {
                                if (endX < startX)
                                {
                                    transX = endX - startX; //result should be a negative number
                                }
                                else
                                {
                                    transX = -(startX) + endX;
                                }
                            }
                            else //endX is a positive #
                            {
                                transX = endX - startX; //result should be a positive number
                            }
                        }
                        else //startX is a positive double
                        {
                            if (endX < 0) //check if endX is a negative double
                            {
                                transX = endX - startX;
                            }
                            else
                            {
                                if (endX < startX)
                                {
                                    transX = -(startX - endX);
                                }
                                else
                                {
                                    transX = endX - startX;
                                }
                            }
                        }
                    }


                    //get translation distance of Y coordinate in DraftingView
                    if (startY == endY)
                    {
                        transY = 0.0;
                    }
                    else
                    {
                        if (startY < 0) //check if negative double
                        {
                            if (endY < 0) //check if a negative double
                            {
                                if (endY < startY)
                                {
                                    transY = endY - startY; //result should be a negative number
                                }
                                else
                                {
                                    transY = -(startY) + endY;
                                }
                            }
                            else //endY is a positive #
                            {
                                transY = endY - startY; //result should be a positive number
                            }
                        }
                        else //startY is a positive double
                        {
                            if (endY < 0) //check if endY is a negative double
                            {
                                transY = endY - startY;
                            }
                            else
                            {
                                if (endY < startY)
                                {
                                    transY = -(startY - endY);
                                }
                                else
                                {
                                    transY = endY - startY;
                                }
                            }
                        }
                    }

                    XYZ translationDist = new XYZ(transX, transY, startZ);

                    ICollection<ElementId> copiedElemIds = ElementTransformUtils.CopyElements(doc, elementsToCopy, translationDist);

                    //launch form for user selection of equipment connection
                    List<Element> filteredEquip = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                        .ToList();

                    List<string> equipNames = (from pnl 
                                               in filteredEquip 
                                               select pnl.LookupParameter("Panel Name").AsString())
                                               .ToList();

                    CopySelectionReferenceForm copyForm = new CopySelectionReferenceForm(equipNames);
                    copyForm.ShowDialog();

                    //result if canceled by user
                    if (copyForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        TaskDialog.Show("User canceled assignment", "The selected elemnts have been copied, but not assigned to any Electrical Equipment.");
                        
                        trac.Commit();
                        return Result.Cancelled;
                    }

                    //result if not completed by user
                    Element selEquip = (from pnl 
                                        in filteredEquip 
                                        where pnl.LookupParameter("Panel Name").AsString() == copyForm.cBoxEquipSelect.Text 
                                        select pnl)
                                        .ToList()
                                        .First();

                    ElecEquipInfo selEquipInfo = new ElecEquipInfo(selEquip);

                    List<Element> copiedElems = (from id in copiedElemIds select doc.GetElement(id)).ToList();
                    Element mainDetItem = (from el in copiedElems where el.LookupParameter("Family").AsValueString().Contains("Panelboard") select el).ToList().First();

                    DetailItemInfo detItemInfo = new DetailItemInfo(mainDetItem);
                    detItemInfo.Name = selEquipInfo.Name;

                    OLEqConIdUpdateClass updateEqConId = new OLEqConIdUpdateClass();
                    updateEqConId.OneLineEqConIdValueUpdate(selEquipInfo, detItemInfo, doc);

                    doc.Regenerate();

                    copiedElemIds.Remove(mainDetItem.Id);

                    foreach (ElementId elemId in copiedElemIds)
                    {
                        Element copiedElem = doc.GetElement(elemId);

                        if (copiedElem.Category.Name == "Detail Items")
                        {
                            copiedElem.LookupParameter("EqConId").Set(selEquipInfo.EqConId);
                        }
                    }

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
