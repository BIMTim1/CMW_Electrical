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
using Autodesk.Revit.UI.Events;
using System.Diagnostics;
using CMW_Electrical.OneLineTools.OneLine_Find;

namespace OneLineFind
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineFind : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = 
                    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Failed;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Find Unassociated Elements");

                    //collect Electrical Equipment and Detail Item elements that are not associated to a Schematic item
                    List<BuiltInCategory> mCats = new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_ElectricalEquipment,
                        BuiltInCategory.OST_DetailComponents
                    };

                    //create ElementMulticategoryFilter to be used by FilteredElementCollectors
                    ElementMulticategoryFilter mCatFilter = new ElementMulticategoryFilter(mCats);

                    //clear Comments parameter of items that may have not been assigned before
                    List<Element> elems = 
                        new FilteredElementCollector(doc)
                        .WherePasses(mCatFilter)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.LookupParameter("EqConId").AsString() == null || 
                        x.LookupParameter("EqConId").AsString() == "")
                        .Where(x => x.LookupParameter("Panel Name") != null ||
                        x.LookupParameter("Panel Name - Detail") != null)
                        .ToList();

                    if (!elems.Any())
                    {
                        errorReport = "There are no unassigned Electrical Equipment or Detail Item families. The tool will now cancel.";
                        elementSet.Insert(doc.ActiveView);

                        return Result.Cancelled;
                    }

                    //convert elements into List<ElementData>
                    List<ElementData> elemDataList = new List<ElementData>();

                    foreach (Element elem in elems)
                    {
                        ElementData elemData = new ElementData(elem);
                        elemDataList.Add(elemData);
                    }

                    //initialize form
                    OneLineFindForm form = new OneLineFindForm(elemDataList.OrderBy(x=>x.EPanelName).ToList());
                    form.ShowDialog();

                    //if (form.outputElementId != null)
                    //{
                    //    MessageBox.Show($"Selected Element Id:\n{form.outputElementId}");
                    //}

                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }

                    //collect views that the selected ElementId is visible in
                    var views = doc.GetElement(form.outputElementId).FindAllViewsWhereAllElementsVisible();
                    ICollection<ElementId> elemList = new List<ElementId>()
                    {
                        form.outputElementId
                    };

                    if (!views.Any()) return Result.Failed;

                    List<ElementId> colViewIds = (from v in views select v.Id).ToList();

                    //View selView = views.First();
                    View selView = null;

                    List<View> openViews = new ViewCollector().GetOpenViews(doc);

                    foreach (View ov in openViews)
                    {
                        if (colViewIds.Contains(ov.Id))
                        {
                            selView = ov;
                        }
                    }

                    if (selView == null)
                    {
                        selView = views.First();
                    }

                    trac.Commit();

                    uidoc.RequestViewChange(selView);

                    uidoc.ShowElements(elemList);

                    uidoc.Selection.SetElementIds(elemList);

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
