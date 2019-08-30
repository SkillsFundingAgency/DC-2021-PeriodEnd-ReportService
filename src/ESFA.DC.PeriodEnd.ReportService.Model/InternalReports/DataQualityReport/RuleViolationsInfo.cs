
namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataQualityReport
{
    public sealed class RuleViolationsInfo
    {
        public string RuleName { get; set; }

        public string ErrorMessage { get; set; }

        public int Providers { get; set; }

        public int Learners { get; set; }

        public int NoOfErrors { get; set; }
    }
}
