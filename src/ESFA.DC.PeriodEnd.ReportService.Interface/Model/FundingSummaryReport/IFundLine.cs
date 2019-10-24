namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IFundLine : IFundingSummaryReportRow
    {
        bool IncludeInTotals { get; }
    }
}