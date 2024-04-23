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
using Autodesk.Revit.DB.Structure;

namespace ScheduleLegendUpdate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PanelLegendUpdate: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            View activeView = doc.ActiveView;
            bool schedInstExists = true;
            XYZ selPoint = null;

            //cancel tool if activeView is not a SheetView
            if (activeView.ViewType != ViewType.DrawingSheet)
            {
                TaskDialog.Show("Tool canceled", "Change the Active View to a Sheet View then rerun the tool.");
                return Result.Cancelled;
            }

            //collect Generic Annotation
            List<Element> schedLegend = new FilteredElementCollector(doc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                .ToElements()
                .Where(x => x.Name == "E_GA_Schedule Legend")
                .ToList();

            //collect all PanelScheduleSheetInstances on the ActiveView
            List<PanelScheduleSheetInstance> panelSchedules = new FilteredElementCollector(doc, activeView.Id)
                .OfClass(typeof(PanelScheduleSheetInstance))
                .ToElements()
                .Cast<PanelScheduleSheetInstance>()
                .ToList();

            List<ScheduleSheetInstance> scheduleInst = new FilteredElementCollector(doc, activeView.Id)
                .OfClass(typeof(ScheduleSheetInstance))
                .ToElements()
                .Cast<ScheduleSheetInstance>()
                .Where(x => !x.IsTitleblockRevisionSchedule)
                .ToList();

            if (schedLegend == null)
            {
                try
                {
                    schedInstExists = false;
                    //prompt user to select point at which to place Generic Annotation
                    selPoint = uidoc.Selection.PickPoint("Select a Point at which to place an instance of the the E_GA_Schedule Legend.");
                }
                catch (OperationCanceledException ex)
                {
                    return Result.Cancelled;
                }
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Schedule Legend Update");

                    FamilyInstance schedLegendInst = null;

                    if (!schedInstExists)
                    {
                        //collect FamilySymbol
                        FamilySymbol famType = new FilteredElementCollector(doc)
                            .OfClass(typeof(FamilySymbol))
                            .ToElements()
                            .Cast<FamilySymbol>()
                            .Where(x => x.FamilyName != null && x.FamilyName == "E_GA_Schedule Legend")
                            .First();

                        schedLegendInst = doc.Create.NewFamilyInstance(selPoint, famType, activeView);
                    }
                    else
                    {
                        schedLegendInst = schedLegend.First() as FamilyInstance;
                    }

                    //toggle Panel Schedules Only if other schedules exist on SheetView
                    if (scheduleInst.Count() != 0)
                    {
                        ScheduleInfoToLegend(schedLegendInst, scheduleInst, activeView);
                    }

                    //define variables to collect parameters from E_GA_Schedule Legend FamilyInstance
                    int schedRows = 9;
                    List<string> schedCols = new List<string>()
                    {
                        "A", "B", "C", "D", "E"
                    };

                    List<Parameter> paramList = null;

                    for (int i = 1; i <= schedRows; i++)
                    {
                        foreach (string col in schedCols)
                        {
                            string paramName = "CELL" + i.ToString() + col;
                            paramList.Add(schedLegendInst.LookupParameter(paramName));
                        }
                    }

                    //sort PanelScheduleSheetInstances by Origin
                    List<PanelScheduleSheetInstance> sortList = panelSchedules.OrderBy(x => x.Origin[1]).ToList();

                    //Update E_GA_Schedule Legend with Panelboard Schedule Names
                    //PanelScheduleInfoToLegend(sortList, )
                    trac.Commit();
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("An error occurred", 
                        "An error has occurred that has prevented the tool from operating. Contact the BIM team for assistance.");
                    return Result.Failed;
                }
            }
        }

        public void ScheduleInfoToLegend(FamilyInstance scheduleLegendInstance, List<ScheduleSheetInstance> scheduleSheetInstances, View view)
        {
            scheduleLegendInstance.LookupParameter("Panel Schedules Only").Set(0);

            //sort schedules based on BoundingBox location
            List<ScheduleSheetInstance> sortedSchedule = scheduleSheetInstances.OrderBy(x => x.get_BoundingBox(view).Max.Y).ToList();

            List<Parameter> legendParameters = new List<Parameter>();
            foreach (Parameter param in legendParameters)
            {
                legendParameters.Add(param);
            }

            //create variables for while loop
            List<Parameter> legendParams = (from param 
                                            in legendParameters 
                                            where param.Definition != null 
                                            && param.Definition.Name.Contains("Sched") 
                                            && !param.IsReadOnly select param)
                                            .ToList();

            legendParams = (legendParams.OrderByDescending(x => x.Definition.Name)).ToList();

            int scheduleIter = 0;
            int legendIter = 0;

            while (scheduleIter < scheduleSheetInstances.Count())
            {
                ScheduleSheetInstance currentSched = scheduleSheetInstances[scheduleIter];
                string currentSchedName = currentSched.Name.ToString();
                currentSchedName = currentSchedName.Replace("E_", "");

                //if (scheduleIter == scheduleSheetInstances.Count())
                //{
                //    break;
                //}

                Parameter legendParam = legendParams[legendIter];
                legendParam.Set(currentSchedName);

                scheduleIter++;
                legendIter++;
            }
        }

        public void PanelScheduleInfoToLegend(List<PanelScheduleSheetInstance> panelScheduleList, int scheduleColumns, List<Parameter> legendParameterList)
        {

        }

        public void UpdateLegendGraphics(FamilyInstance scheduleLegendInstance, List<PanelScheduleSheetInstance> panelScheduleList)
        {

        }
    }
}
