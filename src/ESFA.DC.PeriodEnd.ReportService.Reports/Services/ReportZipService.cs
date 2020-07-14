using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Services
{
    public class ReportZipService : IReportZipService
    {
        private readonly IFileNameService _fileNameService;
        private readonly IZipArchiveService _zipArchiveService;
        private readonly IFileService _fileService;

        private const string ReportsZipName = "Reports";
        private const int BufferSize = 8096;

        public ReportZipService(IFileNameService fileNameService, IZipArchiveService zipArchiveService, IFileService fileService)
        {
            _fileNameService = fileNameService;
            _zipArchiveService = zipArchiveService;
            _fileService = fileService;
        }
        
        public async Task CreateZipAsync(string reportFileNameKey, IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var reportZipFileKey = _fileNameService.GetFilename(reportServiceContext, ReportsZipName, OutputTypes.Zip, false, false);

            using (var memoryStream = new MemoryStream())
            using (var zipSteam = await GetStreamAsync(reportZipFileKey, reportServiceContext, cancellationToken))
            {
                await zipSteam.CopyToAsync(memoryStream, BufferSize, cancellationToken);

                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Update, true))
                {
                    using (var readStream = await _fileService.OpenReadStreamAsync(reportFileNameKey, reportServiceContext.Container, cancellationToken))
                    {
                        await _zipArchiveService.AddEntryToZip(zipArchive, readStream, PrepareFileName(reportFileNameKey), cancellationToken);
                    }
                }

                using (var writeStream = await _fileService.OpenWriteStreamAsync(reportZipFileKey, reportServiceContext.Container, cancellationToken))
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(writeStream, BufferSize, cancellationToken);
                }
            }
        }

        private string PrepareFileName(string fileName)
        {
            return fileName.Split('/').Last().Substring(9);
        }

        private async Task<Stream> GetStreamAsync(string reportZipFileKey, IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            if (await _fileService.ExistsAsync(reportZipFileKey, reportServiceContext.Container, cancellationToken))
            {
                return await _fileService.OpenReadStreamAsync(reportZipFileKey, reportServiceContext.Container, cancellationToken);
            }

            return new MemoryStream();
        }
    }
}
