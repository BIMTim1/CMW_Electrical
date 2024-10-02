using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace CreatePanelSchedules
{
    public class PhaseInformation
    {
        private readonly string _phaseName;
        private readonly ElementId _elementId;
        public PhaseInformation(Phase phase)
        {
            _phaseName = phase.Name;

            _elementId = phase.Id;
        }

        /// <summary>
        /// Get the Name of the Phase Element.
        /// </summary>
        public string GetPhaseName
        {
            get { return _phaseName; }
        }

        /// <summary>
        /// get the ElementId of the Phase Element.
        /// </summary>
        public ElementId GetPhaseElementId
        {
            get { return _elementId; }
        }
    }
}
