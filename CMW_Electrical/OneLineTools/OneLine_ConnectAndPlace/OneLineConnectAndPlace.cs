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
using OLEqConIdUpdate;
using Autodesk.Revit.DB.Electrical;
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
                //Reference connectEquipRef = uidoc.Selection.PickObject(
                //    Autodesk.Revit.UI.Selection.ObjectType.Element, 
                //    selFilter, 
                //    "Select a Detail Item to be the Source Equipment.");

                //debug only
                Reference connectEquipRef = uidoc.Selection.PickObject(ObjectType.Element, "Select a Detail Item to be the Source Equipment");

                connectEquip = doc.GetElement(connectEquipRef) as FamilyInstance;

                //Prompt user to select point at which to place fed equipment Detail Item
                pickedPoint = uidoc.Selection.PickPoint("Select a Point at which to Place Detail Item.");
            }

            catch (OperationCanceledException e)
            {
                TaskDialog.Show("User canceled operation", "The tool was canceled.");
                return Result.Failed;
            }

            tracGroup.Start("Connect and Place Component");
            trac.Start("Place Fed From Instance.");

            FamilyInstance newFamInstance = doc.Create.NewFamilyInstance(pickedPoint, selectedDetailItem, activeView);

            trac.Commit();

            //collect E_DI_Feeder-Line Based Detail Item
            FamilySymbol feederLine = (new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsElementType()
                .ToElements()
                .Where(x => x.LookupParameter("Family Name").AsString() == "E_DI_Feeder-Line Based")
                .ToList())
                .First()
                as FamilySymbol;

            //collect location information to place Detail Lines
            XYZ startPoint = (connectEquip.Location as LocationPoint).Point;
            XYZ endPoint = pickedPoint;

            XYZ firstPoint = null;
            XYZ midTopPoint = null;
            XYZ midBottomPoint = null;
            XYZ secondPoint = null;

            double topY;
            double bottomY;

            bool polyLine = true;

            //find Y offsets of selected and placed Detail Items
            double selYOffset = connectEquip.Symbol.get_BoundingBox(activeView).Max[1];

            double placedYOffset = newFamInstance.Symbol.get_BoundingBox(activeView).Max[1];

            //determine if more than (1) line should be created
            if (startPoint.X == endPoint.X)
            {
                polyLine = false;
            }

            //determine offsets from uppermost point and bottom-most point
            if (startPoint.Y > endPoint.Y)
            {
                topY = startPoint.Y - selYOffset;
                bottomY = endPoint.Y + placedYOffset;

                firstPoint = new XYZ(startPoint.X, topY, startPoint.Z);
                secondPoint = new XYZ(endPoint.X, bottomY, startPoint.Z);
            }
            else
            {
                topY = endPoint.Y - placedYOffset;
                bottomY = startPoint.Y + selYOffset;

                firstPoint = new XYZ(endPoint.X, topY, endPoint.Z);
                secondPoint = new XYZ(startPoint.X, bottomY, endPoint.Z);
            }

            trac.Start("Create Detail Lines for Connected Equipment.");

            List<FamilyInstance> feederLines = new List<FamilyInstance>();

            //create line work based on position of selected and placd elements
            if (!polyLine)
            {
                Line curve = Line.CreateBound(firstPoint, secondPoint);
                //doc.Create.NewDetailCurve(activeView, curve);
                FamilyInstance newFeeder = doc.Create.NewFamilyInstance(curve, feederLine, activeView);

                feederLines.Add(newFeeder);
            }
            else
            {
                double midY = topY - (Line.CreateBound(firstPoint, new XYZ(firstPoint.X, bottomY, secondPoint.Z)).Length / 2);
                midTopPoint = new XYZ(firstPoint.X, midY, firstPoint.Z);
                midBottomPoint = new XYZ(secondPoint.X, midY, secondPoint.Z);

                Line curve1 = Line.CreateBound(firstPoint, midTopPoint);
                Line curve2 = Line.CreateBound(midTopPoint, midBottomPoint);
                Line curve3 = Line.CreateBound(midBottomPoint, secondPoint);
                List<Line> curveList = new List<Line>()
                {
                    curve1,
                    curve2,
                    curve3
                };

                foreach (Line line in curveList)
                {
                    FamilyInstance newFeeder = doc.Create.NewFamilyInstance(line, feederLine, activeView);

                    feederLines.Add(newFeeder);
                }

                //CurveArray curveArray = new CurveArray();

                //foreach (Line curve in curveList)
                //{
                //    curveArray.Append(curve);
                //}

                //doc.Create.NewDetailCurveArray(activeView, curveArray);
                //update CurveArray with correct LineStyle
            }

            trac.Commit();

            if (equipSelectForm.rbtnUseEquipment.Checked)
            {
                //update parameter information
                trac.Start("Update Detail Item Parameters from Electrical Equipment.");

                //update Electrical Parameter Information
                OLUpdateDetailItemInfo newThing = new OLUpdateDetailItemInfo();
                newThing.OneLineUpdateParameters(newFamInstance, selectedEquip as FamilyInstance, doc);

                //update EqConId of Detail Item and Electrical Equipment
                OLEqConIdUpdateClass updateEqConId = new OLEqConIdUpdateClass();
                updateEqConId.OneLineEqConIdValueUpdate(newFamInstance, selectedEquip as FamilyInstance, doc);

                doc.Regenerate();

                //connect fed from equipment to selected source

                //update Detail Item - Line Based feeders with EqConId value
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

                //update Panel Name - Detail
                panelNameDetail.Set(equipSelectForm.tboxNewEquipmentName.Text);

                //update voltage
                int voltage = int.Parse(equipSelectForm.cboxNewEquipmentVoltage.SelectedItem.ToString());
                double voltMultiplier = 10.763910416709711538461538461538;

                int inputVoltage = Convert.ToInt32(voltage * voltMultiplier);

                voltageDetail.Set(inputVoltage);

                //set EqConId to be modified from another tool
                string notAssignedVal = UpdateEqConIdNotAssigned(doc);

                newFamInstance.LookupParameter("EqConId").Set(notAssignedVal);

                trac.Commit();
            }

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

        public void ConnectEquipment(Document document, FamilyInstance sourceDetailItem, FamilyInstance fedToEquip)
        {
            BuiltInCategory bic = BuiltInCategory.OST_ElectricalEquipment;

            //collect Electrical Equipment FamilyInstance with selected sourceDetailItem EqConId
            FamilyInstance sourceEquip = 
                new FilteredElementCollector(document)
                .OfCategory(bic)
                .OfClass(typeof(FamilyInstance))
                .ToElements()
                .Cast<FamilyInstance>()
                .Where(x => x.LookupParameter("EqConId").AsString() == sourceDetailItem.LookupParameter("EqConId")
                .AsString())
                .First();

            if (sourceEquip != null)
            {
                //circuit fedToEquip to sourceEquip
                ConnectorSet connectorSet = fedToEquip.MEPModel.ConnectorManager.UnusedConnectors;
                ElectricalSystem fedToCircuit;
                foreach (Connector connector in connectorSet)
                {
                    ElectricalSystemType elecSysType = connector.ElectricalSystemType;
                    fedToCircuit = ElectricalSystem.Create(connector, elecSysType);
                    fedToCircuit.SelectPanel(sourceEquip);
                }
            }
            else
            {
                TaskDialog.Show(
                    "Selected Detail Item has no Connected Equipment", 
                    "Selected Detail Item does not have a referenced Electrical Equipment family to reference. No circuit will be created.");
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
