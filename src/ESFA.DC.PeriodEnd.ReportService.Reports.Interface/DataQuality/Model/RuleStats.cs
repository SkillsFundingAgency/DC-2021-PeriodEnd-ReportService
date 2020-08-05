namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class RuleStats
    {
        public string RuleName { get; set; }

        public int ProviderCount { get; set; }

        public int LearnerCount { get; set; }

        public int TotalErrorCount { get; set; }
    }
}
