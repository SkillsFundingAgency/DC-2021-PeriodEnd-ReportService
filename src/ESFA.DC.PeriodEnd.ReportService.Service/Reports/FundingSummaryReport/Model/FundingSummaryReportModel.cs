using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport.Model
{
    public class FundingSummaryReportModel : IFundingSummaryReport
    {
        public FundingSummaryReportModel(List<IFundingCategory> fundingCategories)
        {
            FundingCategories = fundingCategories ?? new List<IFundingCategory>();
        }

        public List<IFundingCategory> FundingCategories { get; }
    }
}