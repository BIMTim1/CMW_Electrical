using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLineTools
{
    public class EqConIdCheck
    {
        public bool EqConIdCurrentValueExists(Document document)
        {
            Parameter param = GetEqConIdCurrentValue(document);

            return param != null;
        }

        public Parameter GetEqConIdCurrentValue(Document document)
        {
            Parameter param = null;

            string paramName = "EqConId Current Value";

            ProjectInfo projectInfo = document.ProjectInformation;

            ParameterSet projectParams = projectInfo.Parameters;

            foreach (Parameter p in projectParams)
            {
                if (p.Definition.Name == paramName)
                {
                    param = p;
                }
            }

            return param;
        }
    }
}
