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

namespace ResetPanelScheduleTemplate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class PanelScheduleReset : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            //Application app = uiapp.Application;

            //collect all Branch Panel Templates
            List<Element> allBranchTemp = new FilteredElementCollector(doc).OfClass(typeof(PanelScheduleTemplate))
                .Where(x => x.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString().Contains("Branch"))
                .ToList();

            //collect all Switchboard Panel Templates
            List<Element> allSwitchTemp = new FilteredElementCollector(doc).OfClass(typeof(PanelScheduleTemplate))
                .Where(x => x.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM).AsValueString().Contains("Switchboard"))
                .ToList();

            //collect all PanelScheduleViews
            List<Element> allSchedules = new FilteredElementCollector(doc).OfClass(typeof(PanelScheduleView)).ToElements().ToList();

            //create transaction to modify active document
            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    //do thing
                    trac.Start("CMWElec-Reset all Panel Schedule Templates");

                    UpdatePanelSchedules(allSchedules, allBranchTemp, allSwitchTemp);

                    trac.Commit();

                    //create Results dialog
                    TaskDialog results = new TaskDialog("CMW-Elec - Results")
                    {
                        TitleAutoPrefix = false,
                        CommonButtons = TaskDialogCommonButtons.Ok,
                        MainInstruction = "Results:",
                        MainContent = "All Panel Schedules have been refreshed to display the latest template information."
                    };

                    results.Show();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }  
        }

        public void UpdatePanelSchedules(List<Element> panelSchedules, List<Element> branchTemps, List<Element> switchTemps)
        {
            //foreach PanelScheduleView, refresh all PanelScheduleTemplates
            foreach (PanelScheduleView sched in panelSchedules)
            {
                string schedTemp = sched.get_Parameter(BuiltInParameter.TEMPLATE_NAME).AsString();

                if (schedTemp.Contains("Branch"))
                {
                    foreach (PanelScheduleTemplate temp in branchTemps)
                    {
                        if (temp.Name == schedTemp)
                        {
                            sched.GenerateInstanceFromTemplate(temp.Id);
                        }
                    }
                }
                else if (schedTemp.Contains("Distribution") | schedTemp.Contains("MCC") | schedTemp.Contains("Switch"))
                {
                    foreach (PanelScheduleTemplate temp in switchTemps)
                    {
                        if (temp.Name == schedTemp)
                        {
                            sched.GenerateInstanceFromTemplate(temp.Id);
                        }
                    }
                }
            }
        }
    }
}
