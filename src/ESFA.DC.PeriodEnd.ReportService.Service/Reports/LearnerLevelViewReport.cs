using System;
using System.Collections.Generic;
using System.Globalization;
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

        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;
        private readonly IFM36PeriodEndProviderService _fm36ProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly ILearnerLevelViewModelBuilder _modelBuilder;

        public LearnerLevelViewReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrPeriodEndProviderService ilrPeriodEndProviderService,
            IFM36PeriodEndProviderService fm36ProviderService,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IJsonSerializationService jsonSerializationService,
            ILearnerLevelViewModelBuilder modelBuilder)
        : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
            _fm36ProviderService = fm36ProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _jsonSerializationService = jsonSerializationService;
            _modelBuilder = modelBuilder;
        }

        public override string ReportFileName => "Learner Level View Report";

        public override string ReportTaskName => ReportTaskNameConstants.LearnerLevelViewReport;

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            var externalFileName = GetFilename(reportServiceContext);
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

            // Build the actual Apps Monthly Payment Report
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
                reportServiceContext.ReturnPeriod);

            // Write the full file containing calculated data
            string learnerLevelViewCSV = await GetLearnerLevelViewCsv(learnerLevelViewModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", learnerLevelViewCSV, cancellationToken);

            // Write the abridged report file downloadable by the user
            string learnerLevelFinancialsRemovedCSV = await GetLearnerLevelFinancialsRemovedViewCsv(learnerLevelViewModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{fileName}.csv", learnerLevelFinancialsRemovedCSV, cancellationToken);

            // Create the summary file which will be used by the WebUI to display the summary view
            string summaryFile = CreateSummary(learnerLevelViewModel, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{summaryFileName}.json", summaryFile, cancellationToken);
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
                // Create the model to be saved to the CSV file.
                var learnerLevelViewSummaryModels = new List<LearnerLevelViewSummaryModel>();
                var learnerLevelViewSummary = new LearnerLevelViewSummaryModel()
                {
                    Ukprn = learnerLevelView.FirstOrDefault()?.Ukprn,
                    NumberofLearners = learnerLevelView.Count(),
                    TotalCostofClawbackForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_Clawback).Sum(r => r.IssuesAmount),
                    NumberofClawbacks = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_Clawback).Count(),
                    TotalCostOfHBCPForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_CompletionHoldbackPayment).Sum(r => r.IssuesAmount),
                    NumberofHBCP = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_CompletionHoldbackPayment).Count(),
                    TotalCostofOthersForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_Other).Sum(r => r.IssuesAmount),
                    NumberofOthers = learnerLevelView.Where(p => p.ReasonForIssues == ReasonForIssues_Other).Count(),
                    TotalCostOfDataLocksForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues != null && p.ReasonForIssues.Contains(Generics.DLockErrorRuleNamePrefix)).Sum(r => r.IssuesAmount),
                    NumberofDatalocks = learnerLevelView.Where(p => p.ReasonForIssues != null && p.ReasonForIssues.Contains(Generics.DLockErrorRuleNamePrefix)).Count(),
                    ESFAPlannedPaymentsForThisPeriod = learnerLevelView.Sum(r => r.ESFAPlannedPaymentsThisPeriod),
                    TotalEarningsForThisPeriod = learnerLevelView.Sum(r => r.TotalEarningsForPeriod),
                    CoInvestmentPaymentsToCollectForThisPeriod = learnerLevelView.Sum(r => r.CoInvestmentPaymentsToCollectThisPeriod),
                    NumberofCoInvestmentsToCollect = learnerLevelView.Count(r => r.CoInvestmentPaymentsToCollectThisPeriod != 0),
                    TotalCoInvestmentCollectedToDate = learnerLevelView.Sum(r => r.TotalCoInvestmentCollectedToDate),
                    TotalEarningsToDate = learnerLevelView.Sum(r => r.TotalEarningsToDate),
                    TotalPaymentsToDate = learnerLevelView.Sum(r => r.PlannedPaymentsToYouToDate)
                };

                learnerLevelViewSummary.TotalPaymentsForThisPeriod = learnerLevelViewSummary.TotalCostofClawbackForThisPeriod +
                                                                        learnerLevelViewSummary.TotalCostOfHBCPForThisPeriod +
                                                                        learnerLevelViewSummary.TotalCostofOthersForThisPeriod +
                                                                        learnerLevelViewSummary.TotalCostOfDataLocksForThisPeriod +
                                                                        learnerLevelViewSummary.ESFAPlannedPaymentsForThisPeriod +
                                                                        learnerLevelViewSummary.CoInvestmentPaymentsToCollectForThisPeriod;
                learnerLevelViewSummary.NumberofEarningsReleased = learnerLevelViewSummary.NumberofLearners;
                learnerLevelViewSummary.EarningsReleased = learnerLevelViewSummary.TotalPaymentsForThisPeriod +
                                                            learnerLevelViewSummary.CoInvestmentPaymentsToCollectForThisPeriod -
                                                            learnerLevelViewSummary.TotalEarningsForThisPeriod;

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
