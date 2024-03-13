using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLUpdateInfo
{
    public class OLUpdateDetailItemInfo
    {
        public void OneLineUpdateParameters(FamilyInstance detailItem, FamilyInstance panel, Document document)
        {
            //update One-Line Detail Item FamilyInstance from Electrical Equipment family
            //List<string> diParamStrings = new List<string>()
            //{
            //    "Panel DIName - Detail",
            //    "MLO",
            //    "E_Voltage",
            //    "E_Number of Poles",
            //    "DIEqConId"
            //};
            string panelName = panel.LookupParameter("Panel Name").AsString();
            detailItem.LookupParameter("Panel Name - Detail").Set(panelName);

            //update MLO parameter based on panel information
            string panelMainsType = panel.Symbol.LookupParameter("Mains Type").AsString();
            if (panelMainsType.Contains("MLO"))
            {
                detailItem.LookupParameter("MLO").Set(1);
            }
            else
            {
                detailItem.LookupParameter("MLO").Set(0);
            }

            //update E_Voltage parameter
            string voltString = (document.GetElement(panel.LookupParameter("Distribution System").AsElementId())).LookupParameter("Line to Line Voltage").AsValueString();
            //int voltage = int.Parse((
            //    document.GetElement(
            //        panel.LookupParameter("Distribution System")
            //        .AsElementId()))
            //        .LookupParameter("Line to Line Voltage")
            //        .AsValueString());

            string volt = voltString.Substring(0, 3);
            int voltage = int.Parse(volt);
            double voltMultiplier = 10.763910416709711538461538461538;

            //int voltage = 208;
            double inputVoltage = voltage * voltMultiplier;

            detailItem.LookupParameter("E_Voltage").Set(inputVoltage);

            //adjust E_Number of Poles
            int panelPhase = panel.LookupParameter("Number of Phases").AsInteger();

            int inputPoles;

            if (panelPhase == 1)
            {
                if (voltage == 120 || voltage == 277)
                {
                    inputPoles = 1;
                }
                else
                {
                    inputPoles = 2;
                }
            }
            else
            {
                inputPoles = 3;
            }

            detailItem.LookupParameter("E_Number of Poles").Set(inputPoles);
        }
    }
}

