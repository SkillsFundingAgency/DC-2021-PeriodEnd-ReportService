using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model
{
    public class FundingSummaryReportModel
    {
        public FundingSummaryReportModel(
            IDictionary<string, string> headerData,
            List<FundingCategory> fundingCategories,
            IDictionary<string, string> footerData)
        {
            FundingCategories = fundingCategories ?? new List<FundingCategory>();
            HeaderData = headerData ?? new Dictionary<string, string>();
            FooterData = footerData ?? new Dictionary<string, string>();
        }

        public IDictionary<string, string> HeaderData { get; }

        public List<FundingCategory> FundingCategories { get; }

        public IDictionary<string, string> FooterData { get; }
    }
}