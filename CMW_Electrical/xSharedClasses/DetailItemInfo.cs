﻿using Autodesk.Revit.Attributes;
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
    public class DetailItemInfo
    {
        private readonly FamilyInstance DIFamInst;
        private readonly Parameter DIName;
        private readonly Parameter DIEqConId;
        private readonly Parameter DIVoltage;
        private readonly double DIActualVoltage;
        private readonly Parameter DINumberOfPhases;
        private readonly Parameter DICurrent;
        private readonly Parameter DIPhasing;
        private readonly Parameter DIEqConIdConnSource;
        //private readonly Parameter DISCCR;

        public DetailItemInfo(Element detailItem)
        {
            DIFamInst = detailItem as FamilyInstance;
            DIName = DIFamInst.LookupParameter("Panel Name - Detail");
            DIEqConId = DIFamInst.LookupParameter("EqConId");
            
            DINumberOfPhases = DIFamInst.LookupParameter("Number of Phases - Detail");
            DICurrent = DIFamInst.LookupParameter("Current - Detail");
            DIPhasing = DIFamInst.LookupParameter("New, Existing, Demo (1,2,3)");
            DIEqConIdConnSource = DIFamInst.LookupParameter("EqConId Connection Source");
            //DISCCR = DIFamInst.LookupParameter("Short Circuit Rating - Detail");
            //collect converted voltage information
            string paramName;

            if (DIFamInst.Symbol.Family.Name.Equals("E_DI_OL_XFMR"))
            {
                paramName = "Primary Voltage";
            }
            else
            {
                paramName = "E_Voltage";
            }

            DIVoltage = DIFamInst.LookupParameter(paramName);

            double val = DIVoltage.AsDouble();
            ForgeTypeId unitTypeId = DIVoltage.GetUnitTypeId();

            DIActualVoltage = UnitUtils.ConvertFromInternalUnits(val, unitTypeId);
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
        /// Get or set the EqConId Connected Source parameter of this DetailItem.
        /// </summary>
        /// <value>Return the EqConId Connected Source value of this DetailItem.</value>
        public string EqConIdConnectedSource
        {
            get { return DIEqConIdConnSource.AsString(); }
            set { DIEqConIdConnSource.Set(value); }
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
        /// Get the human readable voltage value of the Detail Item.
        /// </summary>
        public double GetActualVoltage
        {
            get { return DIActualVoltage; }
        }

        /// <summary>
        /// Get or set the E_Number of Poles parameter of this DetailItem.
        /// </summary>
        /// <value>Return the integer value of the E_Number of Poles parameter of this DetailItem.</value>
        public int PhaseNum
        {
            get { return DINumberOfPhases.AsInteger(); }
            set { DINumberOfPhases.Set(value); }
        }

        /// <summary>
        /// Get or set the Current - Detail parameter of this DetailItem
        /// </summary>
        /// <value>Return the double value of the Current - Detail parameter of this DetailItem.</value>
        public double Current
        {
            get { return DICurrent.AsDouble(); }
            set { DICurrent.Set(value); }
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

        ///// <summary>
        ///// Get or set the Short Circuit Rating - Detail parameter of this DetailItem.
        ///// </summary>
        //public double SCCR
        //{
        //    get { return DISCCR.AsDouble(); }
        //    set { DISCCR.Set(value); }
        //}
    }
}
