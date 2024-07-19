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

    internal class PanelSchedFinalFormat: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;

            int versionNum = Int32.Parse(app.VersionNumber); //only applicable in Revit 2023 API
            View activeView = doc.ActiveView;

            //check if ActiveView is a PanelScheduleView, cancel if not
            if (activeView.ViewType != ViewType.PanelSchedule)
            {
                errorReport = "Active View must be a Panelboard Schedule. Change the Active View then rerun the tool.";
                elementSet.Insert(activeView);

                return Result.Cancelled;
            }

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("Format Panelboard Schedules");

                    //collect activeView as PanelScheduleView
                    PanelScheduleView panSched = activeView as PanelScheduleView;

                    //Get TableData to interact with specific cells of the PanelScheduleView
                    TableData tableData = panSched.GetTableData();
                    TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);

                    PanelScheduleType templateTypeName = (doc.GetElement(panSched.GetTemplate()) as PanelScheduleTemplate).GetPanelScheduleType();

                    GetScheduleFormatting schedFormat = new GetScheduleFormatting();
                    List<Int32> columns = schedFormat.GetPanelScheduleColumns(templateTypeName, panSched, doc);
                    Int32 rows = schedFormat.GetPanelScheduleRows(templateTypeName, panSched);

                    for (Int32 rowNum = 2; rowNum <= rows; rowNum++)
                    {
                        foreach (Int32 colNum in columns)
                        {
                            ElectricalSystem cct = panSched.GetCircuitByCell(rowNum, colNum);

                            if (cct != null)
                            {
                                //check if slot is a Spare and has a modified name
                                bool isSpare = panSched.IsSpare(rowNum, colNum);
                                string cctName = cct.LookupParameter("Load Name").AsString();

                                if (isSpare && cctName == "SPARE")
                                {
                                    //delete blank Spares, these are to be added at the end of the panelboard schedule
                                    panSched.RemoveSpare(rowNum, colNum);
                                }
                                else
                                {
                                    Int32 rowIter = rowNum;

                                    while (rowIter - 1 > 1)
                                    {
                                        bool canMove = panSched.CanMoveSlotTo(rowIter, colNum, rowIter - 1, colNum);
                                        ElectricalSystem canMoveCct = panSched.GetCircuitByCell(rowIter - 1, colNum);

                                        if (rowIter - 1 == 1)
                                        {
                                            break;
                                        }

                                        if (!canMove)
                                        {
                                            break;
                                        }

                                        if (canMoveCct != null)
                                        {
                                            break;
                                        }

                                        panSched.MoveSlotTo(rowIter, colNum, rowIter - 1, colNum);
                                        rowIter -= 1;
                                    }
                                }
                            }
                        }
                    }

                    for (Int32 rowNum = 2; rowNum <= rows; rowNum++)
                    {
                        foreach (Int32 colNum in columns)
                        {
                            AddSpares(panSched, rowNum, colNum, sectionData);

                            if (versionNum > 2022)
                            {
                                UnlockSlot(panSched, rowNum, colNum);
                            }
                        }
                    }

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

        public bool AddSpares(PanelScheduleView panSchedView, Int32 rowNumber, Int32 columnNumber, TableSectionData sectData)
        {
            ElectricalSystem circuit = panSchedView.GetCircuitByCell(rowNumber, columnNumber);

            if (circuit == null)
            {
                panSchedView.AddSpare(rowNumber, columnNumber);

                //collect cell data to update HorizontalAlignmentStyle
                TableCellStyle cellStyle = sectData.GetTableCellStyle(rowNumber, columnNumber);

                //create new HorizontalAlignmentStyle object
                HorizontalAlignmentStyle horizAlignment = new HorizontalAlignmentStyle();
                horizAlignment = HorizontalAlignmentStyle.Right;

                //update the collected TableCellStyle with the newly created HorizontalAlignemntStyle
                cellStyle.FontHorizontalAlignment = horizAlignment;

                //apply updated TableCellStyle to active cell
                sectData.SetCellStyle(rowNumber, columnNumber, cellStyle);
            }

            return true;
        }

        public bool UnlockSlot(PanelScheduleView panSchedView, Int32 rowNumber, Int32 columnNumber)
        {
            panSchedView.SetLockSlot(rowNumber, columnNumber, false);

            return true;
        }
    }
}
