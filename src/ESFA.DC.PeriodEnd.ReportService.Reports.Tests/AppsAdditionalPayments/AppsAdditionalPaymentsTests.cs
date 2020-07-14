using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Persistance;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentsTests
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
            var aecLearningDeliveries = Array.Empty<AecLearningDelivery>();
            var priceEpisodes = Array.Empty<ApprenticeshipPriceEpisodePeriodisedValues>();

            var rows = Array.Empty<AppsAdditionalPaymentReportModel>();

            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);

            var csvFileServiceMock = new Mock<ICsvFileService>();

            var fileNameServiceMock = new Mock<IFileNameService>();

            fileNameServiceMock.Setup(s => s.GetFilename(reportServiceContext.Object, $"{ukprn} Apps Additional Payments Report", OutputTypes.Csv, true, false)).Returns(fileName);

            var dataProviderMock = new Mock<IAppsAdditionalPaymentsDataProvider>();

            dataProviderMock.Setup(p => p.GetLearnersAsync(reportServiceContext.Object, cancellationToken)).ReturnsAsync(learners);
            dataProviderMock.Setup(p => p.GetPaymentsAsync(reportServiceContext.Object, cancellationToken)).ReturnsAsync(payments);
            dataProviderMock.Setup(p => p.GetAecLearningDeliveriesAsync(reportServiceContext.Object, cancellationToken)).ReturnsAsync(aecLearningDeliveries);
            dataProviderMock.Setup(p => p.GetPriceEpisodesAsync(reportServiceContext.Object, cancellationToken)).ReturnsAsync(priceEpisodes);

            var modelBuilderMock = new Mock<IAppsAdditionalPaymentsModelBuilder>();

            modelBuilderMock.Setup(b => b.Build(payments, learners, aecLearningDeliveries, priceEpisodes)).Returns(rows);

            var report = NewReport(csvFileServiceMock.Object, fileNameServiceMock.Object, dataProviderMock.Object, modelBuilderMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            csvFileServiceMock.Verify(s => s.WriteAsync<AppsAdditionalPaymentReportModel, AppsAdditionalPaymentsClassMap>(rows, fileName, container, cancellationToken, null, null));
        }

        private Reports.AppsAdditionalPayments.AppsAdditionalPayment NewReport(
            ICsvFileService csvFileService = null,
            IFileNameService fileNameService = null,
            IAppsAdditionalPaymentsDataProvider appsAdditionalPaymentsDataProvider = null,
            IAppsAdditionalPaymentsModelBuilder appsAdditionalPaymentModelBuilder = null,
            IReportDataPersistanceService<ReportData.Model.AppsAdditionalPayment> persistanceService = null,
            IAppsAdditionalPaymentPersistanceMapper appsAdditionalPaymentPersistanceMapper = null)
        {
            return new Reports.AppsAdditionalPayments.AppsAdditionalPayment(
                csvFileService ?? Mock.Of<ICsvFileService>(),
                fileNameService ?? Mock.Of<IFileNameService>(),
                appsAdditionalPaymentsDataProvider ?? Mock.Of<IAppsAdditionalPaymentsDataProvider>(),
                appsAdditionalPaymentModelBuilder ?? Mock.Of<IAppsAdditionalPaymentsModelBuilder>(),
                persistanceService ?? Mock.Of<IReportDataPersistanceService<ReportData.Model.AppsAdditionalPayment>>(),
                appsAdditionalPaymentPersistanceMapper ?? Mock.Of<IAppsAdditionalPaymentPersistanceMapper>());
        }
    }
}