using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Electrical;

namespace PanelSchedFormatting
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    internal class SpareAndSpaceAlign : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            //collect all panelboard schedules in project
            List<PanelScheduleView> allPanelSchedules = 
                new FilteredElementCollector(doc)
                .OfClass(typeof(PanelScheduleView))
                .ToElements()
                .Cast<PanelScheduleView>()
                .ToList();

            if (!allPanelSchedules.Any())
            {
                TaskDialog.Show("No Panelboard Schedules", "There are no Panelboard schedules created in this project. The tool will now cancel.");

                return Result.Cancelled;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Spare and Space Alignment");

                    foreach (PanelScheduleView panSchedView in allPanelSchedules)
                    {
                        //collect PanelScheduelView Body TableSectionData
                        TableData tableData = panSchedView.GetTableData();
                        TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);

                        //collect PanelScheduleTemplate to determine rows and columns to itereate through
                        PanelScheduleType panSchedType = (doc.GetElement(panSchedView.GetTemplate()) as PanelScheduleTemplate).GetPanelScheduleType();
                        
                        //collect column and row information 
                        GetScheduleFormatting schedFormat = new GetScheduleFormatting();
                        List<Int32> columns = schedFormat.GetPanelScheduleColumns(panSchedType, panSchedView, doc);
                        Int32 rows = schedFormat.GetPanelScheduleRows(panSchedType, panSchedView);

                        //iterate through columns and rows of PanelScheduleView
                        for (Int32 rowNum = 2; rowNum <= rows; rowNum++)
                        {
                            foreach (Int32 colNum in columns)
                            {
                                //check if Slot is Spare or Space
                                bool isSpare = panSchedView.IsSpare(rowNum, colNum);
                                bool isSpace = panSchedView.IsSpace(rowNum, colNum);

                                //collect TableCellStyle information to update with FontHorizontalAlignment determined by Spare or Space
                                TableCellStyle cellStyle = sectionData.GetTableCellStyle(rowNum, colNum);

                                string loadName = panSchedView.GetCircuitByCell(rowNum, colNum).LookupParameter("Load Name").AsString();

                                HorizontalAlignmentStyle horizAlignment = new HorizontalAlignmentStyle();

                                if (isSpare && loadName == "SPARE")
                                {
                                    horizAlignment = HorizontalAlignmentStyle.Right;
                                }
                                else if (isSpace && loadName == "SPACE")
                                {
                                    horizAlignment = HorizontalAlignmentStyle.Center;
                                }
                                else
                                {
                                    horizAlignment = HorizontalAlignmentStyle.Left;
                                }

                                cellStyle.FontHorizontalAlignment = horizAlignment;

                                sectionData.SetCellStyle(rowNum, colNum, cellStyle);

                            }
                        }
                    }

                    trac.Commit();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("An error occurred", "An error occurred that has stopped the tool. Contact the BIM team for assistance.");

                    return Result.Failed;
                }
            }
        }
    }
}
