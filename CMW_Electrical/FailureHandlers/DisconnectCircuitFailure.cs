using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMWElec_FailureHandlers
{
    public class DisconnectCircuitFailure: IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failureAccessor)
        {
            //IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
            //inside event handler, get all warnings
            IList<FailureMessageAccessor> failList = failureAccessor.GetFailureMessages();
            foreach (FailureMessageAccessor failure in failList)
            {
                //check FailureDefinitionIds against ones that you want to dismiss
                FailureDefinitionId failId = failure.GetFailureDefinitionId();
                //prevent Revit from showing Disconnected Circuit warnings
                if (failId == BuiltInFailures.ElectricalFailures.FamilyMismatchCircuit)
                {
                    failureAccessor.ResolveFailure(failure);

                    return FailureProcessingResult.ProceedWithCommit;
                }
                else if (failId == BuiltInFailures.ConnectorFailures.AllElementsRemovedFromCircuitGroup)
                {
                    failureAccessor.DeleteWarning(failure);
                }
            }

            return FailureProcessingResult.Continue;
        }
    }
}
