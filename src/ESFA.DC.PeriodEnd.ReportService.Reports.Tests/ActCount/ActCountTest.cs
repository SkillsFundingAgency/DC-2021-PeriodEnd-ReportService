using System;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.ActCount
{
    public class ActCountTest
    {
        [Fact]
        public async Task GenerateReport()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var ukprn = 123456;
            var fileName = "FileName";
            var container = "Container";

            var rows = Array.Empty<ActCountModel>();
            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);

            var csvFileServiceMock = new Mock<ICsvFileService>();
            var fileNameServiceMock = new Mock<IFileNameService>();

            fileNameServiceMock.Setup(s => s.GetInternalFilename(reportServiceContext.Object, "ACT Count Report", OutputTypes.Csv, true, true)).Returns(fileName);

            var modelBuilderMock = new Mock<IActCountModelBuilder>();

            modelBuilderMock.Setup(b => b.BuildAsync(CancellationToken.None)).ReturnsAsync(rows);

            var report = NewReport(csvFileServiceMock.Object, fileNameServiceMock.Object, modelBuilderMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            csvFileServiceMock.Verify(s => s.WriteAsync<ActCountModel, ActCountClassMap>(rows, fileName, container, cancellationToken, null, null));
        }

        private Reports.ActCount.ActCount NewReport(
            ICsvFileService csvFileService = null,
            IFileNameService fileNameService = null,
            IActCountModelBuilder actCountModelBuilder = null)
        {
            return new Reports.ActCount.ActCount(
                fileNameService ?? Mock.Of<IFileNameService>(),
                csvFileService ?? Mock.Of<ICsvFileService>(),
                actCountModelBuilder ?? Mock.Of<IActCountModelBuilder>());
        }
    }
}
