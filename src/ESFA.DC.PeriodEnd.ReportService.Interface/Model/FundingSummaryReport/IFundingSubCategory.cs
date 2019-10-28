using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IFundingSubCategory : IFundingSummaryReportRow
    {
        IList<IFundLineGroup> FundLineGroups { get; }

        string FundingSubCategoryTitle { get; }
    }
}