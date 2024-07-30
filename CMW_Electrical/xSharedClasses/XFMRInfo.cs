using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMW_Electrical
{
    internal class XFMRInfo
    {
        private readonly FamilyInstance XFamInst;
        private readonly Parameter XName;
        private readonly Parameter XPrimaryVoltage;
        private readonly Parameter XSecondaryVoltage;
        private FamilySymbol XFamilyType;
        private readonly Parameter XkVARating;

        public XFMRInfo(Element elecEquip)
        {
            XFamInst = elecEquip as FamilyInstance;
            XName = XFamInst.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME);
            XPrimaryVoltage = XFamInst.LookupParameter("E_Primary Voltage");
            XSecondaryVoltage = XFamInst.LookupParameter("E_Secondary Voltage");

            //FamilyInstance type information
            XFamilyType = XFamInst.Symbol;
            XkVARating = XFamilyType.LookupParameter("XFMR_kVA");
        }

        /// <summary>
        /// Get the FamilyInstance of the Transformer element
        /// </summary>
        public FamilyInstance GetFamilyInstance
        {
            get { return XFamInst; }
        }

        /// <summary>
        /// Get or set the Panel Name parameter of the associated Transformer family.
        /// </summary>
        /// <value>String value to set the Panel Name parameter.</value>
        public string Name
        {
            get { return XName.AsString(); }
            set { XName.Set(value); }
        }

        /// <summary>
        /// Get or set the E_Primary Voltage parameter of the associated Transformer family.
        /// </summary>
        /// <value>Double value of the desired Primary Voltage. Value will need to be adjusted for Revit internal calculation.</value>
        public double PrimaryVoltage
        {
            get { return XPrimaryVoltage.AsDouble(); }
            set { XPrimaryVoltage.Set(value); }
        }

        /// <summary>
        /// Get or set the E_Secondary Voltage parameter of the associated Transformer family.
        /// </summary>
        /// <value>Double value of the desired Secondary Voltage. Value will need to be adjusted for Revit internal calculation.</value>
        public double SecondaryVoltage
        {
            get { return XSecondaryVoltage.AsDouble(); }
            set { XSecondaryVoltage.Set(value); }
        }

        /// <summary>
        /// Get or set the FamilySymbol of the Transformer family
        /// </summary>
        /// <value>FamilySymbol of the ActiveDocument</value>
        public FamilySymbol XFMRFamilySymbol
        {
            get { return XFamilyType; }
            set { XFamilyType = value; }
        }

        /// <summary>
        /// Get the XkVARating value of the associated FamilySymbol
        /// </summary>
        public double GetKVA
        {
            get { return XkVARating.AsDouble(); }
        }
    }
}
