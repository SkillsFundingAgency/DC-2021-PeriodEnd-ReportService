using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport.Model
{
    public class FundingSummaryReportModel : IFundingSummaryReport
    {
        public FundingSummaryReportModel(IDictionary<string, string> headerData, List<IFundingCategory> fundingCategories, IDictionary<string, string> footerData)
        {
            HeaderData = headerData;
            FundingCategories = fundingCategories ?? new List<IFundingCategory>();
            FooterData = footerData;
        }

        public List<IFundingCategory> FundingCategories { get; }

        public IDictionary<string, string> HeaderData { get; }

        public IDictionary<string, string> FooterData { get; }
    }
}