using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class AppsMonthlyTests
    {
        [Fact]
        public async Task GenerateReport()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var ukprn = 123456;
            var fileName = "FileName";
            var container = "Container";

            var learners = Array.Empty<Learner>();
            var payments = Array.Empty<Payment>();
            var larsLearningDeliveries = Array.Empty<LarsLearningDelivery>();
            var earnings = Array.Empty<Earning>();
            var contractAllocations = Array.Empty<ContractAllocation>();
            var priceEpisodes = Array.Empty<AecApprenticeshipPriceEpisode>();

            var rows = Array.Empty<AppsMonthlyRecord>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);

            var csvFileServiceMock = new Mock<ICsvFileService>();

            var fileNameServiceMock = new Mock<IFileNameService>();

            fileNameServiceMock.Setup(s => s.GetFilename(reportServiceContext.Object, "Apps Monthly Payment Report", OutputTypes.Csv, true)).Returns(fileName);

            var dataProviderMock = new Mock<IAppsMonthlyPaymentsDataProvider>();

            dataProviderMock.Setup(p => p.GetLearnersAsync(ukprn, cancellationToken)).ReturnsAsync(learners);
            dataProviderMock.Setup(p => p.GetPaymentsAsync(ukprn, academicYear, cancellationToken)).ReturnsAsync(payments);
            dataProviderMock.Setup(p => p.GetLarsLearningDeliveriesAsync(learners, cancellationToken)).ReturnsAsync(larsLearningDeliveries);
            dataProviderMock.Setup(p => p.GetEarningsAsync(ukprn, cancellationToken)).ReturnsAsync(earnings);
            dataProviderMock.Setup(p => p.GetContractAllocationsAsync(ukprn, cancellationToken)).ReturnsAsync(contractAllocations);
            dataProviderMock.Setup(p => p.GetPriceEpisodesAsync(ukprn, cancellationToken)).ReturnsAsync(priceEpisodes);

            var modelBuilderMock = new Mock<IAppsMonthlyModelBuilder>();

            modelBuilderMock.Setup(b => b.Build(payments, learners, contractAllocations, earnings, larsLearningDeliveries, priceEpisodes)).Returns(rows);

            var report = NewReport(csvFileServiceMock.Object, fileNameServiceMock.Object, dataProviderMock.Object, modelBuilderMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            csvFileServiceMock.Verify(s => s.WriteAsync<AppsMonthlyRecord, AppsMonthlyClassMap>(rows, fileName, container, cancellationToken, null, null));
        }

        private Reports.AppsMonthly.AppsMonthly NewReport(
            ICsvFileService csvFileService = null,
            IFileNameService fileNameService = null,
            IAppsMonthlyPaymentsDataProvider appsMonthlyPaymentsDataProvider = null,
            IAppsMonthlyModelBuilder appsMonthlyPaymentModelBuilder = null,
            ILogger logger = null)
        {
            return new Reports.AppsMonthly.AppsMonthly(
                csvFileService ?? Mock.Of<ICsvFileService>(),
                fileNameService ?? Mock.Of<IFileNameService>(),
                appsMonthlyPaymentsDataProvider ?? Mock.Of<IAppsMonthlyPaymentsDataProvider>(),
                appsMonthlyPaymentModelBuilder ?? Mock.Of<IAppsMonthlyModelBuilder>(),
                logger ?? Mock.Of<ILogger>());
        }
    }
}
