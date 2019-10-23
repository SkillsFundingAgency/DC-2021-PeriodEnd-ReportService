using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract
{
    public abstract class AbstractReport : IReport
    {
        protected readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;
        protected readonly ILogger _logger;

        private readonly IDateTimeProvider _dateTimeProvider;

        protected AbstractReport(IDateTimeProvider dateTimeProvider, IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService, ILogger logger)
        {
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
        }

        public abstract string ReportFileName { get; }

        public abstract string ReportTaskName { get; }

        public string GetFilename(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            return $"R{reportServiceContext.ReturnPeriod:00}_{reportServiceContext.Ukprn}_{reportServiceContext.Ukprn} {ReportFileName} {dateTime:yyyyMMdd-HHmmss}";
        }

        public string GetFilenameForInternalReport(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            return $"{reportServiceContext.ReturnPeriod}_{ReportFileName}";
        }

        public string GetZipFilename(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            return $"{reportServiceContext.Ukprn} {ReportFileName} {dateTime:yyyyMMdd-HHmmss}";
        }

        public abstract Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken);

        public bool IsMatch(string reportTaskName)
        {
            return string.Equals(reportTaskName, ReportTaskName, StringComparison.OrdinalIgnoreCase);
        }

        public virtual void CsvWriterConfiguration(CsvWriter csvWriter)
        {
        }

        protected void WriteCsvRecords<TMapper, TModel>(CsvWriter csvWriter, IEnumerable<TModel> records)
            where TMapper : ClassMap
            where TModel : class
        {
            csvWriter.Configuration.RegisterClassMap<TMapper>();

            csvWriter.Configuration.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "dd/MM/yyyy" };
            csvWriter.Configuration.TypeConverterOptionsCache.GetOptions<DateTime?>().Formats = new[] { "dd/MM/yyyy" };

            CsvWriterConfiguration(csvWriter);

            csvWriter.WriteHeader<TModel>();
            csvWriter.NextRecord();

            if (records != null)
            {
                csvWriter.WriteRecords(records);
            }

            csvWriter.Configuration.UnregisterClassMap();
        }

        /// <summary>
        /// Writes the data to the zip file with the specified filename.
        /// </summary>
        /// <param name="archive">Archive to write to.</param>
        /// <param name="filename">Filename to use in zip file.</param>
        /// <param name="data">Data to write.</param>
        /// <returns>Awaitable task.</returns>
        protected async Task WriteZipEntry(ZipArchive archive, string filename, string data)
        {
            if (archive == null)
            {
                return;
            }

            ZipArchiveEntry entry = archive.GetEntry(filename);
            entry?.Delete();

            ZipArchiveEntry archivedFile = archive.CreateEntry(filename, CompressionLevel.Optimal);
            using (StreamWriter sw = new StreamWriter(archivedFile.Open()))
            {
                await sw.WriteAsync(data);
            }
        }

        protected async Task WriteZipEntry(ZipArchive archive, string filename, Stream stream, CancellationToken cancellationToken)
        {
            if (archive == null)
            {
                return;
            }

            ZipArchiveEntry entry = archive.GetEntry(filename);
            entry?.Delete();

            ZipArchiveEntry archivedFile = archive.CreateEntry(filename, CompressionLevel.Optimal);

            using (var zipStream = archivedFile.Open())
            {
                stream.Position = 0;

                await stream.CopyToAsync(zipStream, 81920, cancellationToken);
            }
        }
    }
}
