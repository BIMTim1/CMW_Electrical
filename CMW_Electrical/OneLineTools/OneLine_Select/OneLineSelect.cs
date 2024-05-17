using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Selection;
using OLUpdateInfo;
using Autodesk.Revit.DB.Electrical;
using OneLineTools;
using CMW_Electrical;
using OneLineConnectAndPlace;
using System.Runtime.CompilerServices;

namespace OneLineSelect
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class OneLineSelect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            ISelectionFilter selFilter;
            string statusPrompt;
            Reference selItem = null;
            string selType;
            BuiltInCategory selectBic;

            Element selElem;

            if (activeView.ViewType == ViewType.DraftingView)
            {
                selFilter = new DetailItemSelectionFilter();
                selectBic = BuiltInCategory.OST_DetailComponents;
                statusPrompt = "Select a Detail Item reference.";
                selType = "Detail Item";
            }
            else
            {
                selFilter = new ElectricalEquipmnentSelectionFilter();
                selectBic = BuiltInCategory.OST_ElectricalEquipment;
                statusPrompt = "Select an Electrical Equipment family reference.";
                selType = "Electrical Equipment";
            }

            //!test if user already has elements selected
            //
            bool preSelect = true;
            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();

            if (selectedElementIds.Any())
            {
                //filter preselected list
                ElementCategoryFilter filter = new ElementCategoryFilter(selectBic);

                List<Element> filteredElemList =
                    new FilteredElementCollector(doc, selectedElementIds)
                    .WherePasses(filter)
                    .ToList();

                if (filteredElemList.Count() != 1)
                {
                    preSelect = false;
                    selElem = null;
                }
                else
                {
                    selElem = filteredElemList.First();
                }
            }
            else
            {
                preSelect = false;
                selElem = null;
            }

            if (!preSelect)
            {
                // Prompt user to select an item based on current view
                try
                {
                    selItem = uidoc.Selection.PickObject(ObjectType.Element, selFilter, statusPrompt);
                    //selItem = uidoc.Selection.PickObject(ObjectType.Element, statusPrompt); //debug only
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    return Result.Failed;
                }

                selElem = doc.GetElement(selItem);
            }

            //!test for EqConId parameter in project
            //

            BuiltInCategory bic;
            string compId;

            // Select connected element by EqConId
            if (selType == "Detail Item")
            {
                bic = BuiltInCategory.OST_ElectricalEquipment;
                DetailItemInfo detItemInfo = new DetailItemInfo(selElem);
                compId = detItemInfo.EqConId;
            }
            else
            {
                bic = BuiltInCategory.OST_DetailComponents;
                ElecEquipInfo equipInfo = new ElecEquipInfo(selElem);
                compId = equipInfo.EqConId;
            }

            if (compId == null || compId == "")
            {
                TaskDialog.Show("Tool canceled", "The Selected Reference does not have an associated element. The tool will now cancel.");
                return Result.Cancelled;
            }

            Element connectedElem = (from el in 
                                         new FilteredElementCollector(doc)
                                         .OfCategory(bic)
                                         .WhereElementIsNotElementType()
                                         .ToElements() 
                                     where el.LookupParameter("EqConId").AsString() == compId 
                                     select el)
                                     .First();

            var views = connectedElem.FindAllViewsWhereAllElementsVisible();
            ICollection<ElementId> connectedElemList = new List<ElementId>
            {
                connectedElem.Id
            };

            //if (null == views) throw new ArgumentNullException("no views");
            if (!views.Any()) return Result.Failed;

            View selView = views.First();

            uidoc.RequestViewChange(selView);

            uidoc.ShowElements(connectedElem);

            uidoc.Selection.SetElementIds(connectedElemList);

            return Result.Succeeded;
        }

        public class DetailItemSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Detail Items")
                {
                    return true;
                }
                return false;
            }
            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        public class ElectricalEquipmnentSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Electrical Equipment")
                {
                    return true;
                }
                return false;
            }
            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
    }
}
