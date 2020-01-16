using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Interface;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Stubs;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Reports
{
    public class TestFundingSummaryReport
    {
        [Fact]
        public async Task SystemTest()
        {
            var container = "Output";
            var cancellationToken = CancellationToken.None;
            var ukprn = 10005788;

            var fcsService = new Mock<IFCSProviderService>();
            fcsService.Setup(f => f.GetContractAllocationNumberFSPCodeLookupAsync(ukprn, cancellationToken)).ReturnsAsync(new Dictionary<string, string>());

            var reportServiceContextMock = new Mock<IReportServiceContext>();
            var periodisedValuesLookupProvider = new Mock<IPeriodisedValuesLookupProviderService>();
            periodisedValuesLookupProvider.Setup(p => p.ProvideAsync(reportServiceContextMock.Object, cancellationToken)).ReturnsAsync(new PeriodisedValuesLookup());

            var referenceDataServiceMock = new Mock<IReferenceDataService>();
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            var persistReportDataMock = new Mock<IPersistReportData>();

            referenceDataServiceMock
                .Setup(m => m.GetLatestIlrSubmissionFileNameAsync(10005788, It.IsAny<CancellationToken>()))
                .ReturnsAsync("10005788/ILR-10005788-1920-20190801-000001-01.xml");

            var fundingSummaryReportModelBuilder = new FundingSummaryReportModelBuilder(referenceDataServiceMock.Object, dateTimeProviderMock.Object);

            var submissionDateTime = new DateTime(2019, 3, 1);

            reportServiceContextMock.Setup(c => c.Ukprn).Returns(10005788);
            reportServiceContextMock.Setup(c => c.Container).Returns(container);
            reportServiceContextMock.Setup(c => c.SubmissionDateTimeUtc).Returns(submissionDateTime);

            var excelService = new ExcelService(new FileServiceStub());

            dateTimeProviderMock.Setup(p => p.ConvertUtcToUk(submissionDateTime)).Returns(submissionDateTime);

            var fundingSummaryReportRenderService = new FundingSummaryReportRenderService();

            var report = NewReport(
                Mock.Of<ILogger>(),
                Mock.Of<IStreamableKeyValuePersistenceService>(),
                dateTimeProviderMock.Object,
                fundingSummaryReportModelBuilder,
                excelService,
                fundingSummaryReportRenderService,
                periodisedValuesLookupProvider.Object,
                fcsService.Object,
                persistReportDataMock.Object);

            excelService.ApplyLicense();

            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update))
                {
                    await report.GenerateReport(reportServiceContextMock.Object, zipArchive, cancellationToken);
                }
            }
        }

        private FundingSummaryReport NewReport(
            ILogger logger = null,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService = null,
            IDateTimeProvider dateTimeProvider = null,
            IFundingSummaryReportModelBuilder fundingSummaryReportModelBuilder = null,
            IExcelService excelService = null,
            IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService = null,
            IPeriodisedValuesLookupProviderService periodisedValuesLookupProviderService = null,
            IFCSProviderService fcsProviderService = null,
            IPersistReportData persisteReportData = null)
        {
            return new FundingSummaryReport(
                logger,
                streamableKeyValuePersistenceService,
                dateTimeProvider,
                fundingSummaryReportModelBuilder,
                excelService,
                fundingSummaryReportRenderService,
                periodisedValuesLookupProviderService,
                fcsProviderService,
                persisteReportData);
        }
    }
}
