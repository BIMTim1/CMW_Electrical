using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMW_Electrical
{
    public class EquipmentReference
    {
        private Parameter EqName;
        private FamilySymbol EqSymbol;
        private Parameter EqVoltage;
        private bool IsConnected;

        public EquipmentReference(FamilyInstance famInst)
        {
            EqName = famInst.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME);

            EqSymbol = famInst.Symbol;

            //collect equipment voltage information
            if (EqSymbol.Name.Contains("Transformer"))
            {
                EqVoltage = EqSymbol.LookupParameter("Primary Voltage");
            }
            else
            {
                EqVoltage = EqSymbol.LookupParameter("Voltage Nominal");
            }

            string eqConId = famInst.LookupParameter("EqConId").AsString();

            if (eqConId != null || eqConId != "")
            {
                IsConnected = true;
            }
            else
            {
                IsConnected = false;
            }
        }
    }
}
