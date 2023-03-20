using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMWElec_FailureHandlers
{
    internal class CircuitBreakerWarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failureAccessor)
        {
            //inside event handler, get all warnings
            IList<FailureMessageAccessor> failList = failureAccessor.GetFailureMessages();
            foreach (FailureMessageAccessor failure in failList)
            {
                //check FailureDefinitionIds against ones that you want to dismiss
                FailureDefinitionId failId = failure.GetFailureDefinitionId();
                //prevent Revit from showing smaller Switchboard type panel schedule warning
                if (failId == BuiltInFailures.ScheduleViewFailures.PanelScheduleSlotInconsistency) ///f71b1bc9-ce7f-449c-ada6-4cdece1e67ab
                {
                    failureAccessor.DeleteWarning(failure);
                }
            }

            return FailureProcessingResult.Continue;
        }
    }
}
