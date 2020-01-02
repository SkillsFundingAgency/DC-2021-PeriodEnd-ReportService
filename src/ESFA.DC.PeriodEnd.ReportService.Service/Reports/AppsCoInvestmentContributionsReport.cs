using System.Collections.Generic;
using System.Diagnostics;
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
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;
using ReportTaskNameConstants = ESFA.DC.PeriodEnd.ReportService.Service.Constants.ReportTaskNameConstants;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports
{
    public sealed class AppsCoInvestmentContributionsReport : AbstractReport, IReport
    {
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IFM36PeriodEndProviderService _fm36PeriodEndProviderService;
        private readonly IAppsCoInvestmentContributionsModelBuilder _modelBuilder;
        private readonly IPersistReportData _persistReportData;

        public AppsCoInvestmentContributionsReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDateTimeProvider dateTimeProvider,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IFM36PeriodEndProviderService fm36PeriodEndProviderService,
            IAppsCoInvestmentContributionsModelBuilder modelBuilder,
            IPersistReportData persistReportData)
        : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _fm36PeriodEndProviderService = fm36PeriodEndProviderService;
            _modelBuilder = modelBuilder;
            _persistReportData = persistReportData;
        }

        public override string ReportFileName => "Apps Co-Investment Contributions Report";

        public override string ReportTaskName => ReportTaskNameConstants.AppsCoInvestmentContributionsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            var appsCoInvestmentIlrInfo = await _ilrPeriodEndProviderService.GetILRInfoForAppsCoInvestmentReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var appsCoInvestmentRulebaseInfo = await _fm36PeriodEndProviderService.GetFM36DataForAppsCoInvestmentReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var appsCoInvestmentPaymentsInfo = await _dasPaymentsProviderService.GetPaymentsInfoForAppsCoInvestmentReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var paymentsAppsCoInvestmentUniqueKeys = await _dasPaymentsProviderService.GetUniqueCombinationsOfKeyFromPaymentsAsync(reportServiceContext.Ukprn, cancellationToken);
            var ilrAppsCoInvestmentUniqueKeys = await _ilrPeriodEndProviderService.GetUniqueAppsCoInvestmentRecordKeysAsync(reportServiceContext.Ukprn, cancellationToken);

            var apprenticeshipIds = appsCoInvestmentPaymentsInfo.Payments.Select(p => p.ApprenticeshipId);

            var apprenticeshipIdLegalEntityNameDictionary = await _dasPaymentsProviderService.GetLegalEntityNameApprenticeshipIdDictionaryAsync(apprenticeshipIds, cancellationToken);

            var appsCoInvestmentContributionsModels = _modelBuilder.BuildModel(appsCoInvestmentIlrInfo, appsCoInvestmentRulebaseInfo, appsCoInvestmentPaymentsInfo, paymentsAppsCoInvestmentUniqueKeys, ilrAppsCoInvestmentUniqueKeys, apprenticeshipIdLegalEntityNameDictionary, reportServiceContext.JobId).ToList();

            string csv = await GetCsv(appsCoInvestmentContributionsModels, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);

            if (reportServiceContext.DataPersistFeatureEnabled)
            {
                Stopwatch stopWatchLog = new Stopwatch();
                stopWatchLog.Start();

                await _persistReportData.PersistReportDataAsync(
                    appsCoInvestmentContributionsModels,
                    reportServiceContext.Ukprn,
                    reportServiceContext.ReturnPeriod,
                    TableNameConstants.AppsCoInvestmentContributions,
                    reportServiceContext.ReportDataConnectionString,
                    cancellationToken);

                _logger.LogDebug($"Performance-AppsCoInvestmentContributionsReport logging took - {stopWatchLog.ElapsedMilliseconds} ms ");
                stopWatchLog.Stop();
            }
            else
            {
                _logger.LogDebug(" Data Persist Feature is disabled.");
            }
        }

        private async Task<string> GetCsv(IEnumerable<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsCoInvestmentContributionsMapper, AppsCoInvestmentContributionsModel>(csvWriter, appsCoInvestmentContributionsModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
