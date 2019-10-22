namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public enum FundingDataSource
    {
        FM25,   // 16-19 (excluding Apprenticeships
        FM35,   // Adult skills
        FM81,   // Other Adult
        FM99,   // Non-funded (No ESFA funding for this learning aim)
        EAS,    // Earnings Adjustment Statement
        DAS,    // DAS Payments
    }
}