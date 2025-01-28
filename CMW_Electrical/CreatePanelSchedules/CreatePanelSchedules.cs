using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using System.Linq.Expressions;
using CMW_Electrical.CreatePanelSchedules;
using System.Windows.Forms;

namespace CreatePanelSchedules
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class CreatePanelSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            #endregion //Autodesk Info

            //BuiltInCategory value for Electrical Equipment
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

            //collect all Electrical Equipment families
            List<Element> elecEquip = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .ToList();

            #region Any Equipment Check
            //cancel tool if no applicable elements
            if (!elecEquip.Any())
            {
                errorReport = "There are no Electrical Equipment families to create Panelboard Schedules from.";
                return Result.Cancelled;
            }
            #endregion //Any Equipment Check

            //get Phases of ActiveDocument
            PhaseArray phaseArray = doc.Phases;

            List<Phase> phase_list = new List<Phase>();
            foreach (Phase ph in phaseArray)
            {
                phase_list.Add(ph);
            }

            #region User Phase Select
            //start Window PhaseSelection
            PhaseSelectWindow phaseSelectWindow = new PhaseSelectWindow(phase_list);
            phaseSelectWindow.ShowDialog();

            //check if Window canceled
            if (phaseSelectWindow.DialogResult == false)
            {
                errorReport = "User canceled Phase selection. Tool will now cancel.";

                return Result.Cancelled;
            }

            ElementId selPhaseId = phaseSelectWindow.cboxPhaseSelect.SelectedValue as ElementId;

            elecEquip = (from eq 
                         in elecEquip 
                         where eq.get_Parameter(BuiltInParameter.PHASE_CREATED).AsElementId() == selPhaseId 
                         select eq)
                         .ToList();
            #endregion //User Phase Select

            //get Revit version number (default string)
            int revNum = int.Parse(uiapp.Application.VersionNumber);

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Create Panelboard Schedules");

                    //string output = "Panel Schedules have been created for the following Electrical Equipment instances:\n";
                    ///define failure handling options of Transaction
                    FailureHandlingOptions options = trac.GetFailureHandlingOptions();
                    options.SetFailuresPreprocessor(new CMWElec_FailureHandlers.CircuitBreakerWarningSwallower());
                    trac.SetFailureHandlingOptions(options);

                    List<Element> outputElemList = new List<Element>();

                    foreach (Element eq in elecEquip)
                    {
                        //test if schedule already created for Electrical Equipment
                        bool exSched = CheckExistingSchedule(eq);
                        if (!exSched)
                        {
                            //get Max # Circuit Breakers of Element
                            int cbNum = GetCircuitBreakersNum(eq, revNum);

                            //collect parameters of Electrical Equipment family to compare
                            string famName = eq.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
                            string panName = eq.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
                            string panPhDemo = eq.LookupParameter("Phase Demolished").AsValueString();
                            string elecData = eq.LookupParameter("Electrical Data").AsString();

                            //test if schedule already created for Electrical Equipment

                            ElementId tempId = GetScheduleId(panName, panPhDemo, famName, elecData, doc, revNum, cbNum);

                            if (tempId != ElementId.InvalidElementId)
                            {
                                //add IFailuresPreprocessor Interface to address issues with 2022 and earlier Distribution Equipment schedule creation
                                PanelScheduleView.CreateInstanceView(doc, tempId, eq.Id);

                                //output += panName + "\n";
                                outputElemList.Add(eq);
                            }
                        }
                    }

                    trac.Commit();

                    //TaskDialog.Show("Created Panelboard Schedules", output);

                    //create output form for UI feedback
                    PanelSchedCreatedForm outputForm = new PanelSchedCreatedForm(outputElemList);
                    outputForm.ShowDialog();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    TaskDialog.Show("Panel Schedule Creation Failed", 
                        "Panel Schedules failed to create. Contact the BIM Team for assistance.");
                    
                    return Result.Failed;
                }
            }
        }

        #region GetCircuitBreakersNum
        public int GetCircuitBreakersNum(Element elem, int revVer)
        {
            int circuitBreakerNum = new int();

            if (revVer < 2022)
            {
                if (elem.LookupParameter("Max #1 Pole Breakers") != null)
                {
                    circuitBreakerNum = elem.LookupParameter("Max #1 Pole Breakers").AsInteger();
                }
            }
            else
            {
                if (elem.LookupParameter("Max Number of Single Pole Breakers") != null)
                {
                    circuitBreakerNum = elem.LookupParameter("Max Number of Single Pole Breakers").AsInteger();
                }
                else if (elem.LookupParameter("Max Number of Circuits") != null)
                {
                    circuitBreakerNum = elem.LookupParameter("Max Number of Circuits").AsInteger();
                }
            }

            return circuitBreakerNum;
        }
        #endregion //GetCircuitBreakersNum

        #region Panelboard Template Collections
        public List<PanelScheduleTemplate> BranchScheduleTemplates(Document document)
        {
            List<PanelScheduleTemplate> branchTemp = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleTemplate))
                .Cast<PanelScheduleTemplate>()
                .Where(x => x.GetPanelScheduleType() == PanelScheduleType.Branch)
                .ToList();

            return branchTemp;
        }

        public List<PanelScheduleTemplate> SwbdScheduleTemplates(Document document)
        {
            List<PanelScheduleTemplate> swbdTemp = new FilteredElementCollector(document)
                .OfClass(typeof(PanelScheduleTemplate))
                .Cast<PanelScheduleTemplate>()
                .Where(x => x.GetPanelScheduleType() == PanelScheduleType.Switchboard)
                .ToList();

            return swbdTemp;
        }
        #endregion //Panelboard Template Collections

        #region CheckExistingSchedule
        public bool CheckExistingSchedule(Element equip)
        {
            bool schedExists = false;
            //create ElementClassFilter for GetDependentElements
            ElementClassFilter filterCat = new ElementClassFilter(typeof(PanelScheduleView));

            //collect PanelScheduleView of Equipment
            List<ElementId> depElem = equip.GetDependentElements(filterCat).ToList();

            //check if PanelScheduleView exists
            if (depElem.Count > 0)
            {
                schedExists = true;
            }

            return schedExists;
        }
        #endregion //CheckExistingSchedule

        #region GetScheduleId
        public ElementId GetScheduleId(string panelName, string phase, string elemName, string electricalData, Document document, int revVer, int cbNumber)
        {
            //create invalid ElementId to compare
            ElementId tempId = ElementId.InvalidElementId;

            string cbStr = cbNumber.ToString();

            if (panelName != null && panelName != "Do Not Use" && panelName != "DO NOT USE" && phase == "None")
            {
                if (cbNumber != 0)
                {
                    if (elemName.Contains("Branch"))
                    {
                        //get applicable Branch Panelboard Schedule ElementId
                        if (Char.GetNumericValue(electricalData[6]) == 3)
                        {
                            foreach (PanelScheduleTemplate schTemp in BranchScheduleTemplates(document))
                            {
                                string testSchName = $"ONE Branch Panel - {cbStr} Circuit";
                                string schTempName = schTemp.Name;

                                if (testSchName == schTempName)
                                {
                                    tempId = schTemp.Id;
                                }
                            }
                        }
                        else
                        {
                            foreach (PanelScheduleTemplate schTemp in BranchScheduleTemplates(document))
                            {
                                string testSchName = "ONE Branch Panel - Single Phase";
                                string schTempName = schTemp.Name;

                                if (testSchName == schTempName)
                                {
                                    tempId = schTemp.Id;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (PanelScheduleTemplate schTemp in SwbdScheduleTemplates(document))
                        {
                            string testSchName;

                            //set empty int for circuit breaker values
                            int maxPoles;

                            //divide by 3 or remain as is depending on Revit version
                            if (revVer < 2022)
                            {
                                maxPoles = cbNumber / 3;
                            }
                            else
                            {
                                maxPoles = cbNumber;
                            }

                            string maxPolesString = maxPoles.ToString();
                            string schTempName = schTemp.Name;

                            if (elemName.Contains("Distribution"))
                            {
                                testSchName = $"ONE Distribution Panel - {maxPolesString} Space";
                                if (testSchName == schTempName)
                                {
                                    tempId = schTemp.Id;
                                }
                            }
                            else if (elemName.Contains("MCC"))
                            {
                                testSchName = $"ONE MCC - {maxPolesString} Space";
                                if (testSchName == schTempName)
                                {
                                    tempId = schTemp.Id;
                                }
                            }
                            else if (elemName.Contains("Switchboard"))
                            {
                                testSchName = $"ONE Switchboard - {maxPolesString} Space";
                                if (testSchName == schTempName)
                                {
                                    tempId = schTemp.Id;
                                }
                            }
                        }
                    }
                }
            }

            return tempId;
        }
        #endregion //GetScheduleId
    }
}
