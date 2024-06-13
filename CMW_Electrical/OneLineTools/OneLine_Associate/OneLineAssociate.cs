using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using System.Windows.Input;
using Autodesk.Revit.UI.Selection;
using CMW_Electrical;
using OneLineTools;

namespace OneLine_Associate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineAssociate: IExternalCommand
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
                TaskDialog.Show("Parameter Does not Exist",
                    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.");
                return Result.Cancelled;
            }

            //collect names in array to add to selection form
            List<string> formInfo = new List<string>();
            List<Element> allRefElements = null;

            if (activeView.ViewType != ViewType.FloorPlan || activeView.ViewType != ViewType.DraftingView) //cancel tool if activeView = incorrect ViewType
            {
                TaskDialog.Show("Incorrect view type", "Change the active view to a Floor Plan or Drafting View then rerun the tool.");
                return Result.Cancelled;
            }
            else if (activeView.ViewType == ViewType.FloorPlan)
            {
                //collect all Electrical Equipment families in document
                allRefElements = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .OfClass(typeof(FamilyInstance))
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId").AsString() == null)
                    .ToList();

                foreach (FamilyInstance eq in allRefElements)
                {
                    string input = eq.LookupParameter("Panel Name").AsString()
                        + ", "
                        + eq.Symbol.LookupParameter("Family Name").AsString()
                        + ": "
                        + eq.Symbol.LookupParameter("Type Name").AsString();

                    formInfo.Add(input);
                }
            }
            else if(activeView.ViewType == ViewType.DraftingView)
            {
                //collect all Detail Item families in document
                allRefElements = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_DetailComponents)
                    .OfClass(typeof(FamilyInstance))
                    .ToElements()
                    .Where(x => x.LookupParameter("EqConId") == null && x.LookupParameter("Panel Name - Detail") != null)
                    .ToList();

                foreach (FamilyInstance di in allRefElements)
                {
                    string input = di.LookupParameter("Panel Name - Detail").AsString() 
                        + ", " 
                        + di.LookupParameter("Family Name").AsString() 
                        + ": " 
                        + di.LookupParameter("Type Name").AsString();

                    formInfo.Add(input);
                }
            }

            //cancel tool if not items can be found
            if (allRefElements == null || !allRefElements.Any())
            {
                TaskDialog.Show("No selectable elements", 
                    "There are no elements in the model that can be referenced to. The tool will now cancel.");

                return Result.Cancelled;
            }

            Reference userSelection;

            try
            {
                //prompt user to select DetailComponent to use for association
                //ISelectionFilter selFilter = new DetailItemSelectionFilter();
                //userSelection = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select a Schematic Detail Item to Reference.");
                userSelection = uidoc.Selection.PickObject(ObjectType.Element, "Select a Schematic Detail Item to Reference."); //debug only
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                //user canceled
                return Result.Cancelled;
            }

            //launch form to select Equipment Family
            OneLineAssociateForm form = new OneLineAssociateForm(formInfo);
            form.ShowDialog();

            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            //collect Electrical Equipment selected by user
            ElecEquipInfo elecEquipInfo = 
                new ElecEquipInfo( 
                    (from eq 
                     in allElecEquip 
                     where form.cBoxEquipSelection.SelectedItem.ToString().Contains(eq.LookupParameter("Panel Name").AsString()) 
                     select eq)
                     .First());

            DetailItemInfo detItemInfo = new DetailItemInfo(doc.GetElement(userSelection));

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Associate Electrical Equipment and Schematic Component");

                    OLEqConIdUpdateClass updateEqConId = new OLEqConIdUpdateClass();
                    updateEqConId.OneLineEqConIdValueUpdate(elecEquipInfo, detItemInfo, doc);

                    detItemInfo.Name = elecEquipInfo.Name;

                    trac.Commit();

                    //TaskDialog.Show("Associate Complete", "Electrical Equipment and Schematic Component have been had their EqConId values updated.");
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("An error occurred", "An error occurred. Contact the BIM team for assistance.");
                    return Result.Failed;
                }
            }
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
