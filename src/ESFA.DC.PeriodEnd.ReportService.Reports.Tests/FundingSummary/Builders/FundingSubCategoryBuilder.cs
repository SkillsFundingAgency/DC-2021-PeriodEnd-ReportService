using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.FundingSummary.Builders
{
    public class FundingSubCategoryBuilder : AbstractBuilder<FundingSubCategory>
    {
        private const int CurrentPeriod = 1;

        public FundingSubCategoryBuilder()
        {
            modelObject = new FundingSubCategory
            {
                FundLineGroups = new List<FundLineGroup>
                {
                    new FundLineGroupBuilder()
                        .With(m => m.ContractAllocationNumber, "C1")
                        .With(b => b.CurrentPeriod, CurrentPeriod)
                        .Build(),
                    new FundLineGroupBuilder()
                        .With(b => b.ContractAllocationNumber, "C2")
                        .With(b => b.CurrentPeriod, CurrentPeriod)
                        .Build()
                }
            };
        }
    }
}
