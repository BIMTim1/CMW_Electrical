using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLineFind
{
    public class ElementData
    {
        private string _famType;
        private string _panelName;
        private ElementId _elementId;

        public ElementData(Element elem)
        {
            _famType = elem.LookupParameter("Family and Type").AsValueString();
            
            if (elem.Category.Name == "Electrical Equipment")
            {
                _panelName = elem.LookupParameter("Panel Name").AsString();
            }
            else
            {
                _panelName = elem.LookupParameter("Panel Name - Detail").AsString();
            }

            _elementId = elem.Id;
        }

        public string EFamilyType
        {
            get { return _famType; }
        }

        public string EPanelName
        {
            get { return _panelName; }
        }

        public ElementId EElementId
        {
            get { return _elementId; }
        }
    }
}
