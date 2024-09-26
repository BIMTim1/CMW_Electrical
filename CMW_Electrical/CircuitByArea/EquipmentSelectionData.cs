using System;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CircuitByArea
{
    public class EquipmentSelectionData
    {
        private readonly Element _element;
        private FamilyInstance _familyInstance;
        private readonly string _panelName;
        private readonly string _familyName;
        private readonly string _typeName;
        public EquipmentSelectionData(Element element)
        {
            _element = element;
            _familyInstance = _element as FamilyInstance;

            _panelName = _element.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NAME).AsString();

            _familyName = _element.LookupParameter("Family").AsValueString();

            _typeName = _element.LookupParameter("Type").AsValueString();
        }

        /// <summary>
        /// Get FamilyInstance of EquipmentSelectionData object.
        /// </summary>
        public FamilyInstance AsFamilyInstance
        {
            get { return _familyInstance; }
        }

        /// <summary>
        /// Collect _panelName, _familyName, and _typeName in a preformatted string
        /// </summary>
        public string PanelInformation => $"{_panelName}: {_familyName}, {_typeName}";
    }
}
