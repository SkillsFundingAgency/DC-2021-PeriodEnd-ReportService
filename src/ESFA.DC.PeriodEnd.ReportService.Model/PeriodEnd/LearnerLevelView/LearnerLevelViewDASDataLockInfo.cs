using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView
{
    public class LearnerLevelViewDASDataLockInfo
    {
        public long UkPrn { get; set; }

        public List<LearnerLevelViewDASDataLockModel> DASDataLocks { get; set; }
    }
}
