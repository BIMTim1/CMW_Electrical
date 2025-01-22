using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;
using CMW_Electrical.ArrayCopy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayCopy
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ArrayCopy : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            #region ViewType check
            View activeView = doc.ActiveView;
            List<ViewType> approvedViewTypes = new List<ViewType>()
            {
                ViewType.FloorPlan, ViewType.CeilingPlan
            };

            if (!approvedViewTypes.Contains(activeView.ViewType))
            {
                elementSet.Insert(activeView);
                errorReport = "Incorrect View Type. Change the active view to a Floor Plan or Reflected Ceiling Plan and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType check

            #region User Selection
            Element selElem = null;

            try
            {
                ISelectionFilter selFilter = new CMWElecSelectionFilter.LightingSelectionFilter();

                selElem = doc.GetElement(
                    uidoc.Selection.PickObject(ObjectType.Element,
                    selFilter,
                    "Select Lighting Fixture instance to array."));
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                errorReport = "User canceled operation. The tool will now cancel.";

                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;

                return Result.Failed;
            }
            #endregion //User Selection

            #region FamilyInstance variables
            FamilyInstance refInst = selElem as FamilyInstance;

            //collect location / orientation data of selected FamilyInstance
            FamilySymbol familySymbol = refInst.Symbol;
            //FamilyPlacementType placementType = familySymbol.Family.FamilyPlacementType;
            Reference hostFace = refInst.HostFace;

            if (hostFace == null)
            {
                hostFace = (refInst.Host as ReferencePlane).GetReference();

                if (hostFace == null)
                {
                    errorReport = "Selected Element doesn't have a host plane. Rehost the selected Lighting Fixture and rerun the tool.";
                    return Result.Cancelled;
                }
            }
            #endregion //FamilyInstance variables

            #region User Array Settings Selection
            ArrayData arrData = new ArrayData();

            ArrayInfoWindow window = new ArrayInfoWindow(arrData);
            window.ShowDialog();

            if (window.DialogResult == false)
            {
                errorReport = "User canceled the array settings operation. The tool will now cancel.";

                return Result.Cancelled;
            }
            #endregion //User Array Settings Selection

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    LocationPoint lp = refInst.Location as LocationPoint;

                    List<XYZ> definedPoints = GetUserDefinedPoints(lp.Point, window);
                    XYZ handOrientation = refInst.HandOrientation;

                    ElementId scheduleLevel = refInst.get_Parameter(
                        BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM)
                        .AsElementId();

                    //cancel tool it no points in list
                    if (!definedPoints.Any())
                    {
                        errorReport = "Quantities were left blank. No changes were made.";

                        return Result.Cancelled;
                    }

                    trac.Start("CMW-Elec - Array Lighting Fixtures");

                    foreach (XYZ point in definedPoints)
                    {
                        FamilyInstance newFamInst = doc.Create.NewFamilyInstance(hostFace, point, handOrientation, familySymbol);

                        newFamInst.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM).Set(scheduleLevel);
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

        public List<XYZ> GetUserDefinedPoints(XYZ referencePoint, ArrayInfoWindow arrayWindow)
        {
            List<XYZ> userDefinedPoints = new List<XYZ>();

            ArrayData arrayData = arrayWindow.DataContext as ArrayData;
            int.TryParse(arrayData.X, out int xquantity);
            int.TryParse(arrayData.Y, out int yquantity);

            double.TryParse(arrayData.XDist, out double xdistance);
            double.TryParse(arrayData.YDist, out double ydistance);

            string xDistType = arrayWindow.cbox_XDistList.SelectedValue.ToString();
            string yDistType = arrayWindow.cbox_YDistList.SelectedValue.ToString();

            double calcXDist = 0;
            double calcYDist = 0;
            double farthestX = referencePoint.X;
            double farthestY = referencePoint.Y;

            //verify calculated distance and total distance in the X direction for XYZ creation
            if (xquantity > 0)
            {
                if (xDistType == "Total Distance")
                {
                    calcXDist = xdistance / xquantity;
                    farthestX = referencePoint.X + xdistance;
                }
                else
                {
                    calcXDist = xdistance;
                    farthestX = referencePoint.X + (xdistance * xquantity);
                }
            }

            //verify calculated distance and total distance in the Y direction for XYZ creation
            if (yquantity > 0)
            {
                if (yDistType == "Total Distance")
                {
                    calcYDist = ydistance / yquantity;
                    farthestY = referencePoint.Y + ydistance;
                }
                else
                {
                    calcYDist = ydistance;
                    farthestY = referencePoint.Y + (ydistance * yquantity);
                }
            }

            //create XYZ coordinate system based on user input.
            if (xquantity > 0 && yquantity > 0)
            {
                for (double x = referencePoint.X; (farthestX > referencePoint.X ? x < farthestX : x > farthestX); x += calcXDist)
                {
                    for (double y = referencePoint.Y; (farthestY > referencePoint.Y ? y < farthestY : y > farthestY); y += calcYDist)
                    {
                        userDefinedPoints.Add(new XYZ(x, y, referencePoint.Z));
                    }
                }
            }

            else if (xquantity > 0 && yquantity == 0)
            {
                for (double x = referencePoint.X; (farthestX > referencePoint.X ? x < farthestX : x > farthestX); x += calcXDist)
                {
                    userDefinedPoints.Add(new XYZ(x, referencePoint.Y, referencePoint.Z));
                }
            }

            else if (xquantity == 0 && yquantity > 0)
            {
                for (double y = referencePoint.Y; (farthestY > referencePoint.Y ? y < farthestY : y > farthestY); y += calcYDist)
                {
                    userDefinedPoints.Add(new XYZ(referencePoint.X, y, referencePoint.Z));
                }
            }

            if (userDefinedPoints.Any())
            {
                //remove first instance in list to not create an identical instance
                userDefinedPoints.RemoveAt(0);
            }

            return userDefinedPoints;
        }
    }
}
