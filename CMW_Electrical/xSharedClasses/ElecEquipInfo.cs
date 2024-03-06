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

namespace CMW_Electrical
{
    internal class ElecEquipInfo
    {
        private readonly FamilyInstance EEFamInst;
        private readonly Parameter EEName;
        private readonly Parameter EEEqConId;
        private readonly Parameter EEDistributionSystem;
        private FamilySymbol EEFamilyType;
        private readonly PanelScheduleView EEScheduleView;

        public ElecEquipInfo(Document document, Element elecEquip)
        {
            EEFamInst = elecEquip as FamilyInstance;
            EEName = EEFamInst.LookupParameter("Panel Name");
            EEEqConId = EEFamInst.LookupParameter("EqConId");
            EEDistributionSystem = EEFamInst.LookupParameter("Distribution System");

            EEFamilyType = EEFamInst.Symbol;

            ElementClassFilter filter = new ElementClassFilter(typeof(PanelScheduleView));
            EEScheduleView = document.GetElement(EEFamInst.GetDependentElements(filter).First()) as PanelScheduleView;
        }

        /// <summary>
        /// Get the FamilyInstance of the Electrical Equipment Element.
        /// </summary>
        /// <value>Get the FamilyInstance of the Electrical Equipment Element.</value>
        public FamilyInstance GetFamilyInstance
        {
            get { return EEFamInst; }
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

        /// <summary>
        /// Get the PanelScheduleView of the Electrical Equipment FamilyInstance
        /// </summary>
        public PanelScheduleView GetScheduleView
        {
            get { return EEScheduleView;  }
        }
    }
}
