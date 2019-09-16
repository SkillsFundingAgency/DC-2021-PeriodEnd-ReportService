using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ALBGlobal
    {
        public int UKPRN { get; set; }
        public string LARSVersion { get; set; }
        public string PostcodeAreaCostVersion { get; set; }
        public string RulebaseVersion { get; set; }
        public List<ALBLearner> Learners { get; set; }
    }
}
