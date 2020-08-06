using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.ExcelService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.DataQuality
{
    public class DataQualityTests
    {
        [Fact]
        public async Task GenerateReport()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var ukprn = 123456;
            var fileName = "FileName";
            var container = "Container";

            var rows = new DataQualityProviderModel();
            var reportServiceContext = new Mock<IReportServiceContext>();

            var workbook = new Workbook();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);

            var fileNameServiceMock = new Mock<IFileNameService>();
            var excelFileServiceMock = new Mock<IExcelFileService>();
            var dataProviderMock = new Mock<IDataQualityDataProvider>();
            var renderServiceMock = new Mock<IDataQualityRenderService>();

            fileNameServiceMock.Setup(s => s.GetInternalFilename(reportServiceContext.Object, "Data Quality Report", OutputTypes.Excel, true, true)).Returns(fileName);

            var modelBuilderMock = new Mock<IDataQualityModelBuilder>();

            modelBuilderMock.Setup(b => b.Build(It.IsAny<DataQualityProviderModel>(), reportServiceContext.Object)).Returns(rows);
            excelFileServiceMock.Setup(s => s.GetWorkbookFromTemplate(It.IsAny<Stream>())).Returns(workbook);

            var report = NewReport(fileNameServiceMock.Object, excelFileServiceMock.Object, dataProviderMock.Object, modelBuilderMock.Object, renderServiceMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            excelFileServiceMock.Verify(s => s.SaveWorkbookAsync(workbook, fileName, container, cancellationToken));
        }

        private Reports.DataQuality.DataQuality NewReport(
            IFileNameService fileNameService = null,
            IExcelFileService excelFileService = null,
            IDataQualityDataProvider dataProvider = null,
            IDataQualityModelBuilder modelBuilder = null,
            IDataQualityRenderService renderService = null
        )
        {
            return new Reports.DataQuality.DataQuality(
                    fileNameService ?? Mock.Of<IFileNameService>(),
                    excelFileService ?? Mock.Of<IExcelFileService>(),
                    dataProvider ?? Mock.Of<IDataQualityDataProvider>(),
                    modelBuilder ?? Mock.Of<IDataQualityModelBuilder>(),
                    renderService ?? Mock.Of<IDataQualityRenderService>()
                );
        }
    }
}
