using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class LearningDeliveriesBuilderTests
    {
        [Fact]
        public void GetLearningDeliveryForRecordKey()
        {
            var matchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .Build();

            var nonMatchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "NotLearningRefNumber")
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                matchingLearningDelivery,
                nonMatchingLearningDelivery,
            };

            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var recordKey = new AppsCoInvestmentRecordKey(
                "LearnRefNumber",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30);

            NewBuilder().GetLearningDeliveryForRecord(learner, recordKey).Should().BeSameAs(matchingLearningDelivery);
        }

        [Theory]
        [InlineData("LDM","356", true)]
        [InlineData("LDM","361", true)]
        [InlineData("ABC","361", false)]
        [InlineData("ABC","123", false)]
        public void HasLdm356Or361_Tests(string type, string code, bool result)
        {
            var learningDeliveryFam = new LearningDeliveryFamBuilder()
                .With(ldfam => ldfam.Type, type)
                .With(ldfam => ldfam.Code, code)
                .Build();
            var learningDeliveryFams = new List<LearningDeliveryFam> {learningDeliveryFam};

            var learningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .With(ld => ld.LearningDeliveryFams, learningDeliveryFams)
                .Build();

            NewBuilder().HasLdm356Or361(learningDelivery).Should().Be(result);
        }
        private LearningDeliveriesBuilder NewBuilder()
        {
            return new LearningDeliveriesBuilder();
        }
    }
}
