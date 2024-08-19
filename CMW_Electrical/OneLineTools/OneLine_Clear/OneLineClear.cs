using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using OneLineTools;
using CMW_Electrical;
using CMW_Electrical.OneLineTools.OneLine_Clear;


namespace OneLine_Clear
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineClear : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            //cancel tool if EqConId Current Value parameter does not exist in project
            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //determine variables for class use
            ISelectionFilter selFilter;
            string refType;
            ICollection<ElementId> elemIdList = new List<ElementId>();
            Element selElem;

            //check for already selected element
            elemIdList = uidoc.Selection.GetElementIds();

            if (!elemIdList.Any())
            {
                //change variables based on active view type
                if (activeView.ViewType == ViewType.DraftingView)
                {
                    selFilter = new CMWElecSelectionFilter.DetailItemSelectionFilter();
                    refType = "Detail Item";
                }
                else if (activeView.ViewType == ViewType.FloorPlan || activeView.ViewType == ViewType.ThreeD)
                {
                    selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();
                    refType = "Electrical Equipment";
                }
                else
                {
                    errorReport = "Incorrect view type. This tool can only be run from a Floor Plan, 3D View, or Drafting View. Change your current view then launch the tool again.";
                    elementSet.Insert(activeView);

                    return Result.Cancelled;
                }

                try
                {
                    selElem = doc.GetElement(
                        uidoc.Selection.PickObject(
                            ObjectType.Element,
                            selFilter,
                            $"Select a {refType} instance to clear."));

                    //selElem = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, $"Select a {refType} instance to clear.")); //debug only
                }
                catch (OperationCanceledException ex)
                {
                    errorReport = "User canceled operation.";

                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
            else
            {
                //collect only first element of currently selected elements
                selElem = doc.GetElement(elemIdList.First());
            }

            //begin Transaction wrap
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Clear EqConId Value");

                    //check for connected elements
                    if (HasConnectedItem(doc, selElem))
                    {
                        OneLineClearForm form = new OneLineClearForm();
                        form.ShowDialog();

                        if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            throw new OperationCanceledException();
                        }
                    }

                    //clear EqConId of selected element
                    selElem.LookupParameter("EqConId").Set("");

                    trac.Commit();

                    TaskDialog results = new TaskDialog("CMW-Elec - Results")
                    {
                        TitleAutoPrefix = false,
                        MainInstruction = "Results:",
                        MainContent = $"Element {selElem.Id} has had its EqConId value cleared.",
                        CommonButtons = TaskDialogCommonButtons.Ok
                    };

                    results.Show();

                    return Result.Succeeded;
                }
                catch (OperationCanceledException ex)
                {
                    errorReport = "User canceled operation.";

                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
        }

        #region HasConnectedItem
        /// <summary>
        /// Check an input element if it has connected elements associated with it.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="selectedElement"></param>
        /// <returns>true / false if selected element has an associated element</returns>
        public bool HasConnectedItem(Document document, Element selectedElement)
        {
            //set BuiltInCategory by selected element (opposite)
            BuiltInCategory selBic =
                selectedElement.Category.Name == "Detail Items"
                ? BuiltInCategory.OST_ElectricalEquipment
                : BuiltInCategory.OST_DetailComponents;

            string selId = selectedElement.LookupParameter("EqConId").AsString();

            if (selId != null && selId != "")
            {
                List<Element> checkList = new FilteredElementCollector(document)
                    .OfCategory(selBic)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId").AsString() == selId)
                    .ToList();

                return checkList.Any();
            }

            return false;
        }
        #endregion //HasConnectedItem
    }
}
