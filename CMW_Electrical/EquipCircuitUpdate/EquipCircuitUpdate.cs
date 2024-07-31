using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipCircuitUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class EquipCircuitUpdate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //BuiltInCategory
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

            List<FamilyInstance> all_equip = 
                new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .Where(x=>HasCircuit(x))
                .ToList();

            //cancel tool if no applicable elements
            if (!all_equip.Any())
            {
                errorReport = "No Electrical Equipment families in the current project have circuits. The tool will now cancel.";
                elementSet.Insert(doc.ActiveView);

                return Result.Cancelled;
            }

            //create Transaction information and start
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Update Equipment Circuit Ratings");

                    int count = 0;

                    //iterate through collected Electrical Equipment
                    foreach (FamilyInstance fam in all_equip)
                    {
                        //collect parameter values to update from
                        double rating = fam.get_Parameter(BuiltInParameter.RBS_ELEC_MAINS).AsDouble();
                        double bussing = fam.LookupParameter("E_Bussing").AsDouble();

                        //collect associated ElectricalSystems from equipment
                        ISet<ElectricalSystem> assoc_circuits = fam.MEPModel.GetElectricalSystems();

                        foreach (ElectricalSystem cct in assoc_circuits)
                        {
                            if (cct.BaseEquipment.Name != fam.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString())
                            {
                                //update ElectricalSystem Rating and Frame
                                cct.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_RATING_PARAM).Set(rating);
                                cct.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_FRAME_PARAM).Set(bussing);
                            }
                        }

                        count++;
                    }

                    trac.Commit();

                    TaskDialog results = new TaskDialog("CMW-Elec - Results")
                    {
                        TitleAutoPrefix = false,
                        CommonButtons = TaskDialogCommonButtons.Ok,
                        MainInstruction = "Results:",
                        MainContent = $"{count} Electrical Equipment families have had their associated " +
                        $"Electrical Circuit Rating and Frame parameter values updated."
                    };

                    results.Show();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    foreach (FamilyInstance famInst in all_equip)
                    {
                        elementSet.Insert(famInst);
                    }

                    return Result.Failed;
                }
            }
        }

        #region HasCircuit
        /// <summary>
        /// Used to verify if the Electrical Equipment FamilyInstance has a source feed.
        /// </summary>
        /// <param name="equip"></param>
        /// <returns>bool hasCircuit</returns>
        internal bool HasCircuit(FamilyInstance equip)
        {
            bool hasCircuit = false;

            ISet<ElectricalSystem> circuits = equip.MEPModel.GetElectricalSystems();

            foreach (ElectricalSystem cct in circuits)
            {
                if (cct.BaseEquipment.Name != equip.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString())
                {
                    hasCircuit = true;
                }
            }

            return hasCircuit;
        }
        #endregion //HasCircuit
    }
}
