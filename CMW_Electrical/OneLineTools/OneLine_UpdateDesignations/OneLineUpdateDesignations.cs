using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using CMW_Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneLineTools;

namespace OneLineUpdateDesignations
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class OneLineUpdateDesignations : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                //TaskDialog.Show("Parameter Does not Exist",
                //    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.");
                errorReport = "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Cancelled;
            }

            //collect Detail Items and Electrical Equipment with an DIEqConId
            List<Element> allDetailItems = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .OfClass(typeof(FamilyInstance))
                .ToElements()
                .Where(x => x.LookupParameter("EqConId").AsString() != null && x.Name.Contains("E_DI_OL_"))
                .ToList();

            List<Element> allElectricalEquip = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilyInstance))
                .ToElements()
                .Where(x => x.LookupParameter("EqConId").AsString() != null)
                .ToList();

            //cancel tool if no items in list
            if (!allDetailItems.Any())
            {
                //TaskDialog.Show("Tool Canceled", "No Electrical Equipment families or Schematic Detail Items have a EqConId value.");
                errorReport = "No Electrical Equipment families or Schematic Detail Items have a EqConId value.";
                return Result.Cancelled;
            }

            //create an instance of the WindowsForm for the user to select which update method to use
            dialogSelectUpdateMethod dialogSelectUpdateMethod = new dialogSelectUpdateMethod();
            dialogSelectUpdateMethod.ShowDialog();

            //if user canceled out of WindowsForm, close tool
            if (dialogSelectUpdateMethod.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                //TaskDialog.Show("User Canceled", "The tool was canceled.");\
                errorReport = "User canceled operation.";
                return Result.Cancelled;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    int noUpdateCount = 0;
                    int updateCount = 0;
                    string tracName;

                    if (dialogSelectUpdateMethod.rbtnUseEquipment.Checked)
                    {
                        //determine Transaction name from user selection
                        tracName = "CMWElec-Update Schematic Detail Items from Electrical Equipment families";

                        trac.Start(tracName);

                        foreach (Element equip in allElectricalEquip)
                        {
                            ElecEquipInfo equipInst = new ElecEquipInfo(equip);

                            Element detailItem = (from di
                                                  in allDetailItems
                                                  where di.LookupParameter("EqConId").AsString() == equipInst.EqConId
                                                  select di)
                                                  .First();

                            if (detailItem != null)
                            {
                                DetailItemInfo detailItemInst = new DetailItemInfo(detailItem);

                                //collect voltage information from equipment distribution system
                                ElecDistributionSystem elecDisSys = new ElecDistributionSystem(doc, equipInst.DistributionSystem);

                                //double voltCalc = 10.763910416709711538461538461538;

                                //update detailItem parameters
                                detailItemInst.Name = equipInst.Name;
                                detailItemInst.Voltage = elecDisSys.GetLineToLineVoltage;// * voltCalc;
                                detailItemInst.PhaseNum = elecDisSys.GetPhase;

                                updateCount++;
                            }
                            else
                            {
                                noUpdateCount++;
                            }
                        }

                        trac.Commit();
                        TaskDialog.Show("Components Updated", "Detail Items associated to Electrical Equipment families have been updated.");
                    }
                    else
                    {
                        tracName = "CMWElec-Update Electrical Equipment families from Schematic Detail Items";

                        trac.Start(tracName);

                        foreach (Element detItem in allDetailItems)
                        {
                            DetailItemInfo detItemInst = new DetailItemInfo(detItem);

                            Element eqElem = (from eq
                                              in allElectricalEquip
                                              where eq.LookupParameter("EqConId").AsString() == detItemInst.EqConId
                                              select eq)
                                              .First();

                            if (detItem != null)
                            {
                                ElecEquipInfo equipInst = new ElecEquipInfo(eqElem)
                                {
                                    Name = detItemInst.Name
                                };

                                equipInst.GetScheduleView?.LookupParameter("Panel Schedule Name").Set(detItemInst.Name);

                                updateCount++;
                            }
                            else
                            {
                                noUpdateCount++;
                            }
                        }

                        trac.Commit();
                        TaskDialog.Show("Equipment Updated", "Electrical Equipment families associated to Schematic Detail Items have been updated.");
                    }

                    return Result.Succeeded;
                }

                catch (Exception ex)
                {
                    //TaskDialog.Show("An error occurred", "An error occurred, contact the BIM team for assistance.");\
                    errorReport = ex.Message;
                    return Result.Failed;
                }
            }
        }
    }
}
