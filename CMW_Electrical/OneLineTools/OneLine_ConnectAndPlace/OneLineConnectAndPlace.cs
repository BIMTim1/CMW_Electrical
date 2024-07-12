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
using System.Xml;
using OLUpdateInfo;
using Autodesk.Revit.DB.Electrical;
using OneLineTools;
using CMW_Electrical;
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
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";
                elementSet.Insert(activeView);

                return Result.Failed;
            }

            //stop tool if activeView is not a Drafting View
            if (activeView.ViewType != ViewType.DraftingView)
            {
                errorReport = "Open a Drafting View that contains your One-Line Diagram and rerun the tool.";
                elementSet.Insert(activeView);

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
                return Result.Cancelled;
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

                //Reference connectEquipRef = uidoc.Selection.PickObject(ObjectType.Element, "Select a Detail Item to be the Source Equipment"); //debug only

                connectEquip = doc.GetElement(connectEquipRef) as FamilyInstance;

                //Prompt user to select point at which to place fed equipment Detail Item
                pickedPoint = uidoc.Selection.PickPoint("Select a Point at which to Place Detail Item.");
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                errorReport = "User canceled operation.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            using (TransactionGroup tracGroup = new TransactionGroup(doc))
            {
                tracGroup.Start("Connect and Place Component");

                using (Transaction trac = new Transaction(doc))
                {
                    try
                    {
                        trac.Start("Place Fed From Instance.");

                        FamilyInstance newFamInstance = doc.Create.NewFamilyInstance(pickedPoint, selectedDetailItem, activeView);

                        trac.Commit();

                        trac.Start("Create Feeder Lines");

                        //collect E_DI_Feeder-Line Based Detail Item
                        FamilySymbol feederLine = (new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_DetailComponents)
                            .WhereElementIsElementType()
                            .ToElements()
                            .Where(x => x.LookupParameter("Family Name").AsString() == "E_DI_Feeder-Line Based")
                            .ToList())
                            .First()
                            as FamilySymbol;

                        //Create FeederLines and circuit breaker if applicable
                        OLCreateFeeder createFeeder = new OLCreateFeeder();
                        List<FamilyInstance> feederLines = createFeeder.CreateFeeder(connectEquip, newFamInstance, pickedPoint, activeView, doc, feederLine);

                        trac.Commit();

                        if (equipSelectForm.rbtnUseEquipment.Checked)
                        {
                            //update parameter information
                            trac.Start("Update Detail Item Parameters from Electrical Equipment.");

                            //update Electrical Parameter Information
                            OLUpdateDetailItemInfo newThing = new OLUpdateDetailItemInfo();
                            newThing.OneLineUpdateParameters(newFamInstance, selectedEquip as FamilyInstance, doc);

                            //update DIEqConId of Detail Item and Electrical Equipment
                            OLEqConIdUpdateClass updateEqConId = new OLEqConIdUpdateClass();

                            ElecEquipInfo elecEquip = new ElecEquipInfo(selectedEquip);
                            DetailItemInfo detailItem = new DetailItemInfo(newFamInstance)
                            {
                                EqConIdConnectedSource = connectEquip.LookupParameter("EqConId").AsString() //set EqConId Connected Source
                            };

                            updateEqConId.OneLineEqConIdValueUpdate(elecEquip, detailItem, doc);

                            doc.Regenerate();

                            //connect fed from equipment to selected source
                            if (connectEquip.LookupParameter("EqConId").AsString() != "")
                            {
                                //create ElectricalSystem from user selected DetailItem
                                CreateEquipmentCircuit equipCircuit = new CreateEquipmentCircuit();
                                equipCircuit.CreateEquipCircuit(doc, connectEquip, selectedEquip as FamilyInstance);

                                if (equipCircuit == null)
                                {
                                    TaskDialog.Show(
                                        "Selected Detail Item has no Connected Equipment", 
                                        "Selected Detail Item does not have a referenced Electrical Equipment family to reference. No circuit will be created.");
                                }
                            }

                            //update Detail Item - Line Based feeders with DIEqConId value
                            foreach (FamilyInstance feeder in feederLines)
                            {
                                feeder.LookupParameter("EqConId").Set(newFamInstance.LookupParameter("EqConId").AsString());
                            }

                            trac.Commit();
                        }

                        else if (equipSelectForm.rbtnDontUseEquipment.Checked)
                        {
                            trac.Start("Update Detail Item Parameters from Form");

                            //detail item parameters
                            Parameter panelNameDetail = newFamInstance.LookupParameter("Panel Name - Detail");
                            Parameter voltageDetail = newFamInstance.LookupParameter("E_Voltage");

                            //update Panel DIName - Detail
                            panelNameDetail.Set(equipSelectForm.tboxNewEquipmentName.Text);

                            //update voltage
                            int voltage = int.Parse(equipSelectForm.cboxNewEquipmentVoltage.SelectedItem.ToString());
                            double voltMultiplier = 10.763910416709711538461538461538;

                            int inputVoltage = Convert.ToInt32(voltage * voltMultiplier);

                            voltageDetail.Set(inputVoltage);

                            //set DIEqConId to be modified from another tool
                            string notAssignedVal = UpdateEqConIdNotAssigned(doc);

                            newFamInstance.LookupParameter("EqConId").Set(notAssignedVal);

                            trac.Commit();
                        }

                        tracGroup.Assimilate();

                        return Result.Succeeded;
                    }
                    catch (Exception ex)
                    {
                        errorReport = ex.Message;
                        elementSet.Insert(connectEquip);

                        return Result.Failed;
                    }
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

        public string UpdateEqConIdNotAssigned(Document document)
        {
            string updateId;

            List<FamilyInstance> notAssignedDetailItems = 
                new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .OfClass(typeof(FamilyInstance))
                .ToElements()
                .Where(x => x.LookupParameter("EqConId").AsString().Contains("NotAssigned"))
                .Cast<FamilyInstance>()
                .ToList();

            string num = (notAssignedDetailItems.Count() + 1).ToString();
            updateId = "NotAssigned" + num;

            return updateId;
        }
    }
}
