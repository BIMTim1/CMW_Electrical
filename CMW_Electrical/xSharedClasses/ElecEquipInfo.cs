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
    internal class ElecEquipInfo
    {
        private readonly Parameter EEName;
        private readonly Parameter EEEqConId;
        private readonly Parameter EEDistributionSystem;
        private FamilySymbol EEFamilyType;

        public ElecEquipInfo(Element elecEquip)
        {
            FamilyInstance elecEquipInstance = elecEquip as FamilyInstance;
            EEName = elecEquipInstance.LookupParameter("Panel Name");
            EEEqConId = elecEquipInstance.LookupParameter("EqConId");
            EEDistributionSystem = elecEquipInstance.LookupParameter("Distribution System");

            EEFamilyType = elecEquipInstance.Symbol;
        }

        /// <summary>
        /// Get or set the Panel Name parameter of the ElectricalEquipment FamilyInstance.
        /// </summary>
        public string Name
        {
            get { return EEName.AsString(); }
            set { EEName.Set(value); }
        }

        /// <summary>
        /// Get or set the EqConId parameter of the ElectricalEquipment FamilyInstance.
        /// </summary>
        public string EqConId
        {
            get { return EEEqConId.AsString(); }
            set { EEEqConId.Set(value); }
        }

        /// <summary>
        /// Get or set the DistributionSysType assigned to the ElectricalEquipment FamilyInstance.
        /// </summary>
        public ElementId DistributionSystem
        {
            get { return EEDistributionSystem.AsElementId(); }
            set { EEDistributionSystem.Set(value); }
        }

        /// <summary>
        /// Get or set the FamilySymbol of the ElectricalEquipment FamilyInstance.
        /// </summary>
        public FamilySymbol EquipFamSymbol
        {
            get { return EEFamilyType; }
            set { EEFamilyType = value; }
        }
    }
}
