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
            string paramName = "EqConId Current Value";

            ProjectInfo projectInfo = document.ProjectInformation;

            ParameterSet projectParams = projectInfo.Parameters;

            foreach (Parameter param in projectParams)
            {
                if (param.Definition.Name == paramName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
