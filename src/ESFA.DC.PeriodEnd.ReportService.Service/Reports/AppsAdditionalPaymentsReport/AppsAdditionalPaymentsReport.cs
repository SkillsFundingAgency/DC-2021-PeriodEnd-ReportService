using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.AppsAdditionalPaymentsReport
{
    public class AppsAdditionalPaymentsReport : AbstractReport
    {
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IFM36PeriodEndProviderService _fm36ProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IAppsAdditionalPaymentsModelBuilder _modelBuilder;

        public AppsAdditionalPaymentsReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IFM36PeriodEndProviderService fm36ProviderService,
            IDateTimeProvider dateTimeProvider,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IAppsAdditionalPaymentsModelBuilder modelBuilder)
        : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName => "Apps Additional Payments Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsAdditionalPaymentsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            _logger.LogInfo("Apps Additional Payments Start");

            var ilrLearners = _ilrPeriodEndProviderService.GetILRInfoForAppsAdditionalPaymentsReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var rulebaseApprenticeshipPriceEpisodes = _fm36ProviderService.GetApprenticeshipPriceEpisodesForAppsAdditionalPaymentsReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var rulebaseLearningDeliveries = _fm36ProviderService.GetLearningDeliveriesForAppsAdditionalPaymentReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var appsAdditionalPaymentDasPaymentsInfo = _dasPaymentsProviderService.GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(reportServiceContext.Ukprn, cancellationToken);

            await Task.WhenAll(ilrLearners, rulebaseApprenticeshipPriceEpisodes, rulebaseLearningDeliveries, appsAdditionalPaymentDasPaymentsInfo);

            var legalNameDictionary = await _dasPaymentsProviderService.GetLegalEntityNameApprenticeshipIdDictionaryAsync(appsAdditionalPaymentDasPaymentsInfo.Result.Select(p => p.ApprenticeshipId), cancellationToken);

            _logger.LogInfo("Apps Additional Payments Data Provision End");

            var appsAdditionalPaymentsModel = _modelBuilder.BuildModel(
                ilrLearners.Result,
                rulebaseApprenticeshipPriceEpisodes.Result,
                rulebaseLearningDeliveries.Result,
                appsAdditionalPaymentDasPaymentsInfo.Result,
                legalNameDictionary);

            _logger.LogInfo("Apps Additional Payments Report Creation End");

            string csv = await GetCsv(appsAdditionalPaymentsModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);

            _logger.LogInfo("Apps Additional Payments Persistence End");
        }

        private async Task<string> GetCsv(
            IEnumerable<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModel,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsAdditionalPaymentsMapper, AppsAdditionalPaymentsModel>(csvWriter, appsAdditionalPaymentsModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
