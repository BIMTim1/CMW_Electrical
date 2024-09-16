using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Electrical;

namespace EquipmentTreeView
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class EquipmentTreeViewClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk info
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk info

            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;
            BuiltInParameter bipSupply = BuiltInParameter.RBS_ELEC_PANEL_SUPPLY_FROM_PARAM;
            BuiltInParameter bipPanName = BuiltInParameter.RBS_ELEC_PANEL_NAME;

            //collect source equipment
            List<FamilyInstance> source_equip = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .OfClass(typeof(FamilyInstance))
                .ToElements().Cast<FamilyInstance>()
                .Where(x => x.get_Parameter(bipSupply).AsString() == "")
                .ToList();

            //cancel if no elements
            if (!source_equip.Any())
            {
                errorReport = "There are no elements to be referenced by the tool. The tool will now cancel.";

                return Result.Cancelled;
            }

            //itereate through list of collected equipment
            List<FamilyInstance> filteredEquip = new List<FamilyInstance>();

            foreach (FamilyInstance eq in source_equip)
            {
                ISet<ElectricalSystem> elecSys = eq.MEPModel.GetElectricalSystems();

                foreach (ElectricalSystem cct in elecSys)
                {
                    if (cct.BaseEquipment != null && cct.BaseEquipment.Name == eq.get_Parameter(bipPanName).AsString() && cct.Elements.Size == 1)
                    {
                        foreach (Element elem in cct.Elements)
                        {
                            if (elem.Category.Name == "Electrical Equipment")
                            {
                                filteredEquip.Add(eq);
                            }
                        }
                    }
                }
            }

            //check if any elements match criteria
            if (!filteredEquip.Any())
            {
                errorReport = "There are no elements to be referenced by the tool. The tool will now cancel.";

                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
