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
using CMWElec_FailureHandlers;
using CMW_Electrical.ChangePanelToSinglePhase;
using System.IO;
using System.Diagnostics;
using Autodesk.Revit.DB.Mechanical;
using System.Net;
using PanelSchedFormatting;

namespace ChangePanelTypeToSinglePhase
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class PanelTypeToSinglePhase : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Application and Document info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Application and Document info

            View activeView = doc.ActiveView;

            string output = "";

            #region check ActiveView ViewType
            //check view type
            if (activeView.ViewType != ViewType.FloorPlan && activeView.ViewType != ViewType.ThreeD)
            {
                errorReport = "Incorrect view type. This tool can only be run from Floor Plan or 3D views. " +
                    "Change your active view and rerun the tool.";

                return Result.Cancelled;
            }
            #endregion //check ActiveView ViewType

            #region BuiltInParameter references
            BuiltInParameter bipPanelName = BuiltInParameter.RBS_ELEC_PANEL_NAME;
            BuiltInParameter bipRating = BuiltInParameter.RBS_ELEC_CIRCUIT_RATING_PARAM;
            BuiltInParameter bipFrame = BuiltInParameter.RBS_ELEC_CIRCUIT_FRAME_PARAM;
            BuiltInParameter bipSupply = BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM;
            BuiltInParameter bipDistSys = BuiltInParameter.RBS_FAMILY_CONTENT_DISTRIBUTION_SYSTEM;
            #endregion //BuiltInParameter references

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
                        "Select an Electrical Equipment family to change family and type.");

                    //selItem = uidoc.Selection.PickObject(ObjectType.Element,
                    //"Select a Panelboard Family to Update the Type to Single Phase."); //debug only
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

                selElem = doc.GetElement(selItem) as FamilyInstance;
            }

            //get Panel DIName to collect Electrical Equipment again
            string pnlName = selElem.get_Parameter(bipPanelName).AsString();
            //get Supply From parameter of Selected Electrical Equipment
            string pnlSupply = selElem.get_Parameter(bipSupply).AsString();

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
                .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM).AsString() == pnlName && x.LookupParameter("Load Name").AsString() != "SPARE")
                .Cast<ElectricalSystem>().ToList();

            //get Electrical Equipment source of selected Panel
            List<Element> source_panel = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .Where(x => x.get_Parameter(bipPanelName).AsString() == pnlSupply)
                .ToList();

            //collect Electrical Equipment types in model (exclude selected element FamilySymbol)
            List<FamilySymbol> all_equipmentTypes = 
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilySymbol))
                .Where(x => x.Name != selElem.Symbol.Name)
                .Cast<FamilySymbol>()
                .ToList();

            //create form instance
            DistributionSelectionForm form = new DistributionSelectionForm(all_equipmentTypes);
            form.ShowDialog();

            //cancel if Cancel button selected
            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                errorReport = "User canceled operation.";

                return Result.Cancelled;
            }

            //select FamilySymbol to change selected family to
            FamilySymbol selFamSym;
            List<FamilySymbol> selFamSymList = (from fs 
                                            in all_equipmentTypes 
                                            where fs.FamilyName + ": " + fs.Name == form.cboxSelectDisSys.SelectedItem.ToString() 
                                            select fs)
                                            .ToList();

            //check if empty
            if (!selFamSymList.Any())
            {
                errorReport = "No Family Type matches the selected item. The tool will now cancel.";

                return Result.Cancelled;
            }

            selFamSym = selFamSymList.First();

            using (TransactionGroup tracGroup = new TransactionGroup(doc))
            {
                tracGroup.Start("CMWElec-Change Equipment Distribution System and Reconnect Circuits");

                using (Transaction trac = new Transaction(doc))
                {
                    try
                    {
                        #region UpdateFamilyAndType
                        trac.Start("CMWElec-Update Selected Equipment Family and Type");

                        //define IFailureHandingProcessor
                        FailureHandlingOptions failOpt = trac.GetFailureHandlingOptions();
                        failOpt.SetFailuresPreprocessor(new DisconnectCircuitFailure());
                        trac.SetFailureHandlingOptions(failOpt);

                        //check for existing panelboard schedule
                        IList<ElementId> previousSched = selElem.GetDependentElements(new ElementClassFilter(typeof(PanelScheduleView)));

                        if (previousSched.Any())
                        {
                            //remove any previously created Spares from Panelboard for successful reconnection of circuits
                            ElementId previousSchedId = previousSched.First();

                            //delete Spares from equipment
                            RemoveSpares(doc, previousSchedId);

                            doc.Delete(previousSchedId);
                        }

                        //check for existing branch circuits and disconnect if different distribution
                        if (col_circuits.Any() && selFamSym.LookupParameter("Voltage Nominal").AsDouble() != selElem.Symbol.LookupParameter("Voltage Nominal").AsDouble())
                        {
                            foreach (ElectricalSystem cct in col_circuits)
                            {
                                cct.DisconnectPanel();
                            }
                        }

                        //change selected Electrical Equipment Type Id
                        Parameter pnlTypeParam = selElem.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                        pnlTypeParam.Set(selFamSym.Id);

                        doc.Regenerate();

                        output += $"Panel Type was updated to the following:\n{selFamSym.FamilyName}: {selFamSym.Name}\n\n";

                        trac.Commit();
                        #endregion //UpdateFamilyAndType

                        #region ReconnectSource
                        trac.Start("CMWElec-Reconnect Source");

                        //collect updated Electrical Equipment FamilyInstance
                        FamilyInstance updated_pnl = new FilteredElementCollector(doc)
                            .OfCategory(bic)
                            .WhereElementIsNotElementType()
                            .Where(x => x.get_Parameter(bipPanelName).AsString() == pnlName).First() as FamilyInstance;

                        //create ElectricalSystem if already previously connected
                        if (source_panel.Any())
                        {
                            //check for source distribution system prior to connection
                            bool reconnect = false;
                            ElectricalSystem newCct = null;

                            ElementId pnl_distSys = updated_pnl.get_Parameter(bipDistSys).AsElementId();

                            FamilyInstance sourceEquip = source_panel.First() as FamilyInstance;
                            
                            if (sourceEquip.Symbol.FamilyName.Contains("Transformer"))
                            {
                                if (sourceEquip.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_SECONDARY_DISTRIBSYS).AsElementId() == pnl_distSys)
                                {
                                    reconnect = true;
                                }
                            }
                            else
                            {
                                if (sourceEquip.get_Parameter(bipDistSys).AsElementId() == pnl_distSys)
                                {
                                    reconnect = true;
                                }
                            }

                            //reconnect if true
                            if (reconnect)
                            {
                                newCct = new CreateEquipmentCircuit().CreateEquipCircuit(source_panel.First() as FamilyInstance, updated_pnl);

                                output += "Selected Electrical Equipment reconnected to original source.\n\n";
                            }
                            else
                            {
                                //collect ElectricalEquipment with same distribution system
                                List<FamilyInstance> sameDistEquipList = new FilteredElementCollector(doc)
                                    .OfCategory(bic)
                                    .OfClass(typeof(FamilyInstance))
                                    .ToElements()
                                    .Where(x => x.get_Parameter(bipDistSys).AsElementId() == pnl_distSys && x.Id != updated_pnl.Id)
                                    .Cast<FamilyInstance>()
                                    .ToList();

                                if (!sameDistEquipList.Any())
                                {
                                    //continue operation of tool, but no new source can be selected
                                    TaskDialog noReconnection = new TaskDialog("CMW-Elec - Can't Select Source")
                                    {
                                        TitleAutoPrefix = false,
                                        CommonButtons = TaskDialogCommonButtons.Ok,
                                        MainInstruction = "There are no Electrical Equipment families that match the updated Distribution System. " +
                                        "The tool is unable to prompt a new source selection but the tool will continue."
                                    };

                                    noReconnection.Show();

                                    output += "Electrical Circuit Reconnection:\nNo Electrical Equipment matched updated Distribution System. Process skipped.\n\n";
                                }
                                else
                                {
                                    //order list by Panel Name
                                    sameDistEquipList.OrderBy(x => x.get_Parameter(bipPanelName).ToString()).ToList();

                                    string ssFormName = "Select Source Equipment";
                                    string ssLabelText = "The selected Electrical Equipment family is unable to reconnect to its original source. " +
                                        "Would you like to select a new source now? Click Cancel to skip.";

                                    //start form for user selection of new ElectricalEquipment source
                                    SelectNewSourceForm selectSourceForm = new SelectNewSourceForm(sameDistEquipList, ssFormName, ssLabelText);
                                    selectSourceForm.ShowDialog();

                                    if (selectSourceForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                                    {
                                        //select FamilyInstance from list index
                                        FamilyInstance newSource = sameDistEquipList[selectSourceForm.cboxSelectSource.SelectedIndex];

                                        //generate new circuit
                                        newCct = new CreateEquipmentCircuit()
                                            .CreateEquipCircuit(newSource, updated_pnl);

                                        output += $"Electrical Circuit Reconnection:\nElectrical Circuit connected to {newSource.get_Parameter(bipPanelName).AsString()}.\n\n";
                                    }
                                    else
                                    {
                                        //user canceled
                                        TaskDialog userCanceled = new TaskDialog("CMW-Elec - New Source Canceled")
                                        {
                                            TitleAutoPrefix = false,
                                            CommonButtons = TaskDialogCommonButtons.Ok,
                                            MainInstruction = "User canceled operation. The tool will continue but a new source will not be selected."
                                        };

                                        userCanceled.Show();

                                        output += "Electrical Circuit Reconnection:\nUser canceled reconnection process.\n\n";
                                    }
                                }
                            }
                        }

                        trac.Commit();
                        #endregion //ReconnectSource

                        #region ReconnectBranchCircuits
                        trac.Start("CMW-Elec - Reconnect Existing Branch Circuits");

                        //process branch circuits of updated ElectricalEquipment
                        if (col_circuits.Any())
                        {
                            //check Voltage
                            Parameter testCct = col_circuits.First().get_Parameter(BuiltInParameter.RBS_ELEC_VOLTAGE);
                            string testVolt = UnitUtils.ConvertFromInternalUnits(testCct.AsDouble(), testCct.GetUnitTypeId()).ToString();

                            DistributionSysType pnlDistSys = doc.GetElement(updated_pnl.get_Parameter(bipDistSys).AsElementId()) as DistributionSysType;
                            string dtLLVolt = pnlDistSys.LookupParameter("Line to Line Voltage").AsValueString();
                            string dtLGVolt = pnlDistSys.LookupParameter("Line to Ground Voltage").AsValueString();

                            //check DistributionSysType info against existing circuits
                            if (dtLLVolt.Contains(testVolt) || dtLGVolt.Contains(testVolt))
                            {
                                foreach (ElectricalSystem ogcct in col_circuits)
                                {
                                    ogcct.SelectPanel(updated_pnl);
                                }
                            }
                            else
                            {
                                //collect ElectricalEquipment with same DistributionSystem as branch ElectricalSystems
                                List<FamilyInstance> availableBranchSources = new FilteredElementCollector(doc)
                                    .OfCategory(bic)
                                    .OfClass(typeof(FamilyInstance))
                                    .ToElements()
                                    .Cast<FamilyInstance>()
                                    .Where(x => doc.GetElement(x.get_Parameter(bipDistSys).AsElementId()).LookupParameter("Line to Line Voltage").AsValueString().Contains(testVolt) 
                                    || doc.GetElement(x.get_Parameter(bipDistSys).AsElementId()).LookupParameter("Line to Ground Voltage").AsValueString().Contains(testVolt)).ToList();

                                if (!availableBranchSources.Any())
                                {
                                    TaskDialog branchConnectFailedDialog = new TaskDialog("CMW-Elec - Branch Connection")
                                    {
                                        TitleAutoPrefix = false,
                                        CommonButtons = TaskDialogCommonButtons.Ok,
                                        MainInstruction = "There are no instances of Electrical Equipment in the model that can be used to " +
                                        "reconnect existing branch circuits. The tool will now cancel."
                                    };

                                    branchConnectFailedDialog.Show();

                                    output += "Branch Circuit Connections:\nUnable to select source for existing branch circuits.\n\n";
                                }
                                else
                                {
                                    string recFormName = "Select New Branch Source";
                                    string recLabelText = $"The branch circuits of {pnlName} cannot be reconnected to the updated equipment family type. " +
                                        $"Would you like to select a new source now? Click Cancel to skip.";

                                    availableBranchSources.OrderBy(x => x.get_Parameter(bipPanelName).ToString()).ToList();

                                    SelectNewSourceForm branchSelectForm = new SelectNewSourceForm(
                                        availableBranchSources, 
                                        recFormName, recLabelText);

                                    branchSelectForm.ShowDialog();

                                    if (branchSelectForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        TaskDialog recResults = new TaskDialog("CMW-Elec - Branch Circuit Reconnection")
                                        {
                                            TitleAutoPrefix = false,
                                            CommonButtons = TaskDialogCommonButtons.Ok,
                                            MainInstruction = "A new source for branch circuits was not selected. " +
                                            "Tool will continue, but branch circuits will have to be connected to a new source manually."
                                        };

                                        recResults.Show();

                                        output += "Branch Circuit Connections:\nBranch circuits have been disconnected from original source. " +
                                            "User canceled selection of new source.\n\n";
                                    }
                                    else
                                    {
                                        FamilyInstance selectBranchSource = availableBranchSources[branchSelectForm.cboxSelectSource.SelectedIndex];

                                        foreach (ElectricalSystem cct in col_circuits)
                                        {
                                            cct.SelectPanel(selectBranchSource);
                                        }

                                        output += $"Branch Circuit Connections:" +
                                            $"\nBranch circuits were connected to the newly selected equipment source: " +
                                            $"{selectBranchSource.get_Parameter(bipPanelName).AsString()}\n\n";
                                    }
                                }
                            }
                        }

                        bool sched = UpdatePanelScheduleView(doc, updated_pnl);

                        if (sched)
                        {
                            output += "Panel Schedule Update:\nA Panel Schedule has been updated for the selected Electrical Equipment.\n\n";
                        }
                        else
                        {
                            output += "Panel Schedule Update:\nA Panel Schedule could not be created for the selected Electrical Equipment.\n\n";
                        }

                        trac.Commit();
                        #endregion //ReconnectBranchCircuits
                    }
                    catch (Exception ex)
                    {
                        errorReport = ex.Message;
                        return Result.Failed;
                    }
                }

                tracGroup.Assimilate();

                CreateOutputFile(output);

                return Result.Succeeded;
            }
        }

        //public bool UpdateDistributionSystem(FamilyInstance equip, Document document)
        //{
        //    bool confirmBool = false;
        //    Parameter voltage = equip.Symbol.LookupParameter("Voltage Nominal");
        //    double val = voltage.AsDouble();
        //    ForgeTypeId ftId = voltage.GetUnitTypeId();

        //    double eq_voltage = UnitUtils.ConvertFromInternalUnits(val, ftId);

        //    //confirm Number of Phases parameter
        //    Parameter numPhasesParam = equip.Symbol.LookupParameter("Number of Poles") ?? 
        //        equip.Symbol.LookupParameter("Primary Number of Phases");

        //    int numPhases = numPhasesParam.AsInteger();

        //    //collect all DistributionSysTypes in document
        //    DistributionSysType selDisType = null;
        //    DistributionSysTypeSet distTypeSet = document.Settings.ElectricalSetting.DistributionSysTypes;

        //    foreach (DistributionSysType dt in distTypeSet)
        //    {
        //        string dtLLVolt = dt.get_Parameter(BuiltInParameter.RBS_DISTRIBUTIONSYS_VLL_PARAM).AsValueString();
        //        string dtLGVolt = dt.get_Parameter(BuiltInParameter.RBS_DISTRIBUTIONSYS_VLG_PARAM).AsValueString();
        //        string dtPhase = dt.get_Parameter(BuiltInParameter.RBS_DISTRIBUTIONSYS_PHASE_PARAM).AsValueString();
        //        int phase;

        //        if (dtPhase == "Three")
        //        {
        //            phase = 3;
        //        }
        //        else
        //        {
        //            phase = 1;
        //        }

        //        //check if DistributionSysType can be used for equip
        //        if (numPhases == 1 || numPhases == 2)
        //        {
        //            if (dtLGVolt == eq_voltage.ToString() && phase == 1)
        //            {
        //                selDisType = dt;
        //            }
        //        }
        //        else
        //        {
        //            if (dtLLVolt == eq_voltage.ToString() && phase == numPhases)
        //            {
        //                selDisType = dt;
        //            }
        //        }
        //    }

        //    //set DistributionSysType of equip
        //    if (selDisType != null)
        //    {
        //        equip.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_DISTRIBUTION_SYSTEM).Set(selDisType.Id);
        //        confirmBool = true;
        //    }

        //    return confirmBool;
        //}

        #region SwitchboardTemplates
        /// <summary>
        /// Collect PanelScheduleTemplates of PanelScheduleType Switchboard.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>List of Switchboard type PanelScheduleTemplates in current document.</returns>
        public List<PanelScheduleTemplate> SwitchboardTemplates(Document document)
        {
            List<PanelScheduleTemplate> switchTemps = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleTemplate))
                .Cast<PanelScheduleTemplate>()
                .Where(x => x.GetPanelScheduleType() == PanelScheduleType.Switchboard)
                .ToList();

            return switchTemps;
        }
        #endregion //SwitchboardTemplates

        #region BranchScheduleTemplates
        /// <summary>
        /// Collect PanelScheduleTemplates of PanelScheduleType Branch
        /// </summary>
        /// <param name="document"></param>
        /// <returns>List of Branch type PanelScheduleTemplates in current document.</returns>
        public List<PanelScheduleTemplate> BranchScheduleTemplates(Document document)
        {
            List<PanelScheduleTemplate> branchTemp = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleTemplate))
                .Cast<PanelScheduleTemplate>()
                .Where(x => x.GetPanelScheduleType() == PanelScheduleType.Branch)
                .ToList();

            return branchTemp;
        }
        #endregion //BranchScheduleTemplates

        #region UpdatePanelScheduleView
        /// <summary>
        /// Delete and recreate PanelScheduleView of selected ElectricalEquipment FamilyInstance.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="panel"></param>
        /// <returns>True if schedule created otherwise false.</returns>
        public bool UpdatePanelScheduleView(Document document, FamilyInstance panel)
        {
            //collect information for new panel schedule
            string famName = panel.Symbol.FamilyName;

            //cancel process if schedule could not be created
            if (famName.Contains("Transformer") || famName.Contains("Cabinet"))
            {
                return false;
            }

            //create PanelScheduleView
            string testName;
            int maxBreakers;
            List<PanelScheduleTemplate> panTemps;

            if (panel.get_Parameter(BuiltInParameter.RBS_ELEC_MAX_POLE_BREAKERS) != null)
            {
                maxBreakers = panel.get_Parameter(BuiltInParameter.RBS_ELEC_MAX_POLE_BREAKERS).AsInteger();
                testName = $"ONE Branch Panel - {maxBreakers} Circuit";

                //collect Branch PanelScheduleTemplates
                panTemps = BranchScheduleTemplates(document);
            }
            else
            {
                maxBreakers = panel.get_Parameter(BuiltInParameter.RBS_ELEC_NUMBER_OF_CIRCUITS).AsInteger();

                //collect Switchboard PanelScheduleTemplates
                panTemps = SwitchboardTemplates(document);

                string refStr;

                if (famName.Contains("Switchboard"))
                {
                    refStr = "Switchboard";
                }
                else if (famName.Contains("Distribution") || famName.Contains("Automatic"))
                {
                    refStr = "Distribution Panel";
                }
                else
                {
                    refStr = "MCC";
                }

                testName = $"ONE {refStr} - {maxBreakers} Space";
            }

            ElementId newTempId = null;

            //iterate through list of PanelScheduleTemplates for correct id
            foreach (PanelScheduleTemplate pst in panTemps)
            {
                if (pst.Name == testName)
                {
                    newTempId = pst.Id;
                }
            }

            //check if newTempId is null
            if (newTempId == null)
            {
                return false;
            }

            PanelScheduleView.CreateInstanceView(document, newTempId, panel.Id);

            return true;
        }
        #endregion //UpdatePanelScheduleView

        #region RemoveSpares
        public bool RemoveSpares(Document document, ElementId panViewId)
        {
            //collect PanelScheduleView
            PanelScheduleView panView = document.GetElement(panViewId) as PanelScheduleView;

            //Get TableData to interact with specific cells of the PanelScheduleView
            TableData tableData = panView.GetTableData();
            TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);

            PanelScheduleType templateTypeName = (document.GetElement(panView.GetTemplate()) as PanelScheduleTemplate).GetPanelScheduleType();

            GetScheduleFormatting schedFormat = new GetScheduleFormatting();
            List<Int32> columns = schedFormat.GetPanelScheduleColumns(templateTypeName, panView, document);
            Int32 rows = schedFormat.GetPanelScheduleRows(templateTypeName, panView);

            for (Int32 rowNum = 2; rowNum <= rows; rowNum++)
            {
                foreach (Int32 colNum in columns)
                {
                    ElectricalSystem cct = panView.GetCircuitByCell(rowNum, colNum);

                    if (cct != null)
                    {
                        //check if slot is a Spare and has a modified name
                        bool isSpare = panView.IsSpare(rowNum, colNum);
                        string cctName = cct.LookupParameter("Load Name").AsString();

                        if (isSpare && cctName == "SPARE")
                        {
                            //delete blank Spares
                            panView.RemoveSpare(rowNum, colNum);
                        }
                    }
                }
            }

            return true;
        }
        #endregion //RemoveSpares

        #region CreateOutputFile
        /// <summary>
        /// Create an output file based on an input string. Open Notepad to display file once complete.
        /// </summary>
        /// <param name="output"></param>
        public void CreateOutputFile(string output)
        {
            string outputLocation = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Temp\\CMWElec_DistributionChangeLog.txt";

            //create text file
            using (StreamWriter sw = File.CreateText(outputLocation))
            {
                sw.WriteLine(output);
            }

            //start a new process and open the created file in NotePad
            Process.Start("notepad.exe", outputLocation);
        }
        #endregion //CreateOutputFile
    }
}
