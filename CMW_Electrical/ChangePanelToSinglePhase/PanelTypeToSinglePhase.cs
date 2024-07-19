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

namespace ChangePanelTypeToSinglePhase
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class PanelTypeToSinglePhase : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //BuiltInParameter references
            BuiltInParameter bipPanelName = BuiltInParameter.RBS_ELEC_PANEL_NAME;
            BuiltInParameter bipRating = BuiltInParameter.RBS_ELEC_CIRCUIT_RATING_PARAM;
            BuiltInParameter bipFrame = BuiltInParameter.RBS_ELEC_CIRCUIT_FRAME_PARAM;

            //Reference selectPanel = null;
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

            FamilyInstance selElem;
            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();

            if (selectedElementIds.Any())
            {
                //filter preselected list
                ElementCategoryFilter filter = new ElementCategoryFilter(bic);

                List<Element> filteredElemList = new FilteredElementCollector(doc, selectedElementIds).WherePasses(filter).ToList();

                if (filteredElemList.Count() != 1)
                {
                    selElem = null;
                }
                else
                {
                    selElem = filteredElemList.First() as FamilyInstance;
                }
            }
            else
            {
                selElem = null;
            }

            if (selElem == null)
            {
                Reference selItem;
                try
                {
                    //create selection elements
                    ISelectionFilter selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();
                    selItem = uidoc.Selection.PickObject(ObjectType.Element,
                        selFilter,
                        "Select a Panelboard Family to Update the Type to Single Phase.");

                    //selItem = uidoc.Selection.PickObject(ObjectType.Element,
                    //"Select a Panelboard Family to Update the Type to Single Phase."); //debug only
                }
                catch (OperationCanceledException ex)
                {
                    errorReport = ex.Message;
                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;
                    return Result.Failed;
                }

                selElem = doc.GetElement(selItem) as FamilyInstance;
            }

            //FamilyInstance selElem = doc.GetElement(selElem.Id) as FamilyInstance;

            //get Panel DIName to collect Electrical Equipment again
            string pnlName = selElem.get_Parameter(bipPanelName).AsString();
            //get Supply From parameter of Selected Electrical Equipment
            string pnlSupply = selElem.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM).AsString();

            //collect selected panelboard circuit parameters to update once new circuit is created
            List<ElectricalSystem> panelCircuits = (from x
                                                    in selElem.MEPModel.GetElectricalSystems()
                                                    where x.PanelName != pnlName
                                                    select x)
                                                    .ToList();

            double panelRating = 20;
            double panelFrame = 400;

            if (panelCircuits.Any())
            {
                foreach (ElectricalSystem panelCircuit in panelCircuits)
                {
                    panelRating = panelCircuit.get_Parameter(bipRating).AsDouble();
                    panelFrame = panelCircuit.get_Parameter(bipFrame).AsDouble();
                }
            }

            //get existing circuits of selected Electrical Equipment
            List<ElectricalSystem> col_circuits = new FilteredElementCollector(doc).OfClass(typeof(ElectricalSystem))
                .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM).AsString() == pnlName)
                .Cast<ElectricalSystem>().ToList();

            //get Electrical Equipment source of selected Panel
            List<Element> source_panel = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .Where(x => x.get_Parameter(bipPanelName).AsString() == pnlSupply)
                .ToList();

            //get Panel family type to change to
            FamilySymbol single_ph_type = null;
            List<Element> single_ph_types = new FilteredElementCollector(doc).OfCategory(bic)
                .WhereElementIsElementType()
                .ToElements()
                .ToList();

            foreach (FamilySymbol fam_type in single_ph_types)
            {
                string fam_name = fam_type.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();
                if (fam_name == selElem.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() & fam_type.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("Single"))
                {
                    single_ph_type = fam_type;
                }
            }

            using (TransactionGroup tracGroup = new TransactionGroup(doc))
            {
                tracGroup.Start("CMWElec-Update Panel Type to Single Phase and Reconnect Circuits");

                using (Transaction trac = new Transaction(doc))
                {
                    try
                    {
                        trac.Start("CMWElec-Update Panel Type to Single Phase");

                        //change selected Electrical Equipment Type Id
                        Parameter pnlTypeParam = selElem.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                        pnlTypeParam.Set(single_ph_type.Id);

                        trac.Commit();

                        trac.Start("CMWElec-Reconnect Original Circuits");

                        //collect updated Electrical Equipment FamilyInstance
                        FamilyInstance updated_pnl = new FilteredElementCollector(doc)
                            .OfCategory(bic)
                            .WhereElementIsNotElementType()
                            .Where(x => x.get_Parameter(bipPanelName).AsString() == pnlName).First() as FamilyInstance;

                        //re-create panel circuit and reconnect to source
                        if (source_panel.Any())
                        {
                            ConnectorSet connector_set = updated_pnl.MEPModel.ConnectorManager.UnusedConnectors;
                            ElectricalSystem newcct;
                            foreach (Connector conn in connector_set)
                            {
                                ElectricalSystemType conn_type = conn.ElectricalSystemType;
                                newcct = ElectricalSystem.Create(conn, conn_type);
                                newcct.SelectPanel(source_panel.First() as FamilyInstance);

                                //update Rating and Frame parameters of selected panelboard
                                newcct.get_Parameter(bipRating).Set(panelRating);
                                newcct.get_Parameter(bipFrame).Set(panelFrame);
                            }
                        }

                        //reconnect branch circuits to updated panel
                        if (col_circuits.Any())
                        {
                            foreach (ElectricalSystem ogcct in col_circuits)
                            {
                                ogcct.SelectPanel(updated_pnl);
                            }
                        }

                        //update Panel Distribution System
                        bool dist_sys = UpdateDistributionSystem(updated_pnl, doc);

                        //update Panel Schedule Template of updated_pnl
                        if (updated_pnl.GetDependentElements(new ElementClassFilter(typeof(PanelScheduleView))) != null)
                        {
                            bool sched = UpdatePanelScheduleView(doc, updated_pnl, pnlName);
                        }

                        trac.Commit();
                    }
                    catch (Exception ex)
                    {
                        errorReport = ex.Message;
                        return Result.Failed;
                    }
                }

                tracGroup.Assimilate();

                return Result.Succeeded;
            }
        }

        public bool UpdateDistributionSystem(FamilyInstance panel, Document document)
        {
            bool confirmBool = false;
            //collect Panel DistributionSystem
            Parameter pnl_dist = panel.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_DISTRIBUTION_SYSTEM);
            //get all project DistributionSysTypes
            List<Element> dist_types = new FilteredElementCollector(document).OfClass(typeof(DistributionSysType)).ToElements().ToList();

            //verify voltage type
            if (pnl_dist.AsValueString().Contains("208"))
            {
                foreach (Element dist in dist_types)
                {
                    if (dist.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("Single") & dist.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("208"))
                    {
                        pnl_dist.Set(dist.Id);
                        confirmBool = true;
                    }
                }
            }

            else if (pnl_dist.AsValueString().Contains("240"))
            {
                foreach (Element dist in dist_types)
                {
                    if (dist.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("Single") & dist.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("240"))
                    {
                        pnl_dist.Set(dist.Id);
                        confirmBool = true;
                    }
                }
            }

            else if (pnl_dist.AsValueString().Contains("480"))
            {
                foreach (Element dist in dist_types)
                {
                    if (dist.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("Single") & dist.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("480"))
                    {
                        pnl_dist.Set(dist.Id);
                        confirmBool = true;
                    }
                }
            }
            return confirmBool;
        }

        public List<PanelScheduleTemplate> BranchScheduleTemplates(Document document)
        {
            List<PanelScheduleTemplate> branchTemp = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleTemplate))
                .Cast<PanelScheduleTemplate>()
                .Where(x => x.GetPanelScheduleType() == PanelScheduleType.Branch)
                .ToList();

            return branchTemp;
        }

        public bool UpdatePanelScheduleView(Document document, FamilyInstance panel, string pnl_name)
        {
            bool panel_bool = false;
            ElementId tempId = null;
            string cbStr = panel.LookupParameter("Max Number of Single Pole Breakers").AsInteger().ToString();

            //get all PanelScheduleViews in project
            List<PanelScheduleView> pnlSchedViews = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleView))
                .WhereElementIsNotElementType()
                .Cast<PanelScheduleView>()
                .ToList();

            foreach (PanelScheduleTemplate schTemp in BranchScheduleTemplates(document))
            {
                string testSchName = $"ONE Branch Panel - {cbStr} Circuit";
                string schTempName = schTemp.Name;

                if (testSchName == schTempName)
                {
                    tempId = schTemp.Id;
                }
            }

            //filter out just the panel schedule of the selected panel
            if (tempId != null)
            {
                foreach (PanelScheduleView sched in pnlSchedViews)
                {
                    if (document.GetElement(sched.GetPanel()).LookupParameter("Panel Name").AsString() == pnl_name)
                    {
                        sched.GenerateInstanceFromTemplate(tempId);
                        panel_bool = true;
                    }
                }
            }

            return panel_bool;
        }
    }
}
