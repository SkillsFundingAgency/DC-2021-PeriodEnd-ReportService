using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.ProviderSubmission
{
    public class ProviderSubmissionTests
    {
        [Fact]
        public async Task GenerateReport()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var ukprn = 123456;
            var fileName = "FileName";
            var container = "Container";

            var rows = Array.Empty<ProviderSubmissionModel>();
            var reportServiceContext = new Mock<IReportServiceContext>();

            var workbook = new Workbook();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);

            var fileNameServiceMock = new Mock<IFileNameService>();
            var excelFileServiceMock = new Mock<IExcelFileService>();
            var dataProviderMock = new Mock<IProviderSubmissionsDataProvider>();
            var renderServiceMock = new Mock<IProviderSubmissionsRenderService>();

            fileNameServiceMock.Setup(s => s.GetInternalFilename(reportServiceContext.Object, "ILR Provider Submissions Report", OutputTypes.Excel, true, true)).Returns(fileName);

            var modelBuilderMock = new Mock<IProviderSubmissionsModelBuilder>();

            modelBuilderMock.Setup(b => b.Build(It.IsAny<ProviderSubmissionsReferenceData>())).Returns(rows);
            excelFileServiceMock.Setup(s => s.GetWorkbookFromTemplate(It.IsAny<Stream>())).Returns(workbook);

            var report = NewReport(fileNameServiceMock.Object, excelFileServiceMock.Object, dataProviderMock.Object, modelBuilderMock.Object, renderServiceMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            excelFileServiceMock.Verify(s => s.SaveWorkbookAsync(workbook, fileName, container, cancellationToken));
        }

        private Reports.ProviderSubmissions.ProviderSubmission NewReport(
            IFileNameService fileNameService = null,
            IExcelFileService excelFileService = null,
            IProviderSubmissionsDataProvider dataProvider = null,
            IProviderSubmissionsModelBuilder modelBuilder = null,
            IProviderSubmissionsRenderService renderService = null
        )
        {
            return new ProviderSubmissions.ProviderSubmission(
                    fileNameService ?? Mock.Of<IFileNameService>(),
                    excelFileService ?? Mock.Of<IExcelFileService>(),
                    dataProvider ?? Mock.Of<IProviderSubmissionsDataProvider>(),
                    modelBuilder ?? Mock.Of<IProviderSubmissionsModelBuilder>(),
                    renderService ?? Mock.Of<IProviderSubmissionsRenderService>()
                );
        }
    }
}
