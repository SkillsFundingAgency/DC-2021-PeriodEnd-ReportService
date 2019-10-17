using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Interface;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Providers;
using ESFA.DC.PeriodEnd.ReportService.Service.Tests.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Reports
{
    public class TestFundingSummaryReport
    {
        //[Fact]
        //public async Task GenerateAsync()
        //{
        //    var container = "Container";

        //    var fundingSummaryReportModelBuilderMock = new Mock<IModelBuilder<IFundingSummaryReport>>();

        //    var reportServiceContextMock = new Mock<IReportServiceContext>();

        //    reportServiceContextMock.Setup(c => c.Container).Returns(container);

        //    var reportServiceDependentData = Mock.Of<IReportServiceDependentData>();
        //    var fundingSummaryReportModel = Mock.Of<IFundingSummaryReport>();

        //    fundingSummaryReportModelBuilderMock.Setup(b => b.Build(reportServiceContextMock.Object, reportServiceDependentData)).Returns(fundingSummaryReportModel);

        //    Workbook workbook = null;
        //    Worksheet worksheet = null;

        //    var excelServiceMock = new Mock<IExcelService>();

        //    excelServiceMock.Setup(s => s.NewWorkbook()).Returns(workbook);
        //    excelServiceMock.Setup(s => s.GetWorksheetFromWorkbook(workbook, 0)).Returns(worksheet);

        //    var fileNameServiceMock = new Mock<IFileNameService>();

        //    var fileName = "FileName";
        //    fileNameServiceMock.Setup(s => s.GetFilename(reportServiceContextMock.Object, "Funding Summary Report", OutputTypes.Excel, true)).Returns(fileName);

        //    var fundingSummaryReportRenderServiceMock = new Mock<IRenderService<IFundingSummaryReport>>();

        //    var report = NewReport(fileNameServiceMock.Object, fundingSummaryReportModelBuilderMock.Object, excelServiceMock.Object, fundingSummaryReportRenderServiceMock.Object);

        //    var cancellationToken = CancellationToken.None;

        //    await report.GenerateAsync(reportServiceContextMock.Object, reportServiceDependentData, cancellationToken);

        //    excelServiceMock.Verify(s => s.SaveWorkbookAsync(workbook, fileName, container, cancellationToken));
        //    fundingSummaryReportRenderServiceMock.Verify(s => s.Render(fundingSummaryReportModel, worksheet));
        //}

        [Fact]
        public async Task SystemTest()
        {
            var container = "Output";
            var cancellationToken = CancellationToken.None;
            var ukprn = 10005788;

            var fcsService = new Mock<IFCSProviderService>();
            fcsService.Setup(f => f.GetContractAllocationNumberFSPCodeLookupAsync(ukprn, cancellationToken)).ReturnsAsync(new Dictionary<string, string>());

            var reportServiceContextMock = new Mock<IReportServiceContext>();
            var periodisedValuesLookupProvider = //PeriodisedValueLookupProviderTests.NewService(); new Mock<IPeriodisedValuesLookupProviderService>();
                new Mock<IPeriodisedValuesLookupProviderService>();
            periodisedValuesLookupProvider.Setup(p => p.ProvideAsync(reportServiceContextMock.Object, cancellationToken)).ReturnsAsync(new PeriodisedValuesLookup());

            var fundingSummaryReportModelBuilder = new FundingSummaryReportModelBuilder();

            var submissionDateTime = new DateTime(2019, 3, 1);

            reportServiceContextMock.Setup(c => c.Ukprn).Returns(10005788);
            reportServiceContextMock.Setup(c => c.Container).Returns(container);
            reportServiceContextMock.Setup(c => c.SubmissionDateTimeUtc).Returns(submissionDateTime);

            var excelService = new ExcelService(new FileServiceStub());

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

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
                fcsService.Object);

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
            IFCSProviderService fcsProviderService = null)
        {
            return new FundingSummaryReport(
                logger,
                streamableKeyValuePersistenceService,
                dateTimeProvider,
                fundingSummaryReportModelBuilder,
                excelService,
                fundingSummaryReportRenderService,
                periodisedValuesLookupProviderService,
                fcsProviderService);
        }
    }
}
