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
        private string Name;
        private string EqConId;

        public ElecEquipInfo(Element elecEquip)
        {
            FamilyInstance elecEquipInstance = elecEquip as FamilyInstance;
            string elecEquipName = elecEquipInstance.LookupParameter("Panel Name").AsString();
            string elecEquipEqConId = elecEquipInstance.LookupParameter("EqConId").AsString();
            Name = elecEquipName;
            EqConId = elecEquipEqConId;
        }

        public string EEName
        {
            get { return Name; }
            set { Name = value; }
        }

        public string EEEqConId
        {
            get { return EqConId; }
            set { EqConId = value; }
        }
    }
}
