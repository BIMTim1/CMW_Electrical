using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMW_Electrical
{
    internal class DetailItemInfo
    {
        private readonly FamilyInstance DIFamInst;
        private readonly Parameter DIName;
        private readonly Parameter DIEqConId;
        private readonly Parameter DIVoltage;
        private readonly Parameter DINumberOfPoles;
        private readonly Parameter DIPhasing;

        public DetailItemInfo(Element detailItem)
        {
            DIFamInst = detailItem as FamilyInstance;
            DIName = DIFamInst.LookupParameter("Panel Name - Detail");
            DIEqConId = DIFamInst.LookupParameter("EqConId");
            DIVoltage = DIFamInst.LookupParameter("E_Voltage");
            DINumberOfPoles = DIFamInst.LookupParameter("E_Number of Poles");
            DIPhasing = DIFamInst.LookupParameter("New, Existing, Demo (1,2,3)");
        }

        /// <summary>
        /// Get the FamilyInstance of the DetailItem Element.
        /// </summary>
        /// <value>Get the FamilyInstance of the DetailItem Element.</value>
        public FamilyInstance GetFamilyInstance
        {
            get { return DIFamInst; }
        }

        /// <summary>
        /// Get or set the Panel Name - Detail parameter of this DetailItem.
        /// </summary>
        /// <value>Return the string value of the Panel Name - Detail parameter of this DetailItem.</value>
        public string Name
        {
            get { return DIName.AsString(); }
            set { DIName.Set(value); }
        }

        /// <summary>
        /// Get or set the EqConId parameter of this DetailItem.
        /// </summary>
        /// <value>Return the EqConId value of this DetailItem.</value>
        public string EqConId
        {
            get { return DIEqConId.AsString(); }
            set { DIEqConId.Set(value); }
        }

        /// <summary>
        /// Get or set the E_Voltage parameter of this DetailItem.
        /// </summary>
        /// <value>Return the double value of the E_Voltage parameter of this DetailItem.</value>
        public double Voltage
        {
            //is a voltage conversion necessary?
            //double voltCalc = 10.763910416709711538461538461538
            get { return DIVoltage.AsDouble(); }
            set { DIVoltage.Set(value); }
        }

        /// <summary>
        /// Get or set the E_Number of Poles parameter of this DetailItem.
        /// </summary>
        /// <value>Return the integer value of the E_Number of Poles parameter of this DetailItem.</value>
        public int Poles
        {
            get { return DINumberOfPoles.AsInteger(); }
            set { DINumberOfPoles.Set(value); }
        }

        /// <summary>
        /// Get or set the New, Existing, Demo (1,2,3) parameter of this DetailItem.
        /// </summary>
        /// <value>Return the integer assigned to the New, Existing, Demo (1,2,3) parameter.</value>
        public int Phase
        {
            //determine how to update from Phase of Equipment
            get { return DIPhasing.AsInteger(); }
            set { DIPhasing.Set(value); }
        }
    }
}
