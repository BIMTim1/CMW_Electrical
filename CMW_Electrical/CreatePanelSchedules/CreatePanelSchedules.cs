using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq.Expressions;

namespace CreatePanelSchedules
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class CreatePanelSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            int count = 0;

            //BuiltInCategory value for Electrical Equipment
            //BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

            //get Revit version number (default string)
            int revNum = int.Parse(uiapp.Application.VersionNumber);

            //create named Transaction
            Transaction trac = new Transaction(doc);

            try
            {
                trac.Start("Create Panelboard Schedules");

                foreach (Element eq in ElecEquip(doc))
                {
                    //get Max # Circuit Breakers of Element
                    int cbNum = GetCircuitBreakersNum(eq, revNum);

                    //collect parameters of Electrical Equipment family to compare
                    string famName = eq.LookupParameter("Family").AsValueString();
                    string panName = eq.LookupParameter("Panel Name").AsString();
                    string panPhDemo = eq.LookupParameter("Phase Demolished").AsValueString();
                    string elecData = eq.LookupParameter("Electrical Data").AsString();

                    ElementId tempId = GetScheduleId(panName, panPhDemo, famName, elecData, doc, revNum, cbNum);

                    if (tempId != ElementId.InvalidElementId)
                    {
                        PanelScheduleView.CreateInstanceView(doc, tempId, eq.Id);
                        count += 1;
                    }
                }

                trac.Commit();
                TaskDialog.Show("Created Panelboard Schedules",
                    $"The tool has run successfully. {count} Panelboard Schedules were created");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                errorReport = ex.Message;
                TaskDialog.Show("Panel Schedule Creation Failed", "Panel Schedules failed to create. Contact the BIM Team for assistance.");
                return Result.Failed;
            }
        }
        public List<Element> ElecEquip(Document document)
        {
            //get Electrical Equipment BuiltInCategory
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

            List<Element> elecEquip = new FilteredElementCollector(document).OfCategory(bic).WhereElementIsNotElementType().ToList();

            return elecEquip;
        }

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

        public List<Element> BranchScheduleTemplates(Document document)
        {
            List<Element> branchTemp = new FilteredElementCollector(document).OfClass(typeof(PanelScheduleTemplate)).WherePasses(
                new ElementParameterFilter(
                    new FilterStringRule(
                        new ParameterValueProvider(
                            new ElementId(BuiltInParameter.ELEM_CATEGORY_PARAM_MT)),
                            new FilterStringEquals(),
                            "Panel Schedule Templates - Branch Panel", true))).ToList();

            return branchTemp;
        }

        public List<Element> SwbdScheduleTemplates(Document document)
        {
            List<Element> swbdTemp = new FilteredElementCollector(document).OfClass(typeof(PanelScheduleTemplate)).WherePasses(
                new ElementParameterFilter(
                    new FilterStringRule(
                        new ParameterValueProvider(
                            new ElementId(BuiltInParameter.ELEM_CATEGORY_PARAM_MT)),
                            new FilterStringEquals(),
                            "Panel Schedule Templates - Switchboard", true))).ToList();

            return swbdTemp;
        }

        public ElementId GetScheduleId(string panelName, string phase, string elemName, string electricalData, Document document, int revVer, int cbNumber)
        {
            //create invalid ElementId to compare
            ElementId tempId = ElementId.InvalidElementId;

            string cbStr = cbNumber.ToString();

            if (panelName != null && panelName != "Do Not Use" && panelName != "DO NOT USE" && phase == "None")
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

            return tempId;
        }
    }
}
