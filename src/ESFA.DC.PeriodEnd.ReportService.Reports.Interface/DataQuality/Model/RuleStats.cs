namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model
{
    public class RuleStats
    {
        public string RuleName { get; set; }

        public string ErrorMessage { get; set; }

        public int Providers { get; set; }

        public int Learners { get; set; }

        public int NoOfErrors { get; set; }
    }
}
