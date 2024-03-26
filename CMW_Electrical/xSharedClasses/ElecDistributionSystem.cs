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
    internal class ElecDistributionSystem
    {
        //private readonly ElementId ElecDisId;
        private readonly string ElecDisName;
        private readonly double ElecDisLTLVoltage;
        private readonly double ElecDisLTGVoltage;
        private readonly int ElecDisPhase;

        public ElecDistributionSystem(Document document, ElementId elemId)
        {
            DistributionSysType disType = document.GetElement(elemId) as DistributionSysType;
            ElecDisName = disType.LookupParameter("Type Name").AsString();

            //collect voltage information
            ElecDisLTLVoltage = disType.VoltageLineToLine.LookupParameter("Actual Voltage").AsDouble();
            ElecDisLTGVoltage = disType.VoltageLineToGround.LookupParameter("Actual Voltage").AsDouble();

            ElecDisPhase = disType.LookupParameter("Phase").AsInteger();
        }

        /// <summary>
        /// Get the Type Name of the applied Electrical DistributionSysType.
        /// </summary>
        public string Name
        {
            get { return ElecDisName; }
        }

        /// <summary>
        /// Get the Line to Line Voltage of the applied Electrical DistributionSysType.
        /// </summary>
        public double GetLineToLineVoltage
        {
            get { return ElecDisLTLVoltage; }
        }

        /// <summary>
        /// Get the Line to Ground Voltage of the applied Electrical DistributionSysType.
        /// </summary>
        public double GetLineToGroundVoltage
        {
            get { return ElecDisLTGVoltage; }
        }

        /// <summary>
        /// Get the Phase of the applied Electrical DistributionSysType
        /// </summary>
        public int GetPhase
        {
            get { return ElecDisPhase; }
        }
    }
}
