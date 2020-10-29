using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.ReportData.Model;
using System;
using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.Serialization.Interfaces;
using System.IO;
using ESFA.DC.FileService.Interface;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView
{
    public class UYPSummaryView : IReport
    {
        public bool IncludeInZip => false;

        private const string ReportZipFileName = "{0}-LLVSample";

        private readonly ICsvFileService _csvFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IUYPSummaryViewDataProvider _uypSummaryViewDataProvider;
        private readonly IUYPSummaryViewModelBuilder _uypSummaryViewModelBuilder;
        private readonly ILogger _logger;
        private readonly IReportDataPersistanceService<LearnerLevelViewReport> _reportDataPersistanceService;
        private readonly IUYPSummaryViewPersistenceMapper _uypSummaryViewPersistenceMapper;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IFileService _fileService;
        private readonly IReportZipService _reportZipService;
        private readonly UTF8Encoding _encoding = new UTF8Encoding(true, true);

        public string ReportTaskName => "TaskLearnerLevelViewReport";
        
        private string BaseReportFileName => "Learner Level View Report";

        public UYPSummaryView(
            ICsvFileService csvFileService,
            IFileNameService fileNameService,
            IUYPSummaryViewDataProvider uypSummaryViewDataProvider,
            IUYPSummaryViewModelBuilder uypSummaryViewModelBuilder,
            IJsonSerializationService jsonSerializationService,
            IFileService fileService,
            IReportZipService reportZipService,
            IReportDataPersistanceService<LearnerLevelViewReport> reportDataPersistanceService,
            IUYPSummaryViewPersistenceMapper uypSummaryViewPersistenceMapper,
            ILogger logger)
        {
            _csvFileService = csvFileService;
            _fileNameService = fileNameService;
            _uypSummaryViewDataProvider = uypSummaryViewDataProvider;
            _uypSummaryViewModelBuilder = uypSummaryViewModelBuilder;
            _jsonSerializationService = jsonSerializationService;
            _fileService = fileService;
            _reportZipService = reportZipService;
            _reportDataPersistanceService = reportDataPersistanceService;
            _uypSummaryViewPersistenceMapper = uypSummaryViewPersistenceMapper;
            _logger = logger;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var ukprn = reportServiceContext.Ukprn;
            var collectionYear = reportServiceContext.CollectionYear;

            var baseFileName = _fileNameService.GetFilename(reportServiceContext, BaseReportFileName, OutputTypes.Csv, true, true);
            var downloadFilename = _fileNameService.GetFilename(reportServiceContext, BaseReportFileName + " Download", OutputTypes.Csv, false, true);
            var summaryFilename = _fileNameService.GetFilename(reportServiceContext, BaseReportFileName + " Summary", OutputTypes.Json, false, true);

            _logger.LogInfo("UYP Summary Report Data Provider Start");

            var dasPaymentsTask = _uypSummaryViewDataProvider.GetDASPaymentsAsync(ukprn, collectionYear, cancellationToken);
            var ilrLearnerInfoTask = _uypSummaryViewDataProvider.GetILRLearnerInfoAsync(ukprn, cancellationToken);
            var learnerDeliveryEarningsTask = _uypSummaryViewDataProvider.GetLearnerDeliveryEarningsAsync(ukprn, cancellationToken);
            var priceEpisodeEarningsTask = _uypSummaryViewDataProvider.GetPriceEpisodeEarningsAsync(ukprn, cancellationToken);
            var coInvestmentsTask = _uypSummaryViewDataProvider.GetCoinvestmentsAsync(ukprn, cancellationToken);
            var dataLockTask = _uypSummaryViewDataProvider.GetDASDataLockAsync(ukprn, collectionYear, cancellationToken);
            var hbcTask = _uypSummaryViewDataProvider.GetHBCPInfoAsync(ukprn, collectionYear, cancellationToken);

            await Task.WhenAll(dasPaymentsTask, ilrLearnerInfoTask, learnerDeliveryEarningsTask, priceEpisodeEarningsTask, coInvestmentsTask, dataLockTask, hbcTask);

            // Get the employer name information
            var apprenticeshipIds = dasPaymentsTask.Result.Select(p => p.ApprenticeshipId);
            var legalEntityNameDictionary = await _uypSummaryViewDataProvider.GetLegalEntityNameAsync(ukprn, apprenticeshipIds, cancellationToken);

            _logger.LogInfo("UYP Summary Report Data Provider End");

            _logger.LogInfo("UYP Summary Report Model Build Start");

            var uypSummaryViewRecords = _uypSummaryViewModelBuilder.Build(
                dasPaymentsTask.Result,
                ilrLearnerInfoTask.Result,
                learnerDeliveryEarningsTask.Result,
                priceEpisodeEarningsTask.Result,
                coInvestmentsTask.Result,
                dataLockTask.Result,
                hbcTask.Result,
                legalEntityNameDictionary,
                reportServiceContext.ReturnPeriod, 
                ukprn).ToList();

            _logger.LogInfo("UYP Summary Report Model Build End");

            // Full data set used for summary and data persist
            await _csvFileService.WriteAsync<LearnerLevelViewModel, UYPSummaryViewClassMap>(uypSummaryViewRecords, baseFileName, reportServiceContext.Container, cancellationToken);
            string summaryFile = CreateSummary(uypSummaryViewRecords, cancellationToken);
            await WriteAsync(summaryFilename, summaryFile, reportServiceContext.Container, cancellationToken);

            // Persist data
            var persistModels = _uypSummaryViewPersistenceMapper.Map(reportServiceContext, uypSummaryViewRecords, cancellationToken);
            await _reportDataPersistanceService.PersistAsync(reportServiceContext, persistModels, cancellationToken);

            // Only learners with issues are made available for the provider to download as a report
            var uypSummaryViewRecordsWithIssues = uypSummaryViewRecords.Where(p => p.IssuesAmount < 0);
            await _csvFileService.WriteAsync<LearnerLevelViewModel, UYPSummaryViewDownloadClassMap>(uypSummaryViewRecordsWithIssues, downloadFilename, reportServiceContext.Container, cancellationToken);

            if (SampleProviders.SampleReportProviderUkPrns.Contains(ukprn))
            {
                var zipName = string.Format(ReportZipFileName, ukprn);
                await _reportZipService.CreateOrUpdateZipWithReportAsync(zipName, baseFileName, reportServiceContext, cancellationToken);
                await _reportZipService.CreateOrUpdateZipWithReportAsync(zipName, summaryFilename, reportServiceContext, cancellationToken);
                await _reportZipService.CreateOrUpdateZipWithReportAsync(zipName, downloadFilename, reportServiceContext, cancellationToken);
            }

            return baseFileName;
        }

        private async Task WriteAsync(string fileName, string csvData, string container, CancellationToken cancellationToken)
        {
            using (Stream stream = await _fileService.OpenWriteStreamAsync(fileName, container, cancellationToken))
            {
                using (TextWriter textWriter = new StreamWriter(stream, _encoding))
                {
                    textWriter.Write(csvData);
                }
            }
        }

        private string CreateSummary(List<LearnerLevelViewModel> learnerLevelView, CancellationToken cancellationToken)
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
                var totalCostofClawbackForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == LearnerLevelViewConstants.ReasonForIssues_Clawback);
                var totalCostOfHBCPForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == LearnerLevelViewConstants.ReasonForIssues_CompletionHoldbackPayment);
                var totalCostofOthersForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues == LearnerLevelViewConstants.ReasonForIssues_Other);
                var totalCostOfDataLocksForThisPeriod = learnerLevelView.Where(p => p.ReasonForIssues != null && p.ReasonForIssues.Contains(LearnerLevelViewConstants.DLockErrorRuleNamePrefix));
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
                throw;
            }
        }

        private string GetJson(
                IEnumerable<LearnerLevelViewSummaryModel> learnerLeveViewModel,
                CancellationToken cancellationToken)
        {
            return _jsonSerializationService.Serialize<IEnumerable<LearnerLevelViewSummaryModel>>(learnerLeveViewModel);
        }
    }
}
