using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.FundingSummary.Builders;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.FundingSummary
{
    public class FundingCategoryBuilderTests
    {
        [Fact]
        public void Test()
        {
            var fundingCategory = new FundingCategoryBuilder()
                .With(b => b.CurrentPeriod, 2)
                .With(b => b.ContractAllocationNumber, "Contract1")
                .Build();

            fundingCategory.CurrentPeriod.Should().Be(2);
            fundingCategory.ContractAllocationNumber.Should().Be("Contract1");
            
        }
    }
}
