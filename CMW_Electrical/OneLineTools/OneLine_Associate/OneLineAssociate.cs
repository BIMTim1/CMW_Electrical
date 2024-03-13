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

            //collect all Electrical Equipment families in document
            List<Element> allElecEquip = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilyInstance))
                .ToElements()
                .Where(x=>x.LookupParameter("EqConId").AsString() == null)
                .ToList();

            //collect names in array to add to selection form
            List<string> eqInfo = new List<string>();

            foreach (FamilyInstance eq in allElecEquip)
            {
                string input = eq.LookupParameter("Panel Name").AsString() 
                    + ", " 
                    + eq.Symbol.LookupParameter("Family Name").AsString() 
                    + ", " 
                    + eq.Symbol.LookupParameter("Type Name").AsString();

                eqInfo.Add(input);
            }

            Reference userSelection;

            try
            {
                //prompt user to select DetailComponent to use for association
                ISelectionFilter selFilter = new DetailItemSelectionFilter();
                userSelection = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select a Schematic Detail Item to Reference.");
                //userSelection = uidoc.Selection.PickObject(ObjectType.Element, "Select a Schematic Detail Item to Reference."); //debug only
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                //user canceled
                return Result.Cancelled;
            }

            //launch form to select Equipment Family
            OneLineAssociateForm form = new OneLineAssociateForm(eqInfo);
            form.ShowDialog();

            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            //collect Electrical Equipment selected by user
            ElecEquipInfo elecEquip = 
                new ElecEquipInfo(
                    doc, 
                    (from eq 
                     in allElecEquip 
                     where form.cBoxEquipSelection.SelectedItem.ToString().Contains(eq.LookupParameter("Panel Name").AsString()) 
                     select eq)
                     .First());

            DetailItemInfo detailItem = new DetailItemInfo(doc.GetElement(userSelection));

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Associate Electrical Equipment and Schematic Component");

                    OLEqConIdUpdateClass updateEqConId = new OLEqConIdUpdateClass();
                    updateEqConId.OneLineEqConIdValueUpdate(elecEquip, detailItem, doc);

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
