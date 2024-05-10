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

namespace OneLineCopy
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineCopy : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            if (activeView.ViewType != ViewType.DraftingView)
            {
                TaskDialog.Show("Incorrect View Type",
                    "This tool can only be run in a Drafting View. Change the current view to a Drafting View and rerun the tool.");
                return Result.Cancelled;
            }

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            ICollection<Element> selectedElems = new List<Element>();

            if (selectedIds.Count() == 0)
            {
                try
                {
                    //ISelectionFilter selFilter = new DetailItemSelectionFilter();
                    //selectedElems = uidoc.Selection.PickElementsByRectangle(selFilter, "Select One Line elements to copy.");
                    selectedElems = uidoc.Selection.PickElementsByRectangle("Select One Line elements to copy."); //debug only
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    return Result.Cancelled;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error occurred", "An error occurred that has prevented the tool from running. Contact the BIM team for assistance.");
                    return Result.Failed;
                }
            }
            else
            {
                foreach (ElementId elemId in selectedIds)
                {
                    Element elem = doc.GetElement(elemId);
                    selectedElems.Add(elem);
                }
            }

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
    }
}
