#region UsingStatements
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneLineTools;
using System.Windows.Forms;
using CMW_Electrical.OneLineTools.OneLine_FindDuplicates;
#endregion //UsingStatements

namespace OneLine_FindDuplicates
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineFindDuplicates : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.DB.View activeView = doc.ActiveView;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport =
                    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Failed;
            }

            BuiltInCategory selBic;
            List<Element> all_elements;

            string noDupsMessage = "There are no duplicate elements that can be referenced from this view.";

            //check activeView View Type
            if (activeView.ViewType == ViewType.DraftingView)
            {
                selBic = BuiltInCategory.OST_DetailComponents;

                all_elements = new FilteredElementCollector(doc, activeView.Id)
                    .OfCategory(selBic)
                    .ToElements()
                    .Where(x => !x.LookupParameter("Family").AsValueString().Contains("Circuit"))
                    .Where(x => !x.LookupParameter("Family").AsValueString().Contains("Feeder"))
                    .Where(x => x.LookupParameter("EqConId").AsString() != null || x.LookupParameter("EqConId").AsString() != "")
                    .ToList();
            }
            else if (activeView.ViewType == ViewType.FloorPlan || activeView.ViewType == ViewType.ThreeD)
            {
                selBic = BuiltInCategory.OST_ElectricalEquipment;

                all_elements = new FilteredElementCollector(doc)
                    .OfCategory(selBic)
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId") != null && x.LookupParameter("EqConId").AsString() != null && x.LookupParameter("EqConId").AsString() != "")
                    .ToList();
            }
            else //cancel if not FloorPlan or DraftingView
            {
                errorReport = "Tool cannot run from this view. Change your active view to a Floor Plan or Drafting View and rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //check if any elements exist in collection
            if (!all_elements.Any())
            {
                errorReport = "No elements have been assigned ids through the CMW - Electrical tool that can be accessed through this view. " +
                    "The tool will now cancel.";

                return Result.Cancelled;
            }

            //iterate through list of potential objects for duplicates
            int currentEqConIds = eqConIdCheck.GetEqConIdCurrentValue(doc).AsInteger();
            int i = 1;

            List<string> dupElemIds = new List<string>();

            while (i < currentEqConIds)
            {
                int tempCount = 0;
                string compVal = $"EqId{i}";

                foreach (Element e in all_elements)
                {
                    if (e.LookupParameter("EqConId").AsString() == compVal)
                    {
                        tempCount++;
                    }
                }

                if (tempCount > 1)
                {
                    dupElemIds.Add(compVal);
                }

                i++;
            }

            //check for duplicates
            if (!dupElemIds.Any())
            {
                errorReport = noDupsMessage;

                return Result.Cancelled;
            }

            //begin transaction and wrap in a using statement
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMW-Elec - Find Duplicates");

                    List<DuplicateElementData> dupElementDataList = new List<DuplicateElementData>();

                    var filteredElements = all_elements
                        .Where(x => dupElemIds.Contains(x.LookupParameter("EqConId").AsString()))
                        .GroupBy(e =>e.LookupParameter("EqConId").AsString())
                        .Select(x => x.First())
                        .ToList();

                    foreach (Element e in filteredElements)
                    {
                        DuplicateElementData dupElemData = new DuplicateElementData(e);

                        dupElementDataList.Add(dupElemData);
                    }

                    //create and launch WindowsForm
                    FindDuplicateElementForm form = new FindDuplicateElementForm(dupElementDataList);
                    form.ShowDialog();

                    if (form.DialogResult == DialogResult.Cancel)
                    {
                        return Result.Succeeded;
                    }

                    //collect objects with the selected EqConId value
                    ICollection<ElementId> elemIds = 
                        (from e 
                         in all_elements 
                         where e.LookupParameter("EqConId").AsString() == form._output 
                         select e.Id)
                         .ToList();

                    uidoc.ShowElements(elemIds);

                    uidoc.Selection.SetElementIds(elemIds);

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
        }
    }
}
