using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using Moq;
using Xunit;


namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class AppsCoInvestmentTests
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
            var aecPriceEpisodePeriodisedValues = Array.Empty<AECApprenticeshipPriceEpisodePeriodisedValues>();
            var rows = Array.Empty<AppsCoInvestmentRecord>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);

            var csvFileServiceMock = new Mock<ICsvFileService>();
            var fileNameServiceMock = new Mock<IFileNameService>();

            fileNameServiceMock.Setup(s => s.GetFilename(reportServiceContext.Object, "Apps Co-Investment Contributions Report", OutputTypes.Csv, true)).Returns(fileName);

            var dataProviderMock = new Mock<IAppsCoInvestmentDataProvider>();

            dataProviderMock.Setup(p => p.GetLearnersAsync(ukprn, cancellationToken)).ReturnsAsync(learners);
            dataProviderMock.Setup(p => p.GetPaymentsAsync(ukprn, cancellationToken)).ReturnsAsync(payments);
            dataProviderMock.Setup(p => p.GetAecPriceEpisodePeriodisedValuesAsync(ukprn, cancellationToken)).ReturnsAsync(aecPriceEpisodePeriodisedValues);
           
            var modelBuilderMock = new Mock<IAppsCoInvestmentModelBuilder>();

            modelBuilderMock.Setup(b => b.Build(learners, payments, aecPriceEpisodePeriodisedValues)).Returns(rows);

            var report = NewReport(csvFileServiceMock.Object, fileNameServiceMock.Object, dataProviderMock.Object, modelBuilderMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            csvFileServiceMock.Verify(s => s.WriteAsync<AppsCoInvestmentRecord, AppsCoInvestmentClassMap>(rows, fileName, container, cancellationToken, null, null));
        }

        private Reports.AppsCoInvestment.AppsCoInvestment NewReport(
            ICsvFileService csvFileService = null,
            IFileNameService fileNameService = null,
            IAppsCoInvestmentDataProvider appsCoInvestmentDataProvider = null,
            IAppsCoInvestmentModelBuilder appsCoInvestmentModelBuilder = null,
            IAppsCoInvestmentPersistenceService appsCoInvestmentPersistenceService = null)
        {
            return new Reports.AppsCoInvestment.AppsCoInvestment(
                csvFileService ?? Mock.Of<ICsvFileService>(),
                fileNameService ?? Mock.Of<IFileNameService>(),
                appsCoInvestmentDataProvider ?? Mock.Of<IAppsCoInvestmentDataProvider>(),
                appsCoInvestmentModelBuilder ?? Mock.Of<IAppsCoInvestmentModelBuilder>(),
                appsCoInvestmentPersistenceService ?? Mock.Of<IAppsCoInvestmentPersistenceService>());
        }
    }
}
