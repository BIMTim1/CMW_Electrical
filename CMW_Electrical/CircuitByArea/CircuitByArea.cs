using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;
using CMW_Electrical.CircuitByArea;
using Autodesk.Revit.DB.Electrical;
using System.Windows;

namespace CircuitByArea
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CircuitByArea : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            View activeView = doc.ActiveView;

            #region ViewType check
            //check ViewType
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                errorReport = "Incorrect ViewType. Change the active view to a FloorPlan view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //ViewType check

            IList<Element> selElements = new List<Element>();

            #region User Selection
            try
            {
                ISelectionFilter selFilter = new CMWElecSelectionFilter.ElecFixtureSelectionFilter();

                selElements = uidoc.Selection.PickElementsByRectangle(selFilter, "Select elements by rectangle.");
            }
            catch (OperationCanceledException ex)
            {
                errorReport = "User canceled operation.";

                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;

                return Result.Failed;
            }
            #endregion //User Selection

            //prompt user to select new ElectricalEquipment source
            //look into a way to filter by DistributionSystem from selected elements (always 120V?)
            List<Element> all_equip = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilyInstance))
                .ToElements().ToList();

            //cancel tool if no equipment
            if (!all_equip.Any())
            {
                errorReport = "No instances of Electrical Equipment exist in the model to be connected to. The tool will now cancel.";

                return Result.Cancelled;
            }

            //all_equip = all_equip.OrderBy(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString()).ToList();
            List<EquipmentSelectionData> equipData = new List<EquipmentSelectionData>();

            foreach (Element equip in all_equip)
            {
                equipData.Add(new EquipmentSelectionData(equip));
            }

            //start WPF
            SelectPanel wpf = new SelectPanel(equipData);
            wpf.ShowDialog();

            //cancel tool if wpf canceled
            if (wpf.DialogResult == false)
            {
                errorReport = "User canceled source selection. Tool will now cancel.";

                return Result.Cancelled;
            }

            //collect FamilyInstance from user selected element
            FamilyInstance selEquip = wpf.cboxPanels.SelectedValue as FamilyInstance;

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Circuit by Area");

                    List<Element> circuitList = new List<Element>();
                    List<ElementId> spaceIdList = new List<ElementId>();

                    //itereate through each ElectricalFixture for unique Spaces
                    foreach (Element ef in selElements)
                    {
                        ElementId spaceId = (ef as FamilyInstance).Space.Id;

                        if (!spaceIdList.Contains(spaceId))
                        {
                            spaceIdList.Add(spaceId);
                        }
                    }

                    //iterate through spaceIdList to collect list of elements to be circuited by space
                    foreach (ElementId sId in spaceIdList)
                    {
                        string spaceName = doc.GetElement(sId).LookupParameter("Name").AsString();

                        ICollection<ElementId> efIds = (from ef in selElements select ef.Id).ToList();

                        List<ElementId> filElems = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
                            .WhereElementIsNotElementType()
                            .ToElementIds()
                            .Where(x => (doc.GetElement(x) as FamilyInstance).Space.Id == sId && efIds.Contains(x))
                            .ToList();

                        //collet ElectricalSystemType information for ElectricalSystem
                        FamilyInstance elecFixt = doc.GetElement(filElems[0]) as FamilyInstance;
                        ConnectorSet connSet = elecFixt.MEPModel.ConnectorManager.UnusedConnectors;
                        ElectricalSystemType elecSysType = new ElectricalSystemType();

                        foreach (Connector conn in connSet)
                        {
                            elecSysType = conn.ElectricalSystemType;
                        }

                        //assume IList is not empty
                        if (filElems.Count() <= 7)
                        {
                            ElectricalSystem elecSys = ElectricalSystem.Create(doc, filElems, elecSysType);
                            elecSys.SelectPanel(selEquip);
                            Parameter loadName = elecSys.LookupParameter("Load Name");
                            loadName.Set(loadName.AsString().ToUpper());
                        }
                        else
                        {
                            int i = 0;
                            //int elemCount = 0;
                            IList<ElementId> cctList = new List<ElementId>();
                            int count = filElems.Count();

                            while (i < count+1)
                            {
                                i++;
                                cctList.Add(filElems[i-1]);

                                if (cctList.Count() == 7 || i == count)
                                {
                                    //create ElectricalSystem from 7 conveience devices
                                    ElectricalSystem elecSys = ElectricalSystem.Create(doc, cctList, elecSysType);
                                    elecSys.SelectPanel(selEquip);

                                    Parameter loadName = elecSys.LookupParameter("Load Name");
                                    loadName.Set(loadName.AsString().ToUpper());

                                    doc.Regenerate();

                                    ////reset elemCount
                                    //elemCount = 0;

                                    cctList.Clear();
                                }

                                if (i == count)
                                {
                                    break;
                                }
                            }
                        }
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
    }
}
