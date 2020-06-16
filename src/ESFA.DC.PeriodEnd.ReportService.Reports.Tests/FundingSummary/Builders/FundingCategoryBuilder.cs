using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.FundingSummary.Builders
{
    public class FundingCategoryBuilder : AbstractBuilder<FundingCategory>
    {
        private const int CurrentPeriod = 1;
        public FundingCategoryBuilder()
        {
            modelObject = new FundingCategory
            {
                FundingSubCategories = new List<FundingSubCategory>
                {
                    new FundingSubCategoryBuilder()
                        .With(b => b.CurrentPeriod, CurrentPeriod)
                        .With(b => b.FundingSubCategoryTitle, "SubCategory1").Build(),
                    new FundingSubCategoryBuilder()
                        .With(b => b.CurrentPeriod, CurrentPeriod)
                        .With(b => b.FundingSubCategoryTitle, "SubCategory2").Build()
                },
            };
        }
    }
}
