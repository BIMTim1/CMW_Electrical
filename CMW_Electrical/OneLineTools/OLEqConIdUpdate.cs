﻿using Autodesk.Revit.DB;
using CMW_Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneLineTools
{
    internal class OLEqConIdUpdateClass
    {
        public void OneLineEqConIdValueUpdate(ElecEquipInfo elecEquipClass, DetailItemInfo detailItemClass, Document document)
        {
            //collect Project Information parameter to evaluate number of already connected OneLine stuff
            Parameter EqConIdParam = document.ProjectInformation.LookupParameter("EqConId Current Value");

            int nextEqConIdVal = EqConIdParam.AsInteger() + 1;

            //update Project Information parameter
            EqConIdParam.Set(nextEqConIdVal);

            //update DetailItem and Electrical Equipment FamilyInstances
            string famInstVal = "EqId" + nextEqConIdVal.ToString();

            elecEquipClass.EqConId = famInstVal;

            detailItemClass.EqConId = famInstVal;

            //update Project Information parameter
            EqConIdParam.Set(nextEqConIdVal);
        }

        public void OneLineEqConIdValueUpdateTemporary(ElecEquipInfo elecEquipClass, Document document)
        {
            //update ElecEquipInfo item
            string famInstVal = "not assigned";

            elecEquipClass.EqConId = famInstVal;
        }

        public void OneLineEqConIdValueUpdateTemporary(DetailItemInfo detailItemClass, Document document)
        {
            //update DetailItemInfo item
            string famInstVal = "not assigned";

            detailItemClass.EqConId = famInstVal;
        }
    }
}
