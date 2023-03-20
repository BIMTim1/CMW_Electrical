using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMWElec_FailureHandlers
{
    public class CreateFailureAdvancedHandler: IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failureAccessor)
        {
            IList<FailureMessageAccessor> failList = failureAccessor.GetFailureMessages();

            if (failList.Count() == 0)
            {
                return FailureProcessingResult.Continue;
            }
            if (failureAccessor.GetSeverity() == FailureSeverity.Warning)
            {
                foreach (FailureMessageAccessor currentMessage in failList)
                {
                    failureAccessor.DeleteWarning(currentMessage);
                }
            }

            return FailureProcessingResult.Continue;
        }
    }
}
