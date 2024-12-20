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
using CMW_Electrical;

namespace CreateFeederInfo
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateFeederInformation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            Reference selRef;
            ISelectionFilter selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();
            string feederValue = "PANEL FEEDER";

            #region User Selection
            try
            {
                //prompt user for selection of Electrical Equipment
                selRef = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select an Electrical Equipment family instance.");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
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

            #region Check Selected Equipment Circuit
            //collect Equipment information
            FamilyInstance selEquip = doc.GetElement(selRef) as FamilyInstance;

            //check for created electrical circuit
            string equipName = selEquip.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();
            ISet<ElectricalSystem> equipCircuits = selEquip.MEPModel.GetElectricalSystems();

            ElectricalSystem equipCircuit = GetEquipmentCircuit(equipCircuits, equipName);

            if (equipCircuit == null)
            {
                errorReport = "Selected Electrical Equipment does not contain an Electrical Circuit. The tool will now cancel.";
                return Result.Cancelled;
            } 
            #endregion //Check Selected Equipment Circuit

            //update Equipment ElectricalSystem information
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW_Elec-Create Feeder Info");

                    equipCircuit.LookupParameter("E_IN_Schedule Filter").Set(feederValue);

                    trac.Commit();
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }

        #region GetEquipmentCircuit method
        public ElectricalSystem GetEquipmentCircuit(ISet<ElectricalSystem> electricalSystems, string equipmentName)
        {
            ElectricalSystem equipmentSystem = null;

            if (electricalSystems.Any())
            {
                foreach (ElectricalSystem electricalSystem in electricalSystems)
                {
                    //string baseEquipmentName = electricalSystem.BaseEquipment.Name;
                    FamilyInstance baseEquipment = electricalSystem.BaseEquipment;

                    if (baseEquipment == null || baseEquipment.Name == equipmentName)
                    {
                        equipmentSystem = electricalSystem;
                    }
                }
            }

            return equipmentSystem;
        }
        #endregion //GetEquipmentCircuit method
    }
}
