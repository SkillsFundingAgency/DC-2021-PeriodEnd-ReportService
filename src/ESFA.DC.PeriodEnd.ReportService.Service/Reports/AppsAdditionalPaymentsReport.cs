using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports
{
    public class AppsAdditionalPaymentsReport : AbstractReport, IReport
    {
        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;

        private readonly IAppsAdditionalPaymentsModelBuilder _modelBuilder;

        public AppsAdditionalPaymentsReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService storage,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IFM36ProviderService fm36ProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IAppsAdditionalPaymentsModelBuilder modelBuilder)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService, logger)
        {
            _logger = logger;
            _storage = storage;
            _ilrProviderService = ilrProviderService;
            _fm36ProviderService = fm36ProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName => "Apps Additional Payments Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsAdditionalPaymentsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            //var jobId = reportServiceContext.JobId;
            //var ukPrn = reportServiceContext.Ukprn.ToString();
            //var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            //var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            string csv = await GetCsv(reportServiceContext, cancellationToken);
            await _storage.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);
            Task<FM36Global> fm36Task = _fm36ProviderService.GetFM36Data(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, validLearnersTask, fm36Task);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var ilrError = new List<string>();

            var additionalPaymentsModels = new List<AppsAdditionalPaymentsModel>();
            foreach (string validLearnerRefNum in validLearnersTask.Result)
            {
                var learner = ilrFileTask.Result?.Learners?.SingleOrDefault(x => string.Equals(x.LearnRefNumber, validLearnerRefNum, StringComparison.OrdinalIgnoreCase));

                var fm36Data = fm36Task.Result?.Learners?.SingleOrDefault(x => string.Equals(x.LearnRefNumber, validLearnerRefNum, StringComparison.OrdinalIgnoreCase));

                if (learner == null || fm36Data == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                additionalPaymentsModels.Add(_modelBuilder.BuildModel(learner, fm36Data));
            }

            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating {nameof(AppsAdditionalPaymentsReport)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsAdditionalPaymentsMapper, AppsAdditionalPaymentsModel>(csvWriter, additionalPaymentsModels);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
