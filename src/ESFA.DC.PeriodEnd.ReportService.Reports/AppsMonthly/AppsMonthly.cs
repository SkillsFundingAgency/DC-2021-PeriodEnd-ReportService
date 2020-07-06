using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Persistence;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthly : IReport
    {
        private readonly ICsvFileService _csvFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IAppsMonthlyPaymentsDataProvider _appsMonthlyPaymentsDataProvider;
        private readonly IAppsMonthlyModelBuilder _appsMonthlyPaymentModelBuilder;
        private readonly ILogger _logger;
        private readonly IReportDataPersistanceService<AppsMonthlyPayment> _reportDataPersistanceService;
        private readonly IAppsMonthlyPersistenceMapper _appsMonthlyPersistenceMapper;
        public string ReportTaskName => "TaskGenerateAppsMonthlyPaymentReport";
        
        private string ReportFileName => "Apps Monthly Payment Report";

        public AppsMonthly(
            ICsvFileService csvFileService,
            IFileNameService fileNameService,
            IAppsMonthlyPaymentsDataProvider appsMonthlyPaymentsDataProvider,
            IAppsMonthlyModelBuilder appsMonthlyPaymentModelBuilder,
            ILogger logger,
            IReportDataPersistanceService<AppsMonthlyPayment> reportDataPersistanceService,
            IAppsMonthlyPersistenceMapper appsMonthlyPersistenceMapper)
        {
            _csvFileService = csvFileService;
            _fileNameService = fileNameService;
            _appsMonthlyPaymentsDataProvider = appsMonthlyPaymentsDataProvider;
            _appsMonthlyPaymentModelBuilder = appsMonthlyPaymentModelBuilder;
            _logger = logger;
            _reportDataPersistanceService = reportDataPersistanceService;
            _appsMonthlyPersistenceMapper = appsMonthlyPersistenceMapper;
        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var ukprn = reportServiceContext.Ukprn;
            var collectionYear = reportServiceContext.CollectionYear;

            var fileName = _fileNameService.GetFilename(reportServiceContext, ReportFileName, OutputTypes.Csv);

            _logger.LogInfo("Apps Monthly Payment Report Data Provider Start");

            var paymentsTask = _appsMonthlyPaymentsDataProvider.GetPaymentsAsync(ukprn, collectionYear, cancellationToken);
            var learnersTask = _appsMonthlyPaymentsDataProvider.GetLearnersAsync(ukprn, cancellationToken);
            var contractAllocationsTask = _appsMonthlyPaymentsDataProvider.GetContractAllocationsAsync(ukprn, cancellationToken);
            var earningsTask = _appsMonthlyPaymentsDataProvider.GetEarningsAsync(ukprn, cancellationToken);
            var priceEpisodesTask = _appsMonthlyPaymentsDataProvider.GetPriceEpisodesAsync(ukprn, cancellationToken);

            await Task.WhenAll(paymentsTask, learnersTask, contractAllocationsTask, earningsTask, priceEpisodesTask);

            var larsLearningDeliveries = await _appsMonthlyPaymentsDataProvider.GetLarsLearningDeliveriesAsync(learnersTask.Result, cancellationToken);

            _logger.LogInfo("Apps Monthly Payment Report Data Provider End");

            _logger.LogInfo("Apps Monthly Payment Report Model Build Start");

            var appsMonthlyRecords = _appsMonthlyPaymentModelBuilder.Build(
                paymentsTask.Result,
                learnersTask.Result,
                contractAllocationsTask.Result,
                earningsTask.Result,
                larsLearningDeliveries,
                priceEpisodesTask.Result).ToList();
            _logger.LogInfo("Apps Monthly Payment Report Model Build End");

            await _csvFileService.WriteAsync<AppsMonthlyRecord, AppsMonthlyClassMap>(appsMonthlyRecords, fileName, reportServiceContext.Container, cancellationToken);

            var persistModels = _appsMonthlyPersistenceMapper.Map(reportServiceContext, appsMonthlyRecords, cancellationToken);
            await _reportDataPersistanceService.PersistAsync(reportServiceContext, persistModels, cancellationToken);

            return fileName;
        }
    }
}
