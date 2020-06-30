using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class LearnersBuilderTests
    {

        [Theory]
        [InlineData("learnref1")]
        [InlineData("LearnRef2")]
        [InlineData("LEARNREF3")]
        public void GetLearnerForRecord_Test(string learnRefNumber)
        {
            IDictionary<string, Learner> learnerDictionary = new Dictionary<string, Learner>(StringComparer.OrdinalIgnoreCase);
           learnerDictionary.Add(learnRefNumber, new LearnerBuilder().With(l => l.LearnRefNumber, learnRefNumber).Build());
           
            var learnersBuilder = NewBuilder();

            var result = learnersBuilder.GetLearnerForRecord(learnerDictionary, new AppsCoInvestmentRecordKey(learnRefNumber, null,0,0,0,0));

            result.LearnRefNumber.Should().Be(learnRefNumber);
        }

        private LearnersBuilder NewBuilder()
        {
            return new LearnersBuilder();
        }
    }
}
