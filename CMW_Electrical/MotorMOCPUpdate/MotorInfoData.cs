using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorMOCPUpdate
{
    public class MotorInfoData
    {
        private string _name;
        private string _levelName;
        private string _mocp;

        public MotorInfoData(FamilyInstance familyInstance)
        {
            _name = familyInstance.LookupParameter("UID").AsString();

            _levelName = familyInstance.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM).AsValueString();

            _mocp = familyInstance.LookupParameter("MES_(MFS) MOCP").AsString();
        }

        public string GetMotorUID
        {
            get { return _name; }
        }

        public string GetLevelName
        {
            get { return _levelName; }
        }

        public string GetMOCP
        {
            get { return _mocp; }
        }
    }
}
