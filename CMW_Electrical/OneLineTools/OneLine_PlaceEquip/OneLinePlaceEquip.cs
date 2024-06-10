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
using System.Net;
using OneLineTools;

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

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                TaskDialog.Show("Parameter Does not Exist",
                    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.");
                return Result.Cancelled;
            }

            //cancel tool if not a FloorPlan
            if (activeView.ViewType != ViewType.FloorPlan)
            {
                TaskDialog.Show("Incorrect Active View", "Change your active view to a Floor Plan and then rerun the tool.");
                return Result.Cancelled;
            }

            //add a Workset check for E_Panels?
            Workset panelWorkset = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .Where(x => x.Name == "E_Panels")
                .ToList()
                .First();

            WorksetTable worksetTable = doc.GetWorksetTable();

            if (worksetTable.GetActiveWorksetId() != panelWorkset.Id)
            {
                worksetTable.SetActiveWorksetId(panelWorkset.Id);
            }

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

            if (compType == "Branch Panelboard" || compType == "Distribution Panelboard" || compType == "Switchboard")
            {
                string compVolt = (detailItemInfo.Voltage / 10.763910416709711538461538461538).ToString();

                if (compVolt == "0")
                {
                    compVolt = "208";
                }

                famSymbols = new FilteredElementCollector(doc)
                    .OfCategory(bicEq)
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .Where(x => x.FamilyName != null && x.FamilyName.Contains($"{compType}_{compVolt}"))
                    .ToList();

                if (compType != "Switchboard")
                {
                    famInstOptions.FaceBasedPlacementType = FaceBasedPlacementType.PlaceOnVerticalFace;
                }
            }
            else
            {
                famSymbols = new FilteredElementCollector(doc)
                    .OfCategory(bicEq)
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .Where(x => x.FamilyName != null && x.FamilyName.Contains(compType))
                    .ToList();
            }

            //cancel if no FamilySymbol can be found
            if (!famSymbols.Any())
            {
                TaskDialog.Show("No Family Symbol Found",
                    "An Electrical Equipment Family Type could not be found to match the Detail Item selected. Load an applicable family from HIVE, then rerun the tool.");

                return Result.Cancelled;
            }

            FamilySymbol famSymbol = famSymbols.First();

            //subscribe to DocumentChangedEventArgs to collect placed elements
            app.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(
                OnDocumentChanged);

            try
            {
                uidoc.PromptForFamilyInstancePlacement(famSymbol, famInstOptions);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                //user has to cancel out of operation
                if (!_added_element_ids.Any())
                {
                    TaskDialog.Show("User canceled", 
                        "Element placement canceled by user. The tool will now close.");
                    
                    return Result.Cancelled;
                }
                //else
                //{
                //    TaskDialog.Show("User canceled with elements placed", 
                //        $"{_added_element_ids.Count()} elements have been added to the active document.");
                //}
            }
            //important to unsubscribe from events as quickly as possible
            app.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(
                OnDocumentChanged);

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec_Update Equipment from Detail Item");

                    Element placedEquip = doc.GetElement(_added_element_ids.First());
                    string outputString = "";

                    //debug string
                    //foreach (ElementId id in _added_element_ids)
                    //{
                    //    outputString = outputString + id.ToString() + "\n";
                    //}

                    //TaskDialog.Show("Debug Message", $"Element Ids created:\n{outputString}Element in use:\n{placedEquip.Id}");
                    
                    ElecEquipInfo equipInfo = new ElecEquipInfo(placedEquip);
                    equipInfo.Name = detailItemInfo.Name;
                    //circuit?

                    //update EqConId values of selected elements
                    OLEqConIdUpdateClass updateId = new OLEqConIdUpdateClass();
                    updateId.OneLineEqConIdValueUpdate(equipInfo, detailItemInfo, doc);

                    //remove extra families created by user duing PromptForFamilyInstancePlacement
                    if (_added_element_ids.Count() > 2)
                    {
                        foreach (ElementId eid in _added_element_ids)
                        {
                            if (eid.ToString() != _added_element_ids[0].ToString() && eid.ToString() != _added_element_ids[1].ToString())
                            {
                                doc.Delete(eid);
                            }
                        }
                    }

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error occurred", 
                        "An error has occurred that has prevented the tool from running. Contact the BIM team for assistance");

                    return Result.Failed;
                }
            }
            }
        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }
    }
}
