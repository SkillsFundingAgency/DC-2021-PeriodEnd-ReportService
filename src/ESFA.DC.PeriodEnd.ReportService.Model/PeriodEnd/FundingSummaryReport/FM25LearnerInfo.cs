using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class FM25LearnerInfo
    {
        public int Ukprn { get; set; }

        public IList<FM25Learner> Learners { get; set; }
    }
}