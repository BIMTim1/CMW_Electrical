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

            //!test for EqConId parameter in project
            

            if (activeView.ViewType == ViewType.DraftingView)
            {
                selFilter = new DetailItemSelectionFilter();
                statusPrompt = "Select a Detail Item reference.";
            }
            else
            {
                selFilter = new ElectricalEquipmnentSelectionFilter();
                statusPrompt = "Select an Electrical Equipment family reference.";
            }

            //prompt user to select an item based on current view
            try
            {
                //selItem = uidoc.Selection.PickObject(ObjectType.Element, selFilter, statusPrompt);
                selItem = uidoc.Selection.PickObject(ObjectType.Element, statusPrompt); //debug only
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                return Result.Failed;
            }

            Element selElem = doc.GetElement(selItem);

            var relevantViewList = FindAllViewsThatCanDisplayElements(doc);

            ElementId selElemId = selElem.Id;
            var idsToCheck = new List<ElementId>()
            {
                selElemId
            };

            IEnumerable<View> viewList = from v 
                                  in relevantViewList 
                                  let idList = 
                                  new FilteredElementCollector(doc, v.Id)
                                  .WhereElementIsNotElementType()
                                  .ToElementIds() 
                                  where !idsToCheck.Except(idList).Any() 
                                  select v;

            return Result.Succeeded;
        }

        private static IEnumerable<View> FindAllViewsThatCanDisplayElements(this Document doc)
        {
            var filter = new ElementMulticlassFilter(new List<Type>()
            {
                typeof(View3D),
                typeof(ViewPlan),
                typeof(ViewSection)
            });

            return new FilteredElementCollector(doc).WherePasses(filter).Cast<View>().Where(v => !v.IsTemplate);
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
