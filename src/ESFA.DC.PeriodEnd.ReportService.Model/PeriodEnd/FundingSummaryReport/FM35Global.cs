using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class FM35Global
    {
        public int UKPRN { get; set; }
        public string CurFundYr { get; set; }
        public string LARSVersion { get; set; }
        public string OrgVersion { get; set; }
        public string PostcodeDisadvantageVersion { get; set; }
        public string RulebaseVersion { get; set; }
        public List<FM35Learner> Learners { get; set; }
    }
}