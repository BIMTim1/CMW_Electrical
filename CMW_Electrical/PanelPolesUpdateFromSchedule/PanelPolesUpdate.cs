using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq.Expressions;
using CMW_Electrical;

namespace ManagePanelSchedules
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PanelPolesUpdate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            #region Autodesk Info
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            #endregion //Autodesk Info

            View activeView = doc.ActiveView;

            //variables to manage / update
            PanelScheduleView panelSchedule;
            FamilyInstance equipment;

            List<ViewType> approvedViewTypes = new List<ViewType>()
            {
                ViewType.PanelSchedule,
                ViewType.FloorPlan,
                ViewType.ThreeD
            };

            #region User Selection / ActiveView selection
            if (!approvedViewTypes.Contains(activeView.ViewType))
            {
                elementSet.Insert(activeView);

                errorReport = "Incorrect View Type. Change the active view to a Floor Plan, 3D View, or Panelboard Schedule view and rerun the tool.";

                return Result.Cancelled;
            }
            else
            {
                if (activeView.ViewType == ViewType.PanelSchedule)
                {
                    //collect active view as PanelScheduleView
                    panelSchedule = activeView as PanelScheduleView;

                    //get Electrical Equipment from PanelScheduleView
                    equipment = doc.GetElement(panelSchedule.GetPanel()) as FamilyInstance;
                }
                else
                {
                    string userMessage = "Select an Electrical Equipment instance.";
                    Reference selObject;

                    try
                    {
                        ISelectionFilter selFilter = new CMWElecSelectionFilter.EquipmentSelectionFilter();

                        selObject = uidoc.Selection.PickObject(ObjectType.Element, selFilter, userMessage);
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                    {
                        errorReport = "User canceled operation. No changes made.";

                        return Result.Cancelled;
                    }
                    catch (Exception ex)
                    {
                        errorReport = ex.Message;

                        return Result.Failed;
                    }

                    //actions if selection doesn't fail
                    equipment = doc.GetElement(selObject) as FamilyInstance;

                    //check if selected Electrical Equipment instance has an associated PanelScheduleView
                    ElementClassFilter elemFilter = new ElementClassFilter(typeof(PanelScheduleView));

                    IList<ElementId> colList = equipment.GetDependentElements(elemFilter);

                    //cancel if no panel schedule found
                    if (!colList.Any())
                    {
                        elementSet.Insert(equipment);

                        errorReport = "The selected Electrical Equipment instance does not have an associated Panel Schedule View. No changes were made.";

                        return Result.Cancelled;
                    }

                    panelSchedule = doc.GetElement(colList.First()) as PanelScheduleView;
                }
                #endregion //User Selection / ActiveView selection

                using (Transaction trac = new Transaction(doc))
                {
                    try
                    {
                        trac.Start("CMWElec-Update Panel Poles");

                        //collect panel schedule information
                        PanelScheduleData tableData = panelSchedule.GetTableData();
                        int numPoles = tableData.NumberOfSlots;

                        Parameter equipPoles = GetCircuitsParameter(equipment);
                        equipPoles.Set(numPoles);

                        trac.Commit();

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

        public Parameter GetCircuitsParameter(FamilyInstance familyInstance)
        {
            Parameter equipmentPoles = null;

            BuiltInParameter bipPanel = BuiltInParameter.RBS_ELEC_MAX_POLE_BREAKERS;
            BuiltInParameter bipSwitchboard = BuiltInParameter.RBS_ELEC_NUMBER_OF_CIRCUITS;

            if (familyInstance.get_Parameter(bipPanel) != null)
            {
                equipmentPoles = familyInstance.get_Parameter(bipPanel);
            }
            else if (familyInstance.get_Parameter(bipSwitchboard) != null)
            {
                equipmentPoles = familyInstance.get_Parameter(bipSwitchboard);
            }
            else
            {
                throw new Exception("Selected Equipment cannot be modified. The tool will now cancel.");
            }

            return equipmentPoles;
        }
    }
}
