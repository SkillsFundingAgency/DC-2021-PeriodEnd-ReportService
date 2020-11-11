using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.UYPSummaryView
{
    public class UYPSummaryViewPersistenceMapperTests
    {
        [Fact]
        public void Test_ReportMapping()
        {
            var mapper = new UYPSummaryViewPersistenceMapper();
            var items = new List<LearnerLevelViewSummaryModel>()
            {
                new LearnerLevelViewSummaryModel()
                {
                    TotalEarningsForThisPeriod = 100.50m,
                    NumberofCoInvestmentsToCollect = 10,
                    NumberofHBCP = 20,
                    NumberofOthers = 30,
                    NumberofClawbacks = 40,
                    NumberofDatalocks = 50,
                    NumberofEarningsReleased = 60,
                    NumberofLearners = 70,
                    TotalCoInvestmentCollectedToDate = 10.10m,
                    TotalCostOfHBCPForThisPeriod = 10.20m,
                    ESFAPlannedPaymentsForThisPeriod = 10.30m,
                    TotalCostOfDataLocksForThisPeriod = 10.40m,
                    EarningsReleased = 10.50m,
                    TotalCostofOthersForThisPeriod = 10.60m,
                    TotalCostofClawbackForThisPeriod = 10.70m,
                    TotalPaymentsToDate = 10.80m,
                    TotalEarningsToDate = 10.90m,
                    CoInvestmentPaymentsToCollectForThisPeriod = 11.00m,
                }
            };

            var mockReportServiceContext = new Mock<IReportServiceContext>();
            mockReportServiceContext.Setup(x => x.ReturnPeriod).Returns(10);
            mockReportServiceContext.Setup(x => x.Ukprn).Returns(10000);

            var result = mapper.Map(mockReportServiceContext.Object, items);
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);

            var model = result.First();

            model.Ukprn.Should().Be(10000);
            model.ReturnPeriod.Should().Be(10);

            model.TotalEarningsForThisPeriod.Should().Be(100.50m);
            model.NumberofCoInvestmentsToCollect.Should().Be(10);
            model.NumberofHBCP.Should().Be(20);
            model.NumberofOthers.Should().Be(30);
            model.NumberofClawbacks.Should().Be(40);
            model.NumberofDatalocks.Should().Be(50);
            model.NumberofEarningsReleased.Should().Be(60);
            model.NumberofLearners.Should().Be(70);
            model.TotalCoInvestmentCollectedToDate.Should().Be(10.10m);
            model.TotalCostOfHBCPForThisPeriod.Should().Be(10.20m);
            model.ESFAPlannedPaymentsForThisPeriod.Should().Be(10.30m);
            model.TotalCostOfDataLocksForThisPeriod.Should().Be(10.40m);
            model.EarningsReleased.Should().Be(10.50m);
            model.TotalCostofOthersForThisPeriod.Should().Be(10.60m);
            model.TotalCostofClawbackForThisPeriod.Should().Be(10.70m);
            model.TotalPaymentsToDate.Should().Be(10.80m);
            model.TotalEarningsToDate.Should().Be(10.90m);
            model.CoInvestmentPaymentsToCollectForThisPeriod.Should().Be(11.00m);
        }
    }
}
