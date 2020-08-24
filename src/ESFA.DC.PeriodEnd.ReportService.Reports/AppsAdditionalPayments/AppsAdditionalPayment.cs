using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Persistance;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class AppsAdditionalPayment : IReport
    {
        private readonly ICsvFileService _csvFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IAppsAdditionalPaymentsDataProvider _appsAdditionalPaymentsDataProvider;
        private readonly IAppsAdditionalPaymentsModelBuilder _appsAdditionalPaymentsModelBuilder;
        private readonly IReportDataPersistanceService<ReportData.Model.AppsAdditionalPayment> _persistanceService;
        private readonly IAppsAdditionalPaymentPersistanceMapper _appsAdditionalPaymentPersistanceMapper;

        private string ReportFileName => "Apps Additional Payments Report";

        public string ReportTaskName => "TaskGenerateAppsAdditionalPaymentsReport";
        public bool IncludeInZip => true;

        public AppsAdditionalPayment(
            ICsvFileService csvFileService,
            IFileNameService fileNameService,
            IAppsAdditionalPaymentsDataProvider appsAdditionalPaymentsDataProvider,
            IAppsAdditionalPaymentsModelBuilder appsAdditionalPaymentsModelBuilder,
            IReportDataPersistanceService<ReportData.Model.AppsAdditionalPayment> persistanceService,
            IAppsAdditionalPaymentPersistanceMapper appsAdditionalPaymentPersistanceMapper)
        {
            _csvFileService = csvFileService;
            _fileNameService = fileNameService;
            _appsAdditionalPaymentsDataProvider = appsAdditionalPaymentsDataProvider;
            _appsAdditionalPaymentsModelBuilder = appsAdditionalPaymentsModelBuilder;
            _persistanceService = persistanceService;
            _appsAdditionalPaymentPersistanceMapper = appsAdditionalPaymentPersistanceMapper;

        }

        public async Task<string> GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
            {
                var fileName = _fileNameService.GetFilename(reportServiceContext, $"{reportServiceContext.Ukprn} {ReportFileName}", OutputTypes.Csv);

                var paymentsTask = _appsAdditionalPaymentsDataProvider.GetPaymentsAsync(reportServiceContext, cancellationToken);
                var learnersTask = _appsAdditionalPaymentsDataProvider.GetLearnersAsync(reportServiceContext, cancellationToken);
                var contractAllocationsTask = _appsAdditionalPaymentsDataProvider.GetAecLearningDeliveriesAsync(reportServiceContext, cancellationToken);
                var priceEpisodesTask = _appsAdditionalPaymentsDataProvider.GetPriceEpisodesAsync(reportServiceContext, cancellationToken);

                await Task.WhenAll(paymentsTask, learnersTask, contractAllocationsTask, priceEpisodesTask);

                var model = _appsAdditionalPaymentsModelBuilder.Build(
                    paymentsTask.Result,
                    learnersTask.Result,
                    contractAllocationsTask.Result,
                    priceEpisodesTask.Result);

                await _csvFileService.WriteAsync<AppsAdditionalPaymentReportModel, AppsAdditionalPaymentsClassMap>(model, fileName, reportServiceContext.Container, cancellationToken, null, null);

            var persistModels = _appsAdditionalPaymentPersistanceMapper.Map(reportServiceContext, model, cancellationToken);
            await _persistanceService.PersistAsync(reportServiceContext, persistModels, cancellationToken);


            return fileName;
            }
    }
}