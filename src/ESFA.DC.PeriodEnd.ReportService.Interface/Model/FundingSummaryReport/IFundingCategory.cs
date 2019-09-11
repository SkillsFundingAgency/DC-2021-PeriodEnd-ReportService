using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport
{
    public interface IFundingCategory : IFundingSummaryReportRow
    {
        List<IFundingSubCategory> FundingSubCategories { get; }

        string FundingCategoryTitle { get; }

        string CumulativeFundingCategoryTitle { get; }

        decimal CumulativePeriod1 { get; }

        decimal CumulativePeriod2 { get; }

        decimal CumulativePeriod3 { get; }

        decimal CumulativePeriod4 { get; }

        decimal CumulativePeriod5 { get; }

        decimal CumulativePeriod6 { get; }

        decimal CumulativePeriod7 { get; }

        decimal CumulativePeriod8 { get; }

        decimal CumulativePeriod9 { get; }

        decimal CumulativePeriod10 { get; }

        decimal CumulativePeriod11 { get; }

        decimal CumulativePeriod12 { get; }

        string Note { get; }
    }
}
