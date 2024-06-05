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
using Autodesk.Revit.DB.Events;

namespace OneLinePlaceEquip
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLinePlaceEquip : IExternalCommand
    {
        List<ElementId> _added_element_ids = new List<ElementId>();
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
                .Where(x => x.LookupParameter("Family").AsValueString().Contains("E_DI_OL_") && x.LookupParameter("EqConId").AsString() == null && x.LookupParameter("Panel Name - Detail") != null)// || x.LookupParameter("EqConId").AsString() == "")
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
            //string compType = form.cboxDetailItemList.SelectedItem.ToString().Split(' ')[1];

            Element selectedDetailItem = (new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x => x.LookupParameter("Panel Name - Detail") != null && x.LookupParameter("Panel Name - Detail").AsString() == compName))
                .First();

            DetailItemInfo detailItemInfo = new DetailItemInfo(selectedDetailItem);

            PromptForFamilyInstancePlacementOptions famInstOptions = new PromptForFamilyInstancePlacementOptions();

            List<FamilySymbol> famSymbols = null;

            //compare family name to available Revit family types
            string compType = form.cboxFamilyTypeSelection.SelectedItem.ToString();

            BuiltInCategory bicEq = BuiltInCategory.OST_ElectricalEquipment;

            if (compType == "Panelboard" || compType == "Distribution Panelboard")
            {
                string compVolt = detailItemInfo.Voltage.ToString();

                if (compVolt == "0")
                {
                    compVolt = "208";
                }

                famSymbols = new FilteredElementCollector(doc)
                    .OfCategory(bicEq)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(x => x.LookupParameter("Family Name").AsString().Contains(compType)
                    && x.LookupParameter("Family Name").AsString().Contains(compVolt)).ToList();

                famInstOptions.FaceBasedPlacementType = FaceBasedPlacementType.PlaceOnVerticalFace;
            }
            else
            {
                famSymbols = new FilteredElementCollector(doc)
                    .OfCategory(bicEq)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(x => x.LookupParameter("Family Name").AsString().Contains(compType)).ToList();

                famInstOptions.FaceBasedPlacementType = FaceBasedPlacementType.PlaceOnWorkPlane;
            }

            //cancel if no FamilySymbol can be found
            if (!famSymbols.Any())
            {
                TaskDialog.Show("No Family Symbol Found", 
                    "An Electrical Equipment Family Type could not be found to match the Detail Item selected. Load an applicable family from HIVE, then rerun the tool.");
                
                return Result.Cancelled;
            }

            FamilySymbol famSymbol = famSymbols.First();

            using (TransactionGroup tracGroup = new TransactionGroup(doc))
            {
                tracGroup.Start("CMWElec - Place Equipment");

                //subscribe to DocumentChangedEventArgs to collect placed elements
                app.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(
                    OnDocumentChanged);

                //opens in its own Transaction
                uidoc.PromptForFamilyInstancePlacement(famSymbol, famInstOptions);

                //important to unsubscribe from events as quickly as possible
                app.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(
                    OnDocumentChanged);

                using (Transaction trac = new Transaction(doc))
                {
                    try
                    {
                        trac.Start("Update Equipment from Detail Item");

                        Element placedEquip = doc.GetElement(_added_element_ids.First());

                        ElecEquipInfo equipInfo = new ElecEquipInfo(placedEquip);
                        equipInfo.Name = detailItemInfo.Name;
                        //circuit?

                        //remove extra families created by user duing PromptForFamilyInstancePlacement
                        if (_added_element_ids.Count() > 1)
                        {
                            _added_element_ids.Remove(placedEquip.Id);

                            foreach (ElementId id in _added_element_ids)
                            {
                                doc.Delete(id);
                            }
                        }

                        trac.Commit();
                    }
                    catch (Exception ex)
                    {
                        return Result.Failed;
                    }
                }

                tracGroup.Assimilate();

                return Result.Succeeded;
            }
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }
    }
}
