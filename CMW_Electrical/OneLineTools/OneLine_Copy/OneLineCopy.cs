﻿using Autodesk.Revit.Attributes;
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
            #region Autodesk Info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            View activeView = doc.ActiveView;

            #region EqConId check
            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";
                return Result.Failed;
            }
            #endregion //EqConId check

            #region ActiveView type check
            //cancel tool if Current View is not a DraftingView
            if (activeView.ViewType != ViewType.DraftingView)
            {
                errorReport = "This tool can only be run in a Drafting View. Change the current view to a Drafting View and rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }
            #endregion //ActiveView type check

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            ICollection<Element> selectedElems = new List<Element>();

            ISelectionFilter selFilter = new CMWElecSelectionFilter.DetailItemSelectionFilter();

            //prompt user to select multiple objects in a rectangular selection if no objects already selected
            if (selectedIds.Count() == 0)
            {
                try
                {
                    selectedElems = uidoc.Selection.PickElementsByRectangle(selFilter, "Select One Line elements to copy.");
                    //selectedElems = uidoc.Selection.PickElementsByRectangle("Select One Line elements to copy."); //debug only
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    errorReport = "User canceled operation.";
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
                    trac.Start("CMWElec-Copy One Line Elements");

                    ICollection<ElementId> elementsToCopy = (from elem in selectedElems select elem.Id).ToList();

                    //get translation XYZ
                    XYZ translationDist = GetTranslationXYZ(startPoint, endPoint);

                    ICollection<ElementId> copiedElemIds = ElementTransformUtils.CopyElements(doc, elementsToCopy, translationDist);

                    //launch form for user selection of equipment connection
                    List<Element> filteredEquip = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                        .ToList();

                    filteredEquip = filteredEquip.OrderBy(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString()).ToList();

                    //List<string> equipNames = (from pnl 
                    //                           in filteredEquip 
                    //                           select pnl.LookupParameter("Panel Name").AsString())
                    //                           .ToList();

                    CopySelectionReferenceForm copyForm = new CopySelectionReferenceForm(filteredEquip);
                    copyForm.ShowDialog();

                    //result if canceled by user
                    if (copyForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        //clear EqConId values of copied elements
                        foreach (ElementId eid in copiedElemIds)
                        {
                            Element elem = doc.GetElement(eid);
                            elem.LookupParameter("EqConId").Set("");
                            elem.LookupParameter("EqConId Connection Source").Set("");
                            elementSet.Insert(elem);

                            //check for Panel Name - Detail, replace value
                            Parameter nameParam = elem.LookupParameter("Panel Name - Detail");
                            if (nameParam == null)
                            {
                                continue;
                            }

                            nameParam.Set("PNL XX");
                        }
                        TaskDialog results = new TaskDialog("CMW-Elec - Results")
                        {
                            TitleAutoPrefix = false,
                            MainInstruction = "Results:",
                            MainContent = "The selected elements have been copied, but not assigned to any Electrical Equipment.",
                            CommonButtons = TaskDialogCommonButtons.Ok
                        };

                        results.Show();
                        //TaskDialog.Show("User canceled assignment", "The selected elements have been copied, but not assigned to any Electrical Equipment.");

                        trac.Commit();
                        return Result.Succeeded;
                    }

                    //result if not completed by user
                    //Element selEquip = (from pnl 
                    //                    in filteredEquip 
                    //                    where pnl.LookupParameter("Panel Name").AsString() == copyForm.cBoxEquipSelect.Text 
                    //                    select pnl)
                    //                    .ToList()
                    //                    .First();

                    ElecEquipInfo selEquipInfo = new ElecEquipInfo(filteredEquip[copyForm.cBoxEquipSelect.SelectedIndex]);

                    List<Element> copiedElems = (from id in copiedElemIds select doc.GetElement(id)).ToList();
                    Element mainDetItem = (from el in copiedElems where !el.LookupParameter("Family").AsValueString().Contains("Feeder") select el).ToList().First();

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

                    //create circuit from selected Detail Items
                    try
                    {
                        Reference selSourceDetItem = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select a Source Detail Item");
                        //Reference selSourceDetItem = uidoc.Selection.PickObject(ObjectType.Element, "Select a Source Detail Item"); //debug only

                        FamilyInstance sourceDetItem = doc.GetElement(selSourceDetItem) as FamilyInstance;

                        ElectricalSystem createdCircuit = new CreateEquipmentCircuit()
                            .CreateEquipCircuit(
                            doc, 
                            sourceDetItem, 
                            mainDetItem as FamilyInstance);
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                    {
                        TaskDialog results = new TaskDialog("CMW-Elec - Results")
                        {
                            TitleAutoPrefix = false,
                            MainInstruction = "Results:",
                            MainContent = "The selected elements have been copied and Electrical Equipment associated, but no circuit was created.",
                            CommonButtons = TaskDialogCommonButtons.Ok
                        };

                        results.Show();
                    }

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

        public XYZ GetTranslationXYZ(XYZ startPoint, XYZ endPoint)
        {
            //create translation XYZ to adjust elements
            XYZ translationDist = new XYZ(0, 0, 0);
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

            translationDist = new XYZ(transX, transY, startZ);

            return translationDist;
        }
    }
}
