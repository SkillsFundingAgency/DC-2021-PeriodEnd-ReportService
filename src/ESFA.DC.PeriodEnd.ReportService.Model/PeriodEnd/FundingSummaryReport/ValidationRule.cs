namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ValidationRule
    {
        public ValidationRule() { }

        public string RuleName { get; set; }
        public bool Desktop { get; set; }
        public bool Online { get; set; }
    }
}