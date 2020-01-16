using System.Collections.Generic;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Service.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Builders
{
    public class AppsCoInvestmentContributionsModelBuilderTests
    {
        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Zero_For_Zero_Inputs()
        {
            decimal? totalDueCurrentYear = 0M;
            decimal? totalDuePreviousYear = 0M;
            decimal? totalCollectedCurrentYear = 0M;
            decimal? totalCollectedPreviousYear = 0M;

            var equalityComparerMock = new Mock<IEqualityComparer<AppsCoInvestmentRecordKey>>();
            var loggerMock = new Mock<ILogger>();

            var builder = new AppsCoInvestmentContributionsModelBuilder(equalityComparerMock.Object, loggerMock.Object);
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            percent.Should().Be(0);
        }

        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Max_For_Huge_Percent()
        {
            decimal max = 99999.99M;

            decimal? totalDueCurrentYear = 10M;
            decimal? totalDuePreviousYear = 10M;
            decimal? totalCollectedCurrentYear = 100000000M;
            decimal? totalCollectedPreviousYear = 0M;

            var equalityComparerMock = new Mock<IEqualityComparer<AppsCoInvestmentRecordKey>>();
            var loggerMock = new Mock<ILogger>();

            var builder = new AppsCoInvestmentContributionsModelBuilder(equalityComparerMock.Object, loggerMock.Object);
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            percent.Should().Be(max);
        }

        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Min_For_Huge_Negative_Percent()
        {
            decimal min = -99999.99M;

            decimal? totalDueCurrentYear = 10M;
            decimal? totalDuePreviousYear = 10M;
            decimal? totalCollectedCurrentYear = -100000000M;
            decimal? totalCollectedPreviousYear = 0M;

            var equalityComparerMock = new Mock<IEqualityComparer<AppsCoInvestmentRecordKey>>();
            var loggerMock = new Mock<ILogger>();

            var builder = new AppsCoInvestmentContributionsModelBuilder(equalityComparerMock.Object, loggerMock.Object);
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            percent.Should().Be(min);
        }

        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Two_Decimal_Places()
        {
            decimal? totalDueCurrentYear = 10M;
            decimal? totalDuePreviousYear = 10M;
            decimal? totalCollectedCurrentYear = 33.131313131313M;
            decimal? totalCollectedPreviousYear = 0M;

            var equalityComparerMock = new Mock<IEqualityComparer<AppsCoInvestmentRecordKey>>();
            var loggerMock = new Mock<ILogger>();

            var builder = new AppsCoInvestmentContributionsModelBuilder(equalityComparerMock.Object, loggerMock.Object);
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            var percentString = percent.ToString();
            percentString.Substring(percentString.IndexOf(".") + 1).Length.Should().Be(2);
        }
    }
}