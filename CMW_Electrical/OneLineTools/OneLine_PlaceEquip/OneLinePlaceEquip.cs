using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMW_Electrical;
using CMW_Electrical.OneLineTools.OneLine_PlaceEquip;

namespace OneLinePlaceEquip
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLinePlaceEquip : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //cancel tool if not a FloorPlan
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Incorrect Active View", "Change your active view to a Floor Plan and then rerun the tool.");
                return Result.Cancelled;
            }

            //add a Workset check for E_Panels?

            List<Element> filteredDetItems = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => x.LookupParameter("Family").AsValueString().Contains("E_DI_OL_") && x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                .ToList();

            if (!filteredDetItems.Any())
            {
                TaskDialog.Show("No Detail Items to Reference", 
                    "There are no available Detail Items to assign to any equipment. The tool will now cancel.");
                
                return Result.Cancelled;
            }

            List<string> detInfo = new List<string>();

            foreach (Element detItem in filteredDetItems)
            {
                string detName = detItem.LookupParameter("Panel Name - Detail").AsString();
                string famName = detItem.LookupParameter("Family").AsValueString();
                string output = detName + ",  " + famName;

                detInfo.Add(output);
            }

            OLSelectDetItemForm form = new OLSelectDetItemForm(detInfo);
            form.ShowDialog();

            //cancel tool if user canceled form
            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            string compName = form.cboxDetailItemList.SelectedItem.ToString().Split(',')[0];
            string compType = form.cboxDetailItemList.SelectedItem.ToString().Split(' ')[1];

            Element selectedDetailItem = (new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => x.LookupParameter("Panel Name - Detail").AsString() == compName))
                .First();

            DetailItemInfo detailItemInfo = new DetailItemInfo(selectedDetailItem);

            PromptForFamilyInstancePlacementOptions famInstOptions = new PromptForFamilyInstancePlacementOptions();

            FamilySymbol famSymbol = null;

            //compare family name to available Revit family types


            //uidoc.PromptForFamilyInstancePlacement();

            return Result.Succeeded;
        }
    }
}
