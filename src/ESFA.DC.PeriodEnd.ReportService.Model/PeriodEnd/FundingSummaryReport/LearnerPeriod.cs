namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class LearnerPeriod
    {
        public LearnerPeriod() { }

        public string LearnRefNumber { get; set; }
        public int? Period { get; set; }
        public decimal? LnrOnProgPay { get; set; }
    }
}