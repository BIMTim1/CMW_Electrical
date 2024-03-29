﻿using System;
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

            TaskDialog.Show("Electrical Equipment Selection", "Select a Panelboard Family to Update the Type to Single Phase.");

            try
            {
                //create selection elements
                //ISelectionFilter selFilter = new EquipmentSelectionFilter();
                //Reference selectPanel = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select a Panelboard Family");
                Reference selectPanel = uidoc.Selection.PickObject(ObjectType.Element, "Select a Panelboard Family");
                FamilyInstance selectedPanel = doc.GetElement(selectPanel.ElementId) as FamilyInstance;

                //get Panel DIName to collect Electrical Equipment again
                string pnlName = selectedPanel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
                //get Supply From parameter of Selected Electrical Equipment
                string pnlSupply = selectedPanel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM).AsString();

                BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

                //get existing circuits of selected Electrical Equipment
                List<ElectricalSystem> col_circuits = new FilteredElementCollector(doc).OfClass(typeof(ElectricalSystem))
                    .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM).AsString() == pnlName)
                    .Cast<ElectricalSystem>().ToList();

                //get Electrical Equipment source of selected Panel
                List<Element> source_panel = new FilteredElementCollector(doc)
                    .OfCategory(bic)
                    .WhereElementIsNotElementType()
                    .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() == pnlSupply)
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
                    if (fam_name == selectedPanel.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() & fam_type.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString().Contains("Single"))
                    {
                        single_ph_type = fam_type;
                    }
                }

                TransactionGroup tracGroup = new TransactionGroup(doc, "Update Panel Type to Single Phase and Reconnect Circuits");
                Transaction trac = new Transaction(doc);

                try
                {
                    tracGroup.Start();

                    //start first Transaction in TransactionGroup
                    trac.Start("Update Panel Type to Single Phase");

                    //change selected Electrical Equipment Type Id
                    Parameter pnlTypeParam = selectedPanel.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                    pnlTypeParam.Set(single_ph_type.Id);

                    trac.Commit();

                    //start second Transaction in TransactionGroup
                    trac.Start("Reconnect Original Circuits");

                    //collect updated Electrical Equipment FamilyInstance
                    FamilyInstance updated_pnl = new FilteredElementCollector(doc)
                        .OfCategory(bic)
                        .WhereElementIsNotElementType()
                        .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() == pnlName).First() as FamilyInstance;

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

                    tracGroup.Assimilate();
                }
                catch (Exception ex)
                {
                    tracGroup.RollBack();
                    errorReport = ex.Message;
                    return Result.Failed;
                }

                return Result.Succeeded;
                //return Result.Failed;
            }

            catch (Exception ex)
            {
                errorReport = ex.Message;
                TaskDialog.Show("User Canceled Operation", "This tool has been canceled by the user.");
                return Result.Failed;
            }
        }

        public class EquipmentSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element.Category.Name == "Electrical Equipment";
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
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

        public bool UpdatePanelScheduleView(Document document, FamilyInstance panel, string pnl_name)
        {
            bool panel_bool = false;

            //get all PanelScheduleViews in project
            List<PanelScheduleView> pnlSchedViews = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleView))
                .WhereElementIsNotElementType()
                .Cast<PanelScheduleView>()
                .ToList();

            //get all PanelScheduleTemplates
            PanelScheduleTemplate schedTemp = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleTemplate))
                .Where(x => x.Name == "ONE Branch Panel - Single Phase")
                .Cast<PanelScheduleTemplate>()
                .ToList()
                .First();

            //filter out just the panel schedule of the selected panel
            foreach (PanelScheduleView sched in pnlSchedViews)
            {
                if (document.GetElement(sched.GetPanel()).LookupParameter("Panel Name").AsString() == pnl_name)
                {
                    sched.GenerateInstanceFromTemplate(schedTemp.Id);
                    panel_bool = true;
                }
            }

            return panel_bool;
        }
    }
}
