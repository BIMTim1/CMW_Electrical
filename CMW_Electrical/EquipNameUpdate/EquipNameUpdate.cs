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

namespace EquipNameUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class EquipInfoUpdate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            //create count variables
            int countPanName = 0;
            int countLoadname = 0;
            int countNonName = 0;
            int countNonLoad = 0;

            //get Revit version number
            int revVer = int.Parse(uiapp.Application.VersionNumber);

            //get collection variables
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;
            ElementClassFilter equipFil = new ElementClassFilter(typeof(PanelScheduleView));

            //collect all Electrical Equipment Families
            List<FamilyInstance> all_equip = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            //create new Transaction
            Transaction trac = new Transaction(doc);

            try
            {
                trac.Start("Update Electrical Equipment Information");

                foreach (FamilyInstance eq in all_equip)
                {
                    string eq_lbl = eq.LookupParameter("Panel Name").AsString();
                    string eq_type = eq.LookupParameter("Family").AsValueString();

                    //get PanelScheduleView associated to equipment
                    IList<ElementId> panSchedId = eq.GetDependentElements(equipFil);

                    //test if PanelScheduleView exists
                    if (panSchedId.Count > 0)
                    {
                        //update Panel Schedule Name to match updated Panel Name
                        Element panSched = doc.GetElement(panSchedId.First());
                        Parameter schedName = panSched.LookupParameter("Panel Schedule Name");
                        schedName.Set(eq_lbl);
                        countPanName += 1;
                    }
                    else
                    {
                        countNonName += 1;
                    }

                    List<ElectricalSystem> eqSys = new List<ElectricalSystem>();
                    //verify Revit version and how to collect ElectricalSystem info
                    if (revVer < 2021)
                    {
                        eqSys = EquipCircuits2020(eq);
                    }
                    else
                    {
                        eqSys = EquipCircuits2021(eq);
                    }

                    try
                    {
                        if (eqSys != null)
                        {
                            if (eq_type.Contains("Transformer") & !(eq_type.Contains("Utility")))
                            {
                                List<Parameter> loadNames = new List<Parameter>();
                                string downPan = "";

                                foreach (ElectricalSystem cct in eqSys)
                                {
                                    string baseEq = cct.BaseEquipment.LookupParameter("Panel Name").AsString();
                                    if (baseEq == eq_lbl)
                                    {
                                        ElementSet sysElems = cct.Elements;

                                        foreach (Element elem in sysElems)
                                        {
                                            //assumes only one sub panel connected to Low Voltage XFMR
                                            downPan = elem.LookupParameter("Panel Name").AsString();
                                        }
                                    }
                                    else
                                    {
                                        //get Load Name parameter of Transformer
                                        loadNames.Add(cct.LookupParameter("Load Name"));
                                    }
                                }

                                //set Load Name of Transformer
                                string setName = eq_lbl + " (" + downPan + ")";
                                loadNames.First().Set(setName);
                                countLoadname += 1;
                            }
                            else
                            {
                                foreach (ElectricalSystem cct in eqSys)
                                {
                                    string baseEq = cct.BaseEquipment.LookupParameter("Panel Name").AsString();
                                    if (baseEq != eq_lbl)
                                    {
                                        //set Load Name of Electrical Equipment
                                        Parameter loadName = cct.LookupParameter("Load Name");
                                        loadName.Set(eq_lbl);
                                        countLoadname += 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            countNonLoad += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        countNonLoad += 1;
                    }
                }

                trac.Commit();
                TaskDialog.Show("Update Equipment Info Succeeded",
                    $"{countPanName} Panel Schedule Names were updated.\n{countLoadname} Load Names were updated.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                trac.RollBack();
                errorReport = ex.Message;
                TaskDialog.Show("Update Equipment Info Failed",
                    "An error occured during the update process. Contact the BIM Team to resolve the issue.");
                return Result.Failed;
            }
        }

        public List<ElectricalSystem> EquipCircuits2021(FamilyInstance equip)
        {
            List<ElectricalSystem> equip_ccts = equip.MEPModel.GetElectricalSystems().ToList();

            return equip_ccts;
        }

        public List<ElectricalSystem> EquipCircuits2020(FamilyInstance equip)
        {
            List<ElectricalSystem> cct_set = equip.MEPModel.ElectricalSystems.Cast<ElectricalSystem>().ToList();

            return cct_set;
        }
    }
}
