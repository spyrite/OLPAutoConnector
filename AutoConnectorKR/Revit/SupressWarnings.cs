using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace OLP.AutoConnectorKR.Revit
{
    public class SupressWarnings : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failures = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor fail in failures)
                if (fail.GetSeverity() == FailureSeverity.Warning || fail.GetSeverity() == FailureSeverity.None)
                    failuresAccessor.DeleteWarning(fail);

            return FailureProcessingResult.Continue;
        }
    }
}
