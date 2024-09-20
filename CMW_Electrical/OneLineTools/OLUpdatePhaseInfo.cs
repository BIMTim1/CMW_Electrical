using CMW_Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLUpdatePhaseInfo
{
    public class OLUpdatePhaseInfoClass
    {
        public void SetDetailItemPhaseInfo(DetailItemInfo detInfo, ElecEquipInfo equipInfo)
        {
            //confirm existing and demo settings of ElecEquipInfo
            if (equipInfo.IsDemoed != "None")
            {
                detInfo.Phase = 3;
            }
            else
            {
                if (equipInfo.IsExisting == 1)
                {
                    detInfo.Phase = 2;
                }
            }
        }
    }
}
