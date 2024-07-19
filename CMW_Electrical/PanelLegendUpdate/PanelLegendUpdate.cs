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
                errorReport = "Change the Active View to a Sheet View then rerun the tool.";
                elementSet.Insert(activeView);

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

            if (schedLegend.Count() == 0)
            {
                try
                {
                    schedInstExists = false;
                    //prompt user to select point at which to place Generic Annotation
                    selPoint = uidoc.Selection.PickPoint("Select a Point at which to place an instance of the the E_GA_Schedule Legend.");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                    errorReport = "User canceled operation.";
                    elementSet.Insert(activeView);

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
                    int schedRows = 8;
                    List<string> schedCols = new List<string>()
                    {
                        "A", "B", "C", "D", "E"
                    };

                    List<Parameter> paramList = new List<Parameter>();

                    for (int i = 1; i <= schedRows; i++)
                    {
                        foreach (string col in schedCols)
                        {
                            string paramName = "CELL" + i.ToString() + col;
                            Parameter param = schedLegendInst.LookupParameter(paramName);
                            paramList.Add(param);

                            //reset values of legend in case user is updating schedule
                            param.Set("");
                        }
                    }

                    doc.Regenerate();

                    //sort PanelScheduleSheetInstances by Origin
                    List<PanelScheduleSheetInstance> sortList = panelSchedules.OrderBy(x => x.Origin[1]).ToList();

                    //Update E_GA_Schedule Legend with Panelboard Schedule Names
                    List<int> scheduleInfo = PanelScheduleInfoToLegend(sortList, schedCols, paramList);

                    //update the E_GA_Schedule Legend FamilyInstance column values
                    int maxCol = scheduleInfo.Max();
                    UpdateLegendGraphics(schedLegendInst, sortList, maxCol);

                    //UpdateLegendGraphics(schedLegendInst, sortList);

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

        public void ScheduleInfoToLegend(FamilyInstance scheduleLegendInstance, List<ScheduleSheetInstance> scheduleSheetInstances, View view)
        {
            scheduleLegendInstance.LookupParameter("Panel Schedules Only").Set(0);

            //sort schedules based on BoundingBox location
            List<ScheduleSheetInstance> sortedSchedule = scheduleSheetInstances.OrderBy(x => x.get_BoundingBox(view).Max.Y).ToList();

            List<Parameter> legendParameters = new List<Parameter>();
            foreach (Parameter param in scheduleLegendInstance.Parameters)
            {
                if (param.Definition != null && param.Definition.Name.Contains("Sched") && !param.IsReadOnly)
                {
                    legendParameters.Add(param);
                }
            }

            legendParameters = (legendParameters.OrderByDescending(x => x.Definition.Name)).ToList();

            int scheduleIter = 0;
            int legendIter = 0;

            while (scheduleIter < scheduleSheetInstances.Count())
            {
                ScheduleSheetInstance currentSched = scheduleSheetInstances[scheduleIter];
                string currentSchedName = currentSched.Name.ToString();
                currentSchedName = currentSchedName.Replace("E_", "");

                Parameter legendParam = legendParameters[legendIter];
                legendParam.Set(currentSchedName);

                scheduleIter++;
                legendIter++;
            }
        }

        public List<int> PanelScheduleInfoToLegend(List<PanelScheduleSheetInstance> panelScheduleList, List<string> scheduleColumns, List<Parameter> legendParameterList)
        {
            XYZ previousSchedOrigin = new XYZ(0, 0, 0);
            XYZ currentSchedOrigin = new XYZ(0, 0, 0);
            int schedVal = 0;
            int cellRowVal = 8;
            int cellColVal = 4;

            int colCount = 1;
            List<int> columnCounts = new List<int>();

            while (schedVal < panelScheduleList.Count())
            {
                //collect current schedule information
                PanelScheduleSheetInstance currentSched = panelScheduleList[schedVal];
                currentSchedOrigin = currentSched.Origin;
                string currentSchedName = currentSched.Name.ToString();

                //determine if the current schedule has a different Y coordinate from previous schedule, reset to E_GA_Schedule Legend right side
                if (previousSchedOrigin.Y != 0)
                {
                    if (Math.Round(currentSchedOrigin.Y, 3) != Math.Round(previousSchedOrigin.Y, 3))
                    {
                        cellRowVal -= 1;
                        cellColVal = 4;

                        columnCounts.Add(colCount);
                        colCount = 1;

                    }
                }

                string cellParamName = "CELL" + cellRowVal.ToString() + scheduleColumns[cellColVal];
                Parameter selParam = (from param in legendParameterList where param.Definition.Name.ToString() == cellParamName select param).First();

                //set parameter value
                selParam.Set(currentSchedName);

                //reset parameters for next iteration
                previousSchedOrigin = currentSchedOrigin;
                cellColVal -= 1;
                schedVal++;

                colCount++;
            }

            return columnCounts;
        }

        public void UpdateLegendGraphics(FamilyInstance scheduleLegendInstance, List<PanelScheduleSheetInstance> panelScheduleList, int colNumber)
        {
            List<double> numRows = new List<double>();
            //List<double> numCols = new List<double>();

            foreach (PanelScheduleSheetInstance pnlSched in panelScheduleList)
            {
                //double roundedX = Math.Round(pnlSched.Origin.X, 3);
                double roundedY = Math.Round(pnlSched.Origin.Y, 3);

                //if (!numCols.Contains(roundedX))
                //{
                //    numCols.Add(roundedX);
                //}

                if (!numRows.Contains(roundedY))
                {
                    numRows.Add(roundedY);
                }
            }

            scheduleLegendInstance.LookupParameter("Columns").Set(colNumber);
            scheduleLegendInstance.LookupParameter("Rows").Set(numRows.Count());
        }
    }
}
