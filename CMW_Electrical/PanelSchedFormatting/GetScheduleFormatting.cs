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
    internal class GetScheduleFormatting
    {
        public List<Int32> GetPanelScheduleColumns(PanelScheduleType panSchedType, PanelScheduleView panSchedView, Document document)
        {
            //create initial list
            List<Int32> columns = new List<Int32>();

            if (panSchedType == PanelScheduleType.Switchboard)
            {
                columns.Add(2);
            }
            else if (panSchedType == PanelScheduleType.Branch)
            {
                string templateName = document.GetElement(panSchedView.GetTemplate()).Name;

                if (templateName.Contains("Single"))
                {
                    columns.Add(2);
                    columns.Add(11);
                }
                else
                {
                    columns.Add(2);
                    columns.Add(13);
                }
            }

            return columns;
        }

        public Int32 GetPanelScheduleRows(PanelScheduleType panSchedType, PanelScheduleView panSchedView)
        {
            //collect SectionData to extrapolate
            TableData tableData = panSchedView.GetTableData();

            TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);

            Int32 lastRowNumber = sectionData.LastRowNumber - 1;

            if (panSchedType == PanelScheduleType.Switchboard)
            {
                lastRowNumber -= 4;
            }

            return lastRowNumber;
        }
    }
}
