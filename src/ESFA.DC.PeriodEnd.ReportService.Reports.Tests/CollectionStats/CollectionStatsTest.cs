using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.ActCount;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CollectionStats.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.Serialization.Interfaces;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.CollectionStats
{
    public class CollectionStatsTest
    {
        [Fact]
        public async Task GenerateReport()
        {
            var cancellationToken = CancellationToken.None;
            var academicYear = 2021;
            var returnPeriod = 1;
            var ukprn = 123456;
            var fileName = "FileName";
            var container = "Container";

            var rows = Array.Empty<CollectionStatsModel>();
            var reportServiceContext = new Mock<IReportServiceContext>();

            reportServiceContext.SetupGet(c => c.Ukprn).Returns(ukprn);
            reportServiceContext.SetupGet(c => c.CollectionYear).Returns(academicYear);
            reportServiceContext.SetupGet(c => c.Container).Returns(container);
            reportServiceContext.SetupGet(c => c.ReturnPeriod).Returns(returnPeriod);

            var fileNameServiceMock = new Mock<IFileNameService>();
            var fileServiceMock = new Mock<IFileService>();
            var jsonSerializationServiceMock = new Mock<IJsonSerializationService>();

            var streamMock = new Mock<Stream>();

            fileNameServiceMock.Setup(s => s.GetInternalFilename(reportServiceContext.Object, "CollectionStats", OutputTypes.Json, false, false)).Returns(fileName);

            var modelBuilderMock = new Mock<ICollectionStatsModelBuilder>();

            modelBuilderMock.Setup(b => b.BuildAsync(academicYear, 1)).ReturnsAsync(rows);

            fileServiceMock.Setup(s => s.OpenWriteStreamAsync(fileName, container, cancellationToken))
                .ReturnsAsync(streamMock.Object);

            var report = NewReport(fileNameServiceMock.Object, modelBuilderMock.Object, fileServiceMock.Object, jsonSerializationServiceMock.Object);

            await report.GenerateReport(reportServiceContext.Object, cancellationToken);

            fileServiceMock.Verify(s => s.OpenWriteStreamAsync(fileName, container, cancellationToken));
        }

        private Reports.CollectionStats.CollectionStats NewReport(IFileNameService fileNameService = null, ICollectionStatsModelBuilder modelBuilder = null, IFileService fileService = null, IJsonSerializationService jsonSerializationService = null)
        {
            return new Reports.CollectionStats.CollectionStats(
                fileNameService ?? Mock.Of<IFileNameService>(),
                modelBuilder ?? Mock.Of<ICollectionStatsModelBuilder>(),
                fileService ?? Mock.Of<IFileService>(),
                jsonSerializationService ?? Mock.Of<IJsonSerializationService>());
        }
    }
}
