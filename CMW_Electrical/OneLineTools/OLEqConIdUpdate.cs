using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLEqConIdUpdate
{
    public class OLEqConIdUpdate
    {
        public void OLEqConIdValueUpdate(FamilyInstance detailItem, FamilyInstance panel, Document document)
        {
            //collect Project Information parameter to evaluate number of already connected OneLine stuff
            Parameter EqConIdParam = document.ProjectInformation.LookupParameter("EqConId Current Value");

            int nextEqConIdVal = EqConIdParam.AsInteger() + 1;

            //update Project Information parameter
            EqConIdParam.Set(nextEqConIdVal);

            //update DetailItem and Electrical Equipment FamilyInstances
            string famInstVal = "EqId" + nextEqConIdVal.ToString();

            detailItem.LookupParameter("EqConId").Set(famInstVal);

            panel.LookupParameter("EqConId").Set(famInstVal);
        }
    }
}
