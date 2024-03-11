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
using CMW_Electrical.OneLineTools.OneLine_Associate;

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

            Reference userSelection;

            try
            {
                //prompt user to select DetailComponent to use for association
                ISelectionFilter selFilter = new DetailItemSelectionFilter();
                //Reference userSelection = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select a Schematic Detail Item to Reference.");
                userSelection = uidoc.Selection.PickObject(ObjectType.Element, "Select a Schematic Detail Item to Reference."); //debug only
            }
            catch (OperationCanceledException ex)
            {
                //user canceled
                return Result.Cancelled;
            }

            //launch form to select Equipment Family
            OneLineAssociateForm form = new OneLineAssociateForm();
            form.ShowDialog();

            if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            DetailItemInfo detailItem = new DetailItemInfo(doc.GetElement(userSelection));

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
