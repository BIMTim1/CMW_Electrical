using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConduitFromEquipmentCircuit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConduitFromEquipmentCircuit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion // Autodesk Info

            #region ViewType Check
            View activeView = doc.ActiveView;

            List<ViewType> allowedViewTypes = new List<ViewType>()
            {
                ViewType.FloorPlan, ViewType.CeilingPlan, ViewType.Section, ViewType.ThreeD
            };

            if (!allowedViewTypes.Contains(activeView.ViewType))
            {
                elementSet.Insert(activeView);
                errorReport = "Incorrect ViewType. Change the active view to a Floor Plan, Ceiling Plan, Section, or 3D view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType Check

            #region User Selection
            Element selElem = null;
            try
            {
                ISelectionFilter selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();

                selElem = doc.GetElement(
                    uidoc.Selection.PickObject(
                        ObjectType.Element,
                        selFilter,
                        "Select an Electrical Equipment Instance."));
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                errorReport = "User canceled selection. Tool will now cancel.";
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;
                return Result.Failed;
            }
            #endregion //User Selection

            #region ElectricalSystem check
            //check if selected Electrical Equipment instance has an Electrical Circuit
            ElectricalSystem elecSys = null;
            ISet<ElectricalSystem> collSystems = (selElem as FamilyInstance).MEPModel.GetElectricalSystems();

            if (collSystems.Any())
            {
                foreach (ElectricalSystem e in collSystems)
                {
                    string baseName = e.BaseEquipment.Name;

                    if (baseName != (selElem as FamilyInstance).get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString())
                    {
                        elecSys = e;
                    }
                }
            }

            if (elecSys == null)
            {
                errorReport = "Selected Equipment doesn't have a source Electrical Circuit. The tool will now cancel.";

                return Result.Cancelled;
            }
            #endregion //ElectricalSystem check

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Create Conduit from Selected Equipment Circuit");

                    #region Create Conduits
                    List<ElementId> conduitTypes = new FilteredElementCollector(doc).OfClass(typeof(ConduitType)).ToElementIds().ToList();
                    ElementId conduitType = conduitTypes[1];

                    IList<XYZ> pathList = elecSys.GetCircuitPath();
                    ElementId levelId = selElem.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM).AsElementId();
                    List<Conduit> conduitList = new List<Conduit>();
                    int count = pathList.Count();
                    int i = 0;

                    while (i < count)
                    {
                        List<XYZ> coordinates = Coordinate_Offsets(i, count, pathList);

                        Conduit conduit = Conduit.Create(doc, conduitType, coordinates[0], coordinates[1], levelId);
                        conduitList.Add(conduit);

                        i++;
                        if (i == count - 1)
                        {
                            break;
                        }
                    }
                    #endregion //Create Conduits

                    doc.Regenerate();

                    #region Create Conduit Elbow Fittings
                    int cCount = conduitList.Count();
                    i = cCount - 1;

                    while (i > 0)
                    {
                        Conduit conn1 = conduitList[i];
                        Conduit conn2 = conduitList[i - 1];

                        List<Connector> connectors = GetConnectors(conn1, conn2);

                        doc.Create.NewElbowFitting(connectors.First(), connectors.Last());
                        i--;
                        if (i == 0)
                        {
                            break;
                        }
                    }
                    #endregion //Create Conduit Elbow Fittings

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

        #region double value adjustments
        public double IsNegativeFirst(double inputInteger)
        {
            double val;

            if (inputInteger < 0)
            {
                val = inputInteger + 1;
            }
            else
            {
                val = inputInteger - 1;
            }
            return val;
        }

        public double IsNegativeSecond(double inputDouble)
        {
            double val;
            if (inputDouble < 0)
            {
                val = inputDouble - 1;
            }
            else
            {
                val = inputDouble + 1;
            }
            return val;
        }
        #endregion //double value adjustments

        public List<XYZ> Coordinate_Offsets(int currentValue, int totalValue, IList<XYZ> pathPointList)
        {
            List<XYZ> output = new List<XYZ>();
            XYZ firstSetPoints = pathPointList[currentValue];
            XYZ secondSetPoints = pathPointList[currentValue + 1];
            XYZ newFirstPoint;
            XYZ newSecondPoint;
            double firstX = Math.Round(firstSetPoints.X, 9);
            double firstY = Math.Round(firstSetPoints.Y, 9);
            double firstZ = Math.Round(firstSetPoints.Z, 9);
            double secX = Math.Round(secondSetPoints.X, 9);
            double secY = Math.Round(secondSetPoints.Y, 9);
            double secZ = Math.Round(secondSetPoints.Z, 9);

            if (firstX != secX)
            {
                firstX = IsNegativeFirst(firstX);
                secX = IsNegativeSecond(secX);
            }
            else if (firstY != secY)
            {
                firstY = IsNegativeFirst(firstY);
                secY = IsNegativeSecond(secY);
            }
            else if (firstZ != secZ)
            {
                if (currentValue == 0)
                {
                    secZ = IsNegativeFirst(secZ);
                }
                else
                {
                    firstZ = IsNegativeFirst(firstZ);
                }
            }

            if (currentValue != 0)
            {
                newFirstPoint = new XYZ(firstX, firstY, firstZ);
            }
            else
            {
                newFirstPoint = firstSetPoints;
            }

            if (currentValue + 1 != totalValue)
            {
                newSecondPoint = new XYZ(secX, secY, secZ);
            }
            else
            {
                newSecondPoint = secondSetPoints;
            }

            output.Add(newFirstPoint);
            output.Add(newSecondPoint);

            return output;
        }

        public List<Connector> GetConnectors(Conduit firstConduit, Conduit secondConduit)
        {
            List<Connector> output = new List<Connector>();
            XYZ start1 = (firstConduit.Location as LocationCurve).Curve.GetEndPoint(0);
            XYZ end1 = (firstConduit.Location as LocationCurve).Curve.GetEndPoint(1);

            XYZ start2 = (secondConduit.Location as LocationCurve).Curve.GetEndPoint(0);
            XYZ end2 = (secondConduit.Location as LocationCurve).Curve.GetEndPoint(1);
            Connector c1start = null, c1end = null, c2start = null, c2end = null;

            foreach (Connector c in firstConduit.ConnectorManager.Connectors)
            {
                if (c.Origin.IsAlmostEqualTo(start1))
                {
                    c1start = c;
                }
                else if (c.Origin.IsAlmostEqualTo(end1))
                {
                    c1end = c;
                }
            }

            foreach (Connector c in secondConduit.ConnectorManager.Connectors)
            {
                if (c.Origin.IsAlmostEqualTo(start2))
                {
                    c2start = c;
                }
                else if (c.Origin.IsAlmostEqualTo(end2))
                {
                    c2end = c;
                }
            }

            output.Add(c1start);
            output.Add(c2end);

            return output;
        }
    }
}
