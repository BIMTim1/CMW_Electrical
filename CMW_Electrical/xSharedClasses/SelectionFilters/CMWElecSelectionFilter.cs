using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMW_Electrical
{
    internal class CMWElecSelectionFilter
    {
        public class EquipmentSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element.Category.Name == "Electrical Equipment";
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        public class DetailItemSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Detail Items")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
    }
}
