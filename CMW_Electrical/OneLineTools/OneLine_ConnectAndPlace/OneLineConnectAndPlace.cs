using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLineConnectAndPlace
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLineConnectAndPlace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //stop tool if activeView is not a Drafting View
            if (activeView.ViewType != ViewType.DraftingView)
            {
                TaskDialog.Show("Incorrect View Type", "Open a Drafting View that contains your One-Line Diagram and rerun the tool.");
                return Result.Failed;
            }

            //collect equipment that has NOT been circuited
            List<Element> uncircuitedEquip = 
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.LookupParameter("Supply From").AsString() == "")
                .ToList();

            List<string> equipNames = (from pnl in uncircuitedEquip select pnl.LookupParameter("Panel Name").AsString()).ToList();

            //start SelectEquipmentToReference Windows Form
            SelectEquipmentToReferenceForm equipSelectForm = new SelectEquipmentToReferenceForm(equipNames);
            equipSelectForm.ShowDialog();

            if (equipSelectForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Failed;
            }

            //collect Detail Item families
            List<FamilySymbol> detailItemTypes = 
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .ToList();

            Element selectedEquip = null;

            //select the Electrical Equipment FamilyInstance if selected by user
            if (equipSelectForm.rbtnUseEquipment.Checked)
            {
                selectedEquip = 
                    new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.LookupParameter("Panel Name").AsString() == equipSelectForm.cboxEquipNameSelect.SelectedItem.ToString())
                    .First();

                string famType = selectedEquip.LookupParameter("Family").AsValueString();

                string detailItemRef = null;

                if (famType.Contains("Branch"))
                {
                    detailItemRef = "Panelboard";
                }
                else if (famType.Contains("Distribution") || famType.Contains("Switchboard"))
                {
                    detailItemRef = "Bus";
                }
                else if (famType.Contains("Transformer-Dry Type"))
                {
                    detailItemRef = "XFMR";
                }
            }
            else if (equipSelectForm.rbtnDontUseEquipment.Checked)
            {
                
            }

            return Result.Succeeded;
        }
    }
}
