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
//using ComponentManager = Autodesk.Windows.ComponentManager;
//using IWin32Window = System.Windows.Forms.IWin32Window;
//using Keys = System.Windows.Forms.Keys;
//using System.Runtime.InteropServices;
//using KeyPress;
//using DirectObjLoader;

namespace OneLineConnectAndPlace
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLineConnectAndPlace : IExternalCommand
    {
        //IWin32Window _revit_window;

        //static bool _place_one_single_instance_then_abort = true;

        //List<ElementId> _added_element_ids = new List<ElementId>();

        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //from Jermey Tammik
            //_revit_window = new JtWindowHandler(ComponentManager.ApplicationWindow);

            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;

            //stop tool if activeView is not a Drafting View
            if (activeView.ViewType != ViewType.DraftingView)
            {
                TaskDialog.Show("Incorrect View Type", "Open a Drafting View that contains your One-Line Diagram and rerun the tool.");
                return Result.Failed;
            }

            //collect equipment that has NOT been circuited
            List<Element> uncircuitedEquip = 
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(x=>x.LookupParameter("Supply From").AsString() == "")
                .ToList();

            List<string> equipNames = (from pnl in uncircuitedEquip select pnl.LookupParameter("Panel Name").AsString()).ToList();

            //start SelectEquipmentToReference Windows Form
            SelectEquipmentToReferenceForm equipSelectForm = new SelectEquipmentToReferenceForm(equipNames);
            equipSelectForm.ShowDialog();

            if (equipSelectForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Failed;
            }

            //collect Detail Item One-Line families
            List<FamilySymbol> detailItemTypes = 
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .OfClass(typeof(FamilySymbol))
                .Where(x=>x.Name.Contains("E_DI_OL_"))
                .Cast<FamilySymbol>()
                .ToList();

            Element selectedEquip = null;
            string detailItemRef = null;

            //select the Electrical Equipment FamilyInstance if selected by user
            if (equipSelectForm.rbtnUseEquipment.Checked)
            {
                selectedEquip = 
                    new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.LookupParameter("Panel Name").AsString() == equipSelectForm.cboxEquipNameSelect.SelectedItem.ToString())
                    .First();

                string famType = selectedEquip.LookupParameter("Family").AsValueString();

                if (famType.Contains("Branch") || famType.Contains("Load Center"))
                {
                    detailItemRef = "Panelboard";
                }
                else if (famType.Contains("Distribution") || famType.Contains("Switchboard"))
                {
                    detailItemRef = "Bus";
                }
                else if (famType.Contains("Transformer-Dry Type"))
                {
                    detailItemRef = "XFMR";
                }
                else if (famType.Contains("Utility"))
                {
                    detailItemRef = "Utility";
                }
            }
            else if (equipSelectForm.rbtnDontUseEquipment.Checked)
            {
                detailItemRef = equipSelectForm.cboxDetailItemType.SelectedItem.ToString();
            }

            //select Detail Item FamilySymbol from list using detailItemRef
            FamilySymbol selectedDetailItem = (from famSym in detailItemTypes where famSym.Name.Contains(detailItemRef) select famSym).First();

            //create Transaction elements
            TransactionGroup tracGroup = new TransactionGroup(doc);
            Transaction trac = new Transaction(doc);

            ISelectionFilter selFilter = new DetailItemSelectionFilter();

            FamilyInstance connectEquip;
            XYZ pickedPoint;

            try
            {
                //prompt user to select source equipment Detail Item
                Reference connectEquipRef = uidoc.Selection.PickObject(
                    Autodesk.Revit.UI.Selection.ObjectType.Element, 
                    selFilter, 
                    "Select a Detail Item to be the Source Equipment.");

                connectEquip = doc.GetElement(connectEquipRef) as FamilyInstance;

                //Prompt user to select point at which to place fed equipment Detail Item
                pickedPoint = uidoc.Selection.PickPoint("Select a Point at which to Place Detail Item.");
            }

            catch (OperationCanceledException e)
            {
                TaskDialog.Show("User canceled operation", "The tool was canceled.");
                return Result.Failed;
            }

            tracGroup.Start("Select Source One-Line Equipment and Place Fed From Instance.");
            trac.Start("Place Fed From Instance.");

            FamilyInstance newFamInstance = doc.Create.NewFamilyInstance(pickedPoint, selectedDetailItem, activeView);

            trac.Commit();

            tracGroup.Assimilate();

            //collect information from newFamInstance

            //_added_element_ids.Clear();

            //app.DocumentChanged += new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);

            //uidoc.PromptForFamilyInstancePlacement(selectedDetailItem);

            //app.DocumentChanged -= new EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(OnDocumentChanged);


            //catch (Exception e)
            //{
            //    TaskDialog.Show("Error Occurred", "An error occurred. Contact the BIM Team for assistance.");

            //    return Result.Failed;
            //}

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

        //void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        //{
        //    ICollection<ElementId> idsAdded = e.GetAddedElementIds();

        //    int n = idsAdded.Count;

        //    if (_place_one_single_instance_then_abort && 0 < n)
        //    {
        //        //mimic pressing the Escape key twice to close out of PromptForFamilyInstancePlacement
        //        Press.PostMesage(_revit_window.Handle, (uint)Press.KEYBOARD_MSG.WM_KEYDOWN, (uint)Keys.Escape, 0);

        //        Press.PostMesage(_revit_window.Handle, (uint)Press.KEYBOARD_MSG.WM_KEYDOWN, (uint)Keys.Escape, 0);
        //    }
        //}
    }
}
