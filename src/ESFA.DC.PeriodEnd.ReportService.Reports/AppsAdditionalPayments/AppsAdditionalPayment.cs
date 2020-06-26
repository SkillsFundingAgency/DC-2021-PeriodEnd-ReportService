using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.CsvService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class AppsAdditionalPayment : IReport
    {
        private readonly ICsvFileService _csvFileService;
        private readonly IFileNameService _fileNameService;
        private readonly IAppsAdditionalPaymentsDataProvider _appsAdditionalPaymentsDataProvider;
        private readonly IAppsAdditionalPaymentsModelBuilder _appsAdditionalPaymentsModelBuilder;

        public string ReportTaskName => "TaskGenerateAppsAdditionalPaymentsReport";

        private string ReportFileName => "Apps Additional Payments Report";

        public AppsAdditionalPayment(
            ICsvFileService csvFileService,
            IFileNameService fileNameService,
            IAppsAdditionalPaymentsDataProvider appsAdditionalPaymentsDataProvider,
            IAppsAdditionalPaymentsModelBuilder appsAdditionalPaymentsModelBuilder)
        {
            _csvFileService = csvFileService;
            _fileNameService = fileNameService;
            _appsAdditionalPaymentsDataProvider = appsAdditionalPaymentsDataProvider;
            _appsAdditionalPaymentsModelBuilder = appsAdditionalPaymentsModelBuilder;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
            {
                var fileName = _fileNameService.GetFilename(reportServiceContext, $"{reportServiceContext.Ukprn} {ReportFileName}", OutputTypes.Csv);

                var paymentsTask = _appsAdditionalPaymentsDataProvider.GetPaymentsAsync(reportServiceContext, cancellationToken);
                var learnersTask = _appsAdditionalPaymentsDataProvider.GetLearnersAsync(reportServiceContext, cancellationToken);
                var contractAllocationsTask = _appsAdditionalPaymentsDataProvider.GetAecLearningDeliveriesAsync(reportServiceContext, cancellationToken);
                var priceEpisodesTask = _appsAdditionalPaymentsDataProvider.GetPriceEpisodesAsync(reportServiceContext, cancellationToken);

                await Task.WhenAll(paymentsTask, learnersTask, contractAllocationsTask, priceEpisodesTask);

                var models = _appsAdditionalPaymentsModelBuilder.Build(
                    paymentsTask.Result,
                    learnersTask.Result,
                    contractAllocationsTask.Result,
                    priceEpisodesTask.Result);

                await _csvFileService.WriteAsync<AppsAdditionalPaymentRecord, AppsAdditionalPaymentsClassMap>(models, fileName, reportServiceContext.Container, cancellationToken, null, null);
        }
    }
}