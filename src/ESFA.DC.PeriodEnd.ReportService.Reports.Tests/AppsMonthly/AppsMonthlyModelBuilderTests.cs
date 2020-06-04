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

            NewBuilder().Build(payments, Enumerable.Empty<Learner>()).Should().HaveCount(1);
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

        private AppsMonthlyModelBuilder NewBuilder()
        {
            return new AppsMonthlyModelBuilder();
        }
    }
}
