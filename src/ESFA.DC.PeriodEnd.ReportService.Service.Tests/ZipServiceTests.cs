using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Context;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Service.Service;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests
{
    public class ZipServiceTests
    {
        [Fact]
        public async Task AddReportToArchiveAsync()
        {
            var fileName = "FileName";
            var zipFileName = "ZipFileName";
            var container = "Container";

            using (var reportStream = new MemoryStream())
            {
                var cancellationToken = CancellationToken.None;

                var fileServiceMock = new Mock<IFileService>();
                var reportServiceContextMock = new Mock<IReportServiceContext>();
                var reportMock = new Mock<IReport>();

                reportServiceContextMock.SetupGet(c => c.Container).Returns(container);

                reportMock.Setup(r => r.GetFilename(reportServiceContextMock.Object)).Returns(fileName);
                reportMock.Setup(r => r.GetZipFilename(reportServiceContextMock.Object)).Returns(zipFileName);

                fileServiceMock.Setup(fs => fs.OpenReadStreamAsync(fileName, container, cancellationToken))
                    .ReturnsAsync(reportStream);

                using (var zipMemoryStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Update, true))
                    {
                        await NewService(fileServiceMock.Object)
                            .AddReportToArchiveAsync(
                                zipArchive,
                                reportMock.Object,
                                reportServiceContextMock.Object,
                                cancellationToken);

                        zipArchive.Entries.Should().Contain(e => e.Name == zipFileName);
                    }
                }
            }
        }

        private ZipService NewService(IFileService fileService = null)
        {
            return new ZipService(fileService);
        }
    }
}
