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
using System.Windows.Media.Imaging;
using OLUpdatePhaseInfo;

namespace OneLinePlaceEquip
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLinePlaceEquip : IExternalCommand
    {
        List<ElementId> _added_element_ids = new List<ElementId>();

        readonly BuiltInParameter bipFamily = BuiltInParameter.ELEM_FAMILY_PARAM;
        readonly BuiltInParameter bipType = BuiltInParameter.ELEM_TYPE_PARAM;
        readonly BuiltInParameter bipEquipName = BuiltInParameter.RBS_ELEC_PANEL_NAME;
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Information
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Information

            View activeView = doc.ActiveView;

            #region EqConId Check
            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Cancelled;
            }
            #endregion //EqConId Check

            string refCategory = "";
            string panNameParam = "";
            List<Element> filteredRefElements = new List<Element>();
            //List<string> formNameInfo = new List<string>();
            List<string> formTypeInfo = new List<string>();

            #region ActiveView Viewtype equals FloorPlan
            if (activeView.ViewType == ViewType.FloorPlan)
            {
                refCategory = "Detail Item";

                //check active Workset for E_Panels
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

                filteredRefElements = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_DetailComponents)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.get_Parameter(bipFamily).AsValueString().Contains("E_DI_OL_"))
                    .Where(x=>x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                    .Where(x=>x.LookupParameter("Panel Name - Detail") != null)
                    .ToList();

                if (!filteredRefElements.Any())
                {
                    //TaskDialog.Show("No Detail Items to Reference",
                    //    "There are no available Detail Items to assign to any equipment. The tool will now cancel.");
                    errorReport = "There are no available Detail Items to assign to any equipment.";

                    return Result.Cancelled;
                }

                //foreach (Element di in filteredRefElements)
                //{
                //    string input = di.LookupParameter("Panel Name - Detail").AsString() 
                //        + ", " 
                //        + di.get_Parameter(bipFamily).AsValueString() 
                //        + ": " 
                //        + di.get_Parameter(bipType).AsValueString();

                //    formNameInfo.Add(input);
                //}

                List<string> detailItemTypes = new List<string>()
                {
                    "Branch Panelboard",
                    "Transformer-Dry Type",
                    "Utility Transformer",
                    "Automatic Transfer Switch",
                    "Distribution Panelboard",
                    "Switchboard"
                };

                foreach (string item in detailItemTypes)
                {
                    formTypeInfo.Add(item);
                }

                panNameParam = "Panel Name - Detail";
            }
            #endregion //ActiveView Viewtype equals FloorPlan

            #region ActiveView ViewType equals DraftingView
            else if (activeView.ViewType == ViewType.DraftingView)
            {
                refCategory = "Electrical Equipment";

                filteredRefElements = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString() != "DO NOT USE" && x.LookupParameter("EqConId").AsString() == null || x.LookupParameter("EqConId").AsString() == "")
                    .ToList();

                if (!filteredRefElements.Any())
                {
                    //TaskDialog.Show("No Equipment Families to Reference", 
                    //    "There are no available Electrical Equipment families to assign to any Detail Item. The tool will now cancel.");
                    errorReport = "There are no available Electrical Equipment families to assign to any Detail Item.";

                    return Result.Cancelled;
                }

                //foreach (Element eq in filteredRefElements)
                //{
                //    string input = eq.get_Parameter(bipEquipName).AsString() 
                //        + ", " 
                //        + eq.get_Parameter(bipFamily).AsValueString() 
                //        + ": " 
                //        + eq.get_Parameter(bipType).AsValueString();

                //    formNameInfo.Add(input);
                //}

                List<string> equipTypes = new List<string>()
                {
                    "ATS",
                    "Bus",
                    "Panelboard",
                    "XFMR",
                    "Utility XFMR"
                };

                foreach (string item in equipTypes)
                {
                    formTypeInfo.Add(item);
                }

                panNameParam = "Panel Name";
            }
            #endregion //ActiveView ViewType equals DraftingView
            else
            {
                //TaskDialog.Show("Incorrect Active View", "Change your active view to a Floor Plan and then rerun the tool.");
                errorReport = "Change your active view to a Floor Plan and then rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            filteredRefElements = filteredRefElements.OrderBy(x => x.LookupParameter(panNameParam).AsString()).ToList();

            OLSelectDetItemForm form = new OLSelectDetItemForm(filteredRefElements, panNameParam, formTypeInfo, refCategory);
            form.ShowDialog();

            //cancel tool if user canceled form
            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                errorReport = "User canceled operation.";

                return Result.Cancelled;
            }

            //determine FamilySymbol and placement settings based on activeView
            string compName = form.cboxDetailItemList.SelectedItem.ToString().Split(',')[0];
            string compType = form.cboxFamilyTypeSelection.SelectedItem.ToString();
            List<FamilySymbol> famSymbols = null;

            PromptForFamilyInstancePlacementOptions famInstOptions = new PromptForFamilyInstancePlacementOptions();
            DetailItemInfo detItemInfo = null;
            ElecEquipInfo equipInfo = null;

            if (refCategory == "Detail Item") //settings for placing Electrical Equipment
            {
                Element selectedDetailItem = filteredRefElements[form.cboxDetailItemList.SelectedIndex];

                detItemInfo = new DetailItemInfo(selectedDetailItem);

                BuiltInCategory bicEq = BuiltInCategory.OST_ElectricalEquipment;

                if (compType == "Branch Panelboard" || compType == "Distribution Panelboard" || compType == "Switchboard")
                {
                    string compVolt = (detItemInfo.GetActualVoltage).ToString();

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
            }
            else //settings for placing a Detail Item
            {
                Element selectedElecEquip = filteredRefElements[form.cboxDetailItemList.SelectedIndex];

                equipInfo = new ElecEquipInfo(selectedElecEquip);

                famInstOptions.FaceBasedPlacementType = FaceBasedPlacementType.Default;

                BuiltInCategory bicDI = BuiltInCategory.OST_DetailComponents;

                if (equipInfo.EquipFamSymbol.FamilyName.Contains("Dry Type"))
                {
                    famSymbols = GetDryTypeTransformerSymbol(doc, bicDI, selectedElecEquip);
                }
                else
                {
                    famSymbols = new FilteredElementCollector(doc)
                        .OfCategory(bicDI).OfClass(typeof(FamilySymbol))
                        .WhereElementIsElementType()
                        .Cast<FamilySymbol>()
                        .Where(x => x.FamilyName != null && x.FamilyName.Contains(compType))
                        .ToList();
                }
            }

            //cancel if no FamilySymbol can be found
            if (!famSymbols.Any())
            {
                //TaskDialog.Show("No Family Symbol Found",
                //    "An Electrical Equipment Family Type could not be found to match the Detail Item selected. Load an applicable family from HIVE, then rerun the tool.");
                errorReport = $"A {refCategory} Family Type could not be found to match the Detail Item selected. " +
                    "Load an applicable family from HIVE, then rerun the tool.";

                return Result.Failed;
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
                    //TaskDialog.Show("User canceled", 
                    //    "Element placement canceled by user. The tool will now close.");
                    errorReport = "Element placement canceled by user. The tool will now close.";


                    return Result.Cancelled;
                }
            }

            //important to unsubscribe from events as quickly as possible
            app.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(
                OnDocumentChanged);

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Update Equipment from Detail Item");

                    Element placedItem = doc.GetElement(_added_element_ids.First());
                    
                    if (refCategory == "Detail Item")
                    {
                        equipInfo = new ElecEquipInfo(placedItem);

                        equipInfo.Name = detItemInfo.Name;
                    }
                    else
                    {
                        detItemInfo = new DetailItemInfo(placedItem);

                        detItemInfo.Name = equipInfo.Name;

                        new OLUpdatePhaseInfoClass().SetDetailItemPhaseInfo(detItemInfo, equipInfo);
                    }

                    //update EqConId values of selected elements
                    OLEqConIdUpdateClass updateId = new OLEqConIdUpdateClass();
                    updateId.OneLineEqConIdValueUpdate(equipInfo, detItemInfo, doc);

                    //remove extra families created by user duing PromptForFamilyInstancePlacement
                    if (_added_element_ids.Count() > 2)
                    {
                        for (int i = 2; i < _added_element_ids.Count(); i++)
                        {
                            Element elem = doc.GetElement(_added_element_ids[i]);

                            if (elem != null)
                            {
                                doc.Delete(_added_element_ids[i]);
                            }
                        }
                    }

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    //TaskDialog.Show("Error occurred", 
                    //    "An error has occurred that has prevented the tool from running. Contact the BIM team for assistance");
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
            }
        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }

        #region GetDryTypeTransformerSymbol
        /// <summary>
        /// Get List<FamilySymbol> information regarding the related Transformer Detail Item
        /// </summary>
        /// <param name="document"></param>
        /// <param name="bic"></param>
        /// <param name="equipInfo"></param>
        /// <returns>List of FamilySymbols that pass criteria.</returns>
        internal List<FamilySymbol> GetDryTypeTransformerSymbol(Document document, BuiltInCategory bic, Element element)
        {
            string kva = element.LookupParameter("kVA_Rating").AsDouble().ToString() + " kVA";

            List<FamilySymbol> famSymbols = 
                new FilteredElementCollector(document)
                .OfCategory(bic)
                .OfClass(typeof(FamilySymbol))
                .ToElements()
                .Where(x => x.Name == kva)
                .Cast<FamilySymbol>()
                .ToList();

            return famSymbols;
        }
        #endregion //GetDryTypeTransformerSymbol
    }
}
