using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneLineTools;
using CMW_Electrical;
using System.Drawing.Drawing2D;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;
using CMW_Electrical.OneLineTools.OneLine_Remove;

namespace OneLine_Remove
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class OneLineRemove : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.DB.View activeView = doc.ActiveView;

            ISelectionFilter selFilter;
            string statusPrompt;
            Reference selItem = null;
            //string selType;
            BuiltInCategory selectBic;


            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Failed;
            }

            //check ActiveView ViewType for variables through rest of code
            if (activeView.ViewType == ViewType.DraftingView)
            {
                selFilter = new CMWElecSelectionFilter.DetailItemSelectionFilter();
                selectBic = BuiltInCategory.OST_ElectricalEquipment;
                statusPrompt = "Select a Detail Item reference.";
                //selType = "Detail Item";
            }
            else if (activeView.ViewType == ViewType.FloorPlan)
            {
                selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();
                selectBic = BuiltInCategory.OST_DetailComponents;
                statusPrompt = "Select an Electrical Equipment reference.";
                //selType = "Electrical Equipment";
            }
            else //cancel tool if ActiveView is not FloorPlan or DraftingView
            {
                errorReport = "The current view does not support selection of elements for tool. The tool will now cancel.";

                return Result.Cancelled;
            }

            //prompt user for selection
            try
            {
                //selItem = uidoc.Selection.PickObject(ObjectType.Element, selFilter, statusPrompt);
                selItem = uidoc.Selection.PickObject(ObjectType.Element, statusPrompt); //debug only
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex) //cancel tool if user hit Esc
            {
                errorReport = "User canceled operation.";

                return Result.Cancelled;
            }
            catch (Exception ex) //error handling for unknown
            {
                errorReport = ex.Message;

                return Result.Failed;
            }

            //begin transaction
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Remove Family and Associated Elements");

                    Element selElem = doc.GetElement(selItem);
                    string selParam = selElem.LookupParameter("EqConId").AsString();

                    if (selParam != null || selParam != "")
                    {
                        //filter through associated elements
                        List<Element> assocElems =
                            new FilteredElementCollector(doc)
                            .OfCategory(selectBic)
                            .WhereElementIsNotElementType()
                            .ToElements()
                            .Where(x => x.LookupParameter("EqConId").AsString() == selParam)
                            .ToList();

                        if (assocElems.Any())
                        {
                            //interact with form for user selection of what to do with associated elements
                            OneLineRemoveForm form = new OneLineRemoveForm();
                            form.ShowDialog();

                            //cancel tool if user selected Cancel button
                            if (form.DialogResult == DialogResult.Cancel)
                            {
                                errorReport = "User canceled operaton.";

                                return Result.Cancelled;
                            }

                            //check which RadialButton was selected in form
                            if (form.radBtnRemove.Checked) //delete associated elements
                            {
                                foreach (Element ae in assocElems)
                                {
                                    doc.Delete(ae.Id);
                                }
                            }
                            else //remove EqConId value
                            {
                                foreach (Element ae in assocElems)
                                {
                                    ae.LookupParameter("EqConId").Set("");
                                }
                            }
                        }
                    }

                    //remove associated Detail Items if Detail Item
                    if (selElem.Category.Name == "Detail Items")
                    {
                        BuiltInCategory tempBic = BuiltInCategory.OST_DetailComponents;

                        List<Element> detailAssocElems = 
                            new FilteredElementCollector(doc)
                            .OfCategory(tempBic)
                            .WhereElementIsNotElementType()
                            .ToElements()
                            .Where(x => x.LookupParameter("EqConId").AsString() == selParam && x.Id != selElem.Id)
                            .ToList();

                        //if list is not empty
                        if (detailAssocElems.Any())
                        {
                            //delete associated instances of Detail Items with the selected Element
                            foreach (Element detailAssocElem in detailAssocElems)
                            {
                                doc.Delete(detailAssocElem.Id);
                            }
                        }
                    }

                    //delete selected Element
                    doc.Delete(selElem.Id);

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex) //unknown error handling
                {
                    errorReport = ex.Message;
                    return Result.Failed;
                }
            }
        }
    }
}
