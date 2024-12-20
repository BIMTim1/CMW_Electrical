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

            //check if selected element has an Electrical Circuit
            FamilyInstance selEquip = doc.GetElement(selRef) as FamilyInstance;

            //check for created electrical circuit
            string noCircuitText = "Selected Electrical Equipment does not contain an Electrical Circuit. The tool will now cancel.";
            ISet<ElectricalSystem> equipCircuits = selEquip.MEPModel.GetElectricalSystems();

            if (!equipCircuits.Any())
            {
                errorReport = noCircuitText;

                return Result.Cancelled;
            }

            ElectricalSystem equipCircuit = null;
            string equipName = selEquip.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();

            //iterate through ISet of ElectricalSystems
            foreach (ElectricalSystem elecSys in equipCircuits)
            {
                string baseEquipName = elecSys.BaseEquipment.Name;

                if (baseEquipName != equipName)
                {
                    equipCircuit = elecSys;
                }
            }

            if (equipCircuit == null)
            {
                errorReport = noCircuitText;

                return Result.Cancelled;
            }

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
    }
}
