using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class AppsMonthlyModelBuilderTests
    {
        [Fact]
        public void Grouping()
        {
            var paymentOne = new Payment();
            var paymentTwo = new Payment();


            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            NewBuilder().Build(payments, Enumerable.Empty<Learner>(), Enumerable.Empty<ContractAllocation>()).Should().HaveCount(1);
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey()
        {
            var matchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef,"LearnAimRef")
                .Build();

            var nonMatchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef, "NotLearnAimRef")
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                matchingLearningDelivery,
                nonMatchingLearningDelivery,
            };

            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40, 
                10, 
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(learner, recordKey).Should().BeSameAs(matchingLearningDelivery);
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey_NoMatch()
        {
            var nonMatchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef, "NotLearnAimRef")
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                nonMatchingLearningDelivery,
            };

            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(learner, recordKey).Should().BeNull();
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey_NullLearner()
        {
            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(null, recordKey).Should().BeNull();
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey_NullLearningDeliveries()
        {
            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, null)
                .Build();

            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(learner, recordKey).Should().BeNull();
        }

        [Theory]
        [InlineData("16-18 Apprenticeship (From May 2017) Levy Contract", "LEVY1799")]
        [InlineData("16-18 Apprenticeship (Employer on App Service) Levy funding", "LEVY1799")]
        [InlineData("19+ Apprenticeship (From May 2017) Levy Contract",	"LEVY1799")]
        [InlineData("19+ Apprenticeship (Employer on App Service) Levy funding", "LEVY1799")]
        [InlineData("16-18 Apprenticeship (From May 2017) Non-Levy Contract", "APPS2021")]
        [InlineData("16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)", "APPS2021")]
        [InlineData("16-18 Apprenticeship Non-Levy Contract (procured)", "16-18NLAP2018")]
        [InlineData("16-18 Apprenticeship (Employer on App Service) Non-Levy funding", "NONLEVY2019")]
        [InlineData("19+ Apprenticeship (From May 2017) Non-Levy Contract", "APPS2021")]
        [InlineData("19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)", "APPS2021")]
        [InlineData("19+ Apprenticeship Non-Levy Contract (procured)", "ANLAP2018")]
        [InlineData("19+ Apprenticeship (Employer on App Service) Non-Levy funding", "NONLEVY2019")]
        [InlineData("NotAMatch", null)]
        public void FundingStreamPeriodForReportingAimFundingLineType(string reportingAimFundingLineType, string fundingStreamPeriod)
        {
            NewBuilder().FundingStreamPeriodForReportingAimFundingLineType(reportingAimFundingLineType).Should().Be(fundingStreamPeriod);
        }

        [Fact]
        public void BuildContractNumberLookup()
        {
            var contractAllocations = new List<ContractAllocation>()
            {
                new ContractAllocationBuilder().With(ca => ca.FundingStreamPeriod, "One").With(ca => ca.ContractAllocationNumber, "ABC").Build(),
                new ContractAllocationBuilder().With(ca => ca.FundingStreamPeriod, "One").With(ca => ca.ContractAllocationNumber, "DEF").Build(),
                new ContractAllocationBuilder().With(ca => ca.FundingStreamPeriod, "Two").With(ca => ca.ContractAllocationNumber, "GHI").Build(),
            };

            var contractNumberLookup = NewBuilder().BuildContractNumbersLookup(contractAllocations);

            contractNumberLookup.Should().HaveCount(2);
            contractNumberLookup["One"].Should().Be("ABC; DEF");

            contractNumberLookup["Two"].Should().Be("GHI");
        }

        [Fact]
        public void BuildContractNumberLookup_EmptySet()
        {
            NewBuilder().BuildContractNumbersLookup(Enumerable.Empty<ContractAllocation>()).Should().HaveCount(0);
        }

        private AppsMonthlyModelBuilder NewBuilder()
        {
            return new AppsMonthlyModelBuilder();
        }
    }
}
