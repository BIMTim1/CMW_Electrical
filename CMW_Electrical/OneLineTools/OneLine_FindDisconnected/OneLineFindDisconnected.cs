using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneLineTools;
using CMW_Electrical.OneLineTools.OneLine_FindDisconnected;

namespace OneLine_FindDisconnected
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineFindDisconnected : IExternalCommand
    {
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
            if (!eqConIdCheck.EqConIdCurrentValueExists(doc))
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. " +
                    "Contact the BIM team for assistance.";

                return Result.Failed;
            }

            //create variables to use depending on ActiveView.ViewType
            BuiltInCategory selBic;
            BuiltInCategory compBic;
            string canceledRef = "";
            List<Element> all_elements;

            //check ActiveView.ViewType
            if (activeView.ViewType == ViewType.DraftingView)
            {
                //select disconnected Electrical Equipment families to reference to object in current view
                compBic = BuiltInCategory.OST_ElectricalEquipment;
                selBic = BuiltInCategory.OST_DetailComponents;

                all_elements = new FilteredElementCollector(doc)
                    .OfCategory(selBic)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.LookupParameter("Family").AsValueString().Contains("E_DI_OL_"))
                    .Where(x => !x.LookupParameter("Family").AsValueString().Contains("Feeder") && !x.LookupParameter("Family").AsValueString().Contains("Circuit"))
                    .Where(x => x.LookupParameter("Panel Name - Detail") != null)
                    .Where(x=>x.LookupParameter("EqConId").AsString() != null && x.LookupParameter("EqConId").AsString() != "")
                    .ToList();

                canceledRef = "Detail Items";
            }
            else if (activeView.ViewType == ViewType.FloorPlan || activeView.ViewType == ViewType.ThreeD)
            {
                //select disconnected Detail Item families to reference to object in current view
                compBic = BuiltInCategory.OST_DetailComponents;
                selBic = BuiltInCategory.OST_ElectricalEquipment;

                all_elements = new FilteredElementCollector(doc)
                    .OfCategory(selBic)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId").AsString() != null && x.LookupParameter("EqConId").AsString() != "")
                    .ToList();

                canceledRef = "Electrical Equipment";
            }
            else //cancel if incorrect ViewType
            {
                errorReport = "Incorrect view type. Change your active view to a Floor Plan, 3D View, or One Line Schematic Drafting View and rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            //check if any elements from collection
            if (!all_elements.Any())
            {
                errorReport = $"There are no disconnected {canceledRef} Elements that can be referenced from this view. The tool will now cancel.";

                return Result.Cancelled;
            }

            //begin Transaction
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Find Disconnected One Line Elements");

                    //create list of elements to add to form
                    List<Element> discElements = new List<Element>();

                    //collect elements to compare to
                    List<Element> compElements = new FilteredElementCollector(doc)
                        .OfCategory(compBic)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.LookupParameter("EqConId").AsString() != null || x.LookupParameter("EqConId").AsString() != "")
                        .ToList();

                    foreach (Element elem in all_elements)
                    {
                        List<Element> compElems = (from el 
                                                   in compElements 
                                                   where el.LookupParameter("EqConId").AsString() == elem.LookupParameter("EqConId").AsString() 
                                                   select el)
                                                   .ToList();

                        if (!compElems.Any())
                        {
                            discElements.Add(elem);
                        }
                    }

                    //cancel tool if no disconnected elements found
                    if (!discElements.Any())
                    {
                        errorReport = $"There are no disconnected {canceledRef} elements that can be referenced from this view. The tool will now cancel.";

                        return Result.Cancelled;
                    }

                    //create list of DisconnectedElements
                    List<DisconnectedElement> discElemsList = new List<DisconnectedElement>();

                    foreach (Element el in discElements)
                    {
                        DisconnectedElement discElem = new DisconnectedElement(el);

                        discElemsList.Add(discElem);
                    }

                    //create form and launch
                    FindDisconnectedElementForm form = new FindDisconnectedElementForm(discElemsList);
                    form.ShowDialog();

                    //clear the EqConId of DisconnectedElement checked from form
                    foreach (DisconnectedElement discElem in discElemsList)
                    {
                        if (discElem.ClearEqConId)
                        {
                            discElem.GetFamilyInstance.LookupParameter("EqConId").Set("");
                        }
                    }

                    trac.Commit();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        var views = doc.GetElement(form._output).FindAllViewsWhereAllElementsVisible();
                        ICollection<ElementId> elemList = new List<ElementId>()
                        {
                            form._output
                        };

                        if (!views.Any())
                        {
                            errorReport = "There are no views that contain the selected element id;";

                            return Result.Succeeded;
                        }

                        //verify if active view can display element
                        bool changeView = true;
                        foreach (View v in views)
                        {
                            if (v.Id == activeView.Id)
                            {
                                changeView = false;
                            }
                        }

                        //change view if ActiveView not in list
                        if (changeView)
                        {
                            View selView = views.First();

                            uidoc.RequestViewChange(selView);
                        }

                        //zoom to elements
                        uidoc.ShowElements(elemList);

                        //select elements from ElementIds
                        uidoc.Selection.SetElementIds(elemList);
                    }

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
