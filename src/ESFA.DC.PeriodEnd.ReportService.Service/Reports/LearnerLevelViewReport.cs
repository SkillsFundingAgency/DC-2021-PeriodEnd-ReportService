using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.DataPersist;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Mapper;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports
{
    public class LearnerLevelViewReport : AbstractReport
    {
        public const string ReasonForIssues_CompletionHoldbackPayment = "Completion Holdback";
        public const string ReasonForIssues_Clawback = "Clawback";
        public const string ReasonForIssues_Other = "Other Issue";

        private readonly UTF8Encoding _encoding = new UTF8Encoding(true, true);

        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IFM36PeriodEndProviderService _fm36ProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly ILearnerLevelViewModelBuilder _modelBuilder;
        private readonly IFileService _fileService;
        private readonly IPersistReportData _persistReportData;

        public LearnerLevelViewReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IFM36PeriodEndProviderService fm36ProviderService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IJsonSerializationService jsonSerializationService,
            ILearnerLevelViewModelBuilder modelBuilder,
            IFileService fileService,
            IPersistReportData persistReportData)
        : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _jsonSerializationService = jsonSerializationService;
            _fileService = fileService;
            _persistReportData = persistReportData;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName => "Learner Level View Report";

        public override string ReportTaskName => ReportTaskNameConstants.LearnerLevelViewReport;

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            var externalFileName = GetCustomFilename(reportServiceContext, $"{dateTime:yyyyMMdd-HHmmss}");
            var summaryFileName = GetCustomFilename(reportServiceContext, "Summary");
            var fileName = GetCustomFilename(reportServiceContext, "Download");

            // get the main base DAS payments data
            var appsMonthlyPaymentDasInfo =
                await _dasPaymentsProviderService.GetPaymentsInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // get the ILR data
            var appsMonthlyPaymentIlrInfo =
                await _ilrPeriodEndProviderService.GetILRInfoForAppsMonthlyPaymentReportAsync(
                    reportServiceContext.Ukprn, cancellationToken);

            // Get the earning data
            var learnerLevelViewFM36Info = await _fm36ProviderService.GetFM36DataForLearnerLevelView(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Get the employer investment info
            var appsCoInvestmentIlrInfo = await _ilrPeriodEndProviderService.GetILRInfoForAppsCoInvestmentReportAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Get the datalock information
            var learnerLevelDatalockInfo = await _dasPaymentsProviderService.GetDASDataLockInfoAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            // Get the HBCP information
            var learnerLevelHBCPInfo = await _dasPaymentsProviderService.GetHBCPInfoAsync(
                    reportServiceContext.Ukprn,
                    cancellationToken);

            var paymentsDictionary = BuildPaymentInfoDictionary(appsMonthlyPaymentDasInfo);
            var aECPriceEpisodeDictionary = BuildAECPriceEpisodeDictionary(learnerLevelViewFM36Info?.AECApprenticeshipPriceEpisodePeriodisedValues);
            var aECLearningDeliveryDictionary = BuildAECLearningDeliveryDictionary(learnerLevelViewFM36Info?.AECLearningDeliveryPeriodisedValuesInfo);

            // Get the employer name information
            var apprenticeshipIds = appsMonthlyPaymentDasInfo.Payments.Select(p => p.ApprenticeshipId);
            var apprenticeshipIdLegalEntityNameDictionary = await _dasPaymentsProviderService.GetLegalEntityNameApprenticeshipIdDictionaryAsync(apprenticeshipIds, cancellationToken);

            // Build the Learner level view Report
            var learnerLevelViewModel = _modelBuilder.BuildLearnerLevelViewModelList(
                reportServiceContext.Ukprn,
                appsMonthlyPaymentIlrInfo,
                appsCoInvestmentIlrInfo,
                learnerLevelDatalockInfo,
                learnerLevelHBCPInfo,
                learnerLevelViewFM36Info,
                paymentsDictionary,
                aECPriceEpisodeDictionary,
                aECLearningDeliveryDictionary,
                apprenticeshipIdLegalEntityNameDictionary,
                reportServiceContext.ReturnPeriod).ToList();

            // Write the full file containing calculated data
            string learnerLevelViewCSV = await GetLearnerLevelViewCsv(learnerLevelViewModel, cancellationToken);
            await WriteAsync($"{externalFileName}.csv", learnerLevelViewCSV, reportServiceContext.Container, cancellationToken);

            // Write the abridged report file downloadable by the user
            string learnerLevelFinancialsRemovedCSV = await GetLearnerLevelFinancialsRemovedViewCsv(learnerLevelViewModel, cancellationToken);
            await WriteAsync($"{fileName}.csv", learnerLevelFinancialsRemovedCSV, reportServiceContext.Container, cancellationToken);

            // Create the summary file which will be used by the WebUI to display the summary view
            string summaryFile = CreateSummary(learnerLevelViewModel, cancellationToken);
            await WriteAsync($"{summaryFileName}.json", summaryFile, reportServiceContext.Container, cancellationToken);

            if (reportServiceContext.DataPersistFeatureEnabled)
            {
                Stopwatch stopWatchLog = new Stopwatch();
                stopWatchLog.Start();
                await _persistReportData.PersistReportDataAsync(
                    learnerLevelViewModel,
                    reportServiceContext.Ukprn,
                    reportServiceContext.ReturnPeriod,
                    TableNameConstants.LearnerLevelViewReport,
                    reportServiceContext.ReportDataConnectionString,
                    cancellationToken);
                _logger.LogDebug($"Performance-Learner Level View Report logging took - {stopWatchLog.ElapsedMilliseconds} ms ");
                stopWatchLog.Stop();
            }
            else
            {
                _logger.LogDebug(" Data Persist Feature is disabled.");
            }
        }

        public async Task WriteAsync(string fileName, string csvData, string container, CancellationToken cancellationToken)
        {
            using (Stream stream = await _fileService.OpenWriteStreamAsync(fileName, container, cancellationToken))
            {
                using (TextWriter textWriter = new StreamWriter(stream, _encoding))
                {
                    textWriter.Write(csvData);
                }
            }
        }

        public IDictionary<LearnerLevelViewPaymentsKey, List<AppsMonthlyPaymentDasPaymentModel>> BuildPaymentInfoDictionary(AppsMonthlyPaymentDASInfo paymentsInfo)
        {
            return paymentsInfo
                .Payments
                .GroupBy(
                p => new LearnerLevelViewPaymentsKey(p.LearnerReferenceNumber, p.ReportingAimFundingLineType), new LLVPaymentRecordKeyEqualityComparer())
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        public IDictionary<string, List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> BuildAECPriceEpisodeDictionary(List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aECPriceEpisodeInfo)
        {
            return aECPriceEpisodeInfo
                .GroupBy(p => p.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        public IDictionary<string, List<AECLearningDeliveryPeriodisedValuesInfo>> BuildAECLearningDeliveryDictionary(List<AECLearningDeliveryPeriodisedValuesInfo> aECLearningDeliveryInfo)
        {
            return aECLearningDeliveryInfo
                .GroupBy(p => p.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        private string GetJson(
                        IEnumerable<LearnerLevelViewSummaryModel> learnerLeveViewModel,
                        CancellationToken cancellationToken)
        {
            return _jsonSerializationService.Serialize<IEnumerable<LearnerLevelViewSummaryModel>>(learnerLeveViewModel);
        }

        private string CreateSummary(IReadOnlyList<LearnerLevelViewModel> learnerLevelView, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Drop out if there is no data to report on.
            if (learnerLevelView == null)
            {
                return null;
            }

            try
            {
                // Calc the groups of learners first to avoid doing two linq queries
                var totalCostofClawbackForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_Clawback);
                var totalCostOfHBCPForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_CompletionHoldbackPayment);
                var totalCostofOthersForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_Other);
                var totalCostOfDataLocksForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues != null && p.ReasonForIssues.Contains(Generics.DLockErrorRuleNamePrefix));
                var earningsReleased = learnerLevelView.Where(r => (r.ESFAPlannedPaymentsThisPeriod + r.CoInvestmentPaymentsToCollectThisPeriod) > r.TotalEarningsForPeriod);

                // Create the model to be saved to the CSV file.
                var learnerLevelViewSummaryModels = new List<LearnerLevelViewSummaryModel>();
                var learnerLevelViewSummary = new LearnerLevelViewSummaryModel()
                {
                    Ukprn = learnerLevelView.FirstOrDefault()?.Ukprn,
                    NumberofLearners = learnerLevelView.Count(),
                    TotalCostofClawbackForThisPeriod = totalCostofClawbackForThisPeriod.Sum(r => r.IssuesAmount),
                    NumberofClawbacks = totalCostofClawbackForThisPeriod.Count(),
                    TotalCostOfHBCPForThisPeriod = totalCostOfHBCPForThisPeriod.Sum(r => r.IssuesAmount),
                    NumberofHBCP = totalCostOfHBCPForThisPeriod.Count(),
                    TotalCostofOthersForThisPeriod = totalCostofOthersForThisPeriod.Sum(r => r.IssuesAmount),
                    NumberofOthers = totalCostofOthersForThisPeriod.Count(),
                    TotalCostOfDataLocksForThisPeriod = totalCostOfDataLocksForThisPeriod.Sum(r => r.IssuesAmount),
                    NumberofDatalocks = totalCostOfDataLocksForThisPeriod.Count(),
                    ESFAPlannedPaymentsForThisPeriod = learnerLevelView.Sum(r => r.ESFAPlannedPaymentsThisPeriod),
                    TotalEarningsForThisPeriod = learnerLevelView.Sum(r => r.TotalEarningsForPeriod),
                    CoInvestmentPaymentsToCollectForThisPeriod = learnerLevelView.Sum(r => r.CoInvestmentPaymentsToCollectThisPeriod),
                    NumberofCoInvestmentsToCollect = learnerLevelView.Count(r => r.CoInvestmentPaymentsToCollectThisPeriod != 0),
                    TotalCoInvestmentCollectedToDate = learnerLevelView.Sum(r => r.TotalCoInvestmentCollectedToDate),
                    TotalEarningsToDate = learnerLevelView.Sum(r => r.TotalEarningsToDate),
                    TotalPaymentsToDate = learnerLevelView.Sum(r => r.PlannedPaymentsToYouToDate),
                    EarningsReleased = earningsReleased.Sum(q => q.ESFAPlannedPaymentsThisPeriod + q.CoInvestmentPaymentsToCollectThisPeriod - q.TotalEarningsForPeriod),
                    NumberofEarningsReleased = earningsReleased.Count()
                };

                learnerLevelViewSummaryModels.Add(learnerLevelViewSummary);

                return GetJson(learnerLevelViewSummaryModels, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Build Summary Json file", ex);
                throw ex;
            }
        }

        private async Task<string> GetLearnerLevelViewCsv(IReadOnlyList<LearnerLevelViewModel> learnerLevelViewModel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<LearnerLevelViewMapper, LearnerLevelViewModel>(csvWriter, learnerLevelViewModel);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private async Task<string> GetLearnerLevelFinancialsRemovedViewCsv(IReadOnlyList<LearnerLevelViewModel> learnerLevelViewModel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<LearnerLevelViewModel> issuesOnlyLearners = learnerLevelViewModel.Where(p => p.IssuesAmount < Generics.MinimumIssuesAmount || p.ReasonForIssues != string.Empty);

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<LearnerLevelViewFinancialsRemovedMapper, LearnerLevelViewModel>(csvWriter, issuesOnlyLearners);

                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
