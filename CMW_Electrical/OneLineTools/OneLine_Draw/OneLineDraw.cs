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

namespace OneLineDraw
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineDraw : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Failed;
            }

            if (activeView.ViewType != ViewType.DraftingView)
            {
                errorReport = "This tool can only be run in a Drafting View. Change the current view to a Drafting View and rerun the tool.";

                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //collect E_DI_Feeder-Line Based Detail Item
            FamilySymbol feederLine = (new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsElementType()
                .ToElements()
                .Where(x => x.LookupParameter("Family Name").AsString() == "E_DI_Feeder-Line Based")
                .ToList())
                .First()
                as FamilySymbol;

            if (feederLine == null)
            {
                return Result.Cancelled;
            }

            //create transaction and begin operation
            using (Transaction trac = new Transaction(doc))
            {
                trac.Start("Draw Feeder Lines");

                bool run = true;

                List<XYZ> points = new List<XYZ>();
                XYZ firstPoint = new XYZ();
                List<FamilyInstance> newFamInstances = new List<FamilyInstance>();
                FamilyInstance tempLine = null;

                while (run)
                {
                    try
                    {
                        XYZ point = uidoc.Selection.PickPoint(ObjectSnapTypes.Midpoints
                            | ObjectSnapTypes.Endpoints
                            | ObjectSnapTypes.Intersections
                            | ObjectSnapTypes.Nearest
                            | ObjectSnapTypes.Quadrants 
                            | ObjectSnapTypes.Perpendicular 
                            | ObjectSnapTypes.Points,
                            "Pick Next Point to Draw Feeder Lines");

                        points.Add(point);

                        if (points.Count() == 1)
                        {
                            XYZ tempPoint = new XYZ(points[0].X, points[0].Y + 0.1, points[0].Z);
                            Line tempCurve = Line.CreateBound(points[0], tempPoint);
                            tempLine = doc.Create.NewFamilyInstance(tempCurve, feederLine, activeView);

                            firstPoint = points[0];

                            doc.Regenerate();
                        }

                        if (points.Count() == 2)
                        {
                            Line curve = Line.CreateBound(points[0], points[1]);

                            FamilyInstance newFeederLine = doc.Create.NewFamilyInstance(curve, feederLine, activeView); //create feeder line
                            newFamInstances.Add(newFeederLine);

                            doc.Regenerate();

                            points.RemoveAt(0); //update list to only contain the last point
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                    {
                        run = false;

                        if (points.Count() == 0)
                        {
                            errorReport = "User canceled operation.";

                            return Result.Cancelled;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorReport = ex.Message;

                        return Result.Failed;
                    }
                }
                //code to prompt user to update assignment of feeders?
                if (tempLine != null)
                {
                    doc.Delete(tempLine.Id);
                }

                //prompt user to select downstream equipment reference
                try
                {
                    ISelectionFilter selFilter = new CMWElecSelectionFilter.DetailItemSelectionFilter();

                    Reference sourceDetailItemRef = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select source equipment for feeder reference.");
                    //Reference sourceDetailItemRef = uidoc.Selection.PickObject(ObjectType.Element, "Select source equipment for feeder reference."); //debug only

                    Reference fedToDetailItem = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select downstream equipment for feeder reference.");
                    //Reference fedToDetailItem = uidoc.Selection.PickObject(ObjectType.Element, "Select downstream equipment for feeder reference."); //debug only

                    FamilyInstance sourceDetailItem = doc.GetElement(sourceDetailItemRef) as FamilyInstance;

                    bool deleteFirst = false;

                    //check if source equipment is bus and create E_DI_OL_Circuit Breaker Family Instance
                    if (sourceDetailItem.Name.Contains("Bus"))
                    {
                        FamilySymbol cbSymbol = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_DetailComponents)
                            .OfClass(typeof(FamilySymbol))
                            .Where(x => x.Name.Contains("Circuit"))
                            .Cast<FamilySymbol>()
                            .First();

                        FamilyInstance circuitBreaker = doc.Create.NewFamilyInstance(firstPoint, cbSymbol, activeView);
                        newFamInstances.Add(circuitBreaker);

                        doc.Regenerate();

                        BoundingBoxXYZ cbBoundingBox = circuitBreaker.get_BoundingBox(activeView);

                        XYZ startPoint = new XYZ(firstPoint.X, cbBoundingBox.Min.Y, firstPoint.Z);
                        XYZ endPoint = new XYZ(firstPoint.X, newFamInstances[0].get_BoundingBox(activeView).Min.Y, firstPoint.Z);

                        Line replaceCurve = Line.CreateBound(startPoint, endPoint);

                        FamilyInstance newLine = doc.Create.NewFamilyInstance(replaceCurve, feederLine, activeView);
                        newFamInstances.Add(newLine);

                        doc.Regenerate();

                        deleteFirst = true;
                    }

                    DetailItemInfo selItem = new DetailItemInfo(doc.GetElement(fedToDetailItem))
                    {
                        EqConIdConnectedSource = sourceDetailItem.LookupParameter("EqConId").AsString()
                    };

                    //set EqConId value of created Feeder Lines
                    foreach (FamilyInstance fam in newFamInstances)
                    {
                        fam.LookupParameter("EqConId").Set(selItem.EqConId);
                    }

                    if (deleteFirst)
                    {
                        doc.Delete(newFamInstances[0].Id);

                        doc.Regenerate();
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    //TaskDialog.Show("User canceled", 
                    //    "User canceled the selection operation. Feeder lines were created but not assigned to an Equipment reference.");
                    errorReport = "User canceled the selection operation. Feeder lines were created but not assigned to an Equipment reference.";
                }
                catch (Exception ex)
                {
                    //error occurred
                    errorReport = ex.Message;
                    return Result.Failed;
                }

                trac.Commit();
                return Result.Succeeded;
            }
        }
    }
}
