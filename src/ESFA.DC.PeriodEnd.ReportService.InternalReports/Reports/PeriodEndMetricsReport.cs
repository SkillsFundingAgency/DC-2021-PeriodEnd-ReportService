using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.InternalReports.Mappers;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.PeriodEndMetrics;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public class PeriodEndMetricsReport : AbstractInternalReport, IInternalReport
    {
        private const string TemplateName = "PeriodEndChecks-Template-v2.xlsx";
        private const string EarningsPaymentsTabName = "Earnings vs Payments";
        private const int TemplateCellRow = 2;

        private readonly ILogger _logger;
        private readonly IPaymentsService _paymentsService;
        private readonly IPeriodEndQueryService1920 _ilrPeriodEndService;
        private readonly IStreamableKeyValuePersistenceService _persistenceService;

        public PeriodEndMetricsReport(
            ILogger logger,
            IPaymentsService paymentsService,
            IPeriodEndQueryService1920 ilrPeriodEndService,
            IStreamableKeyValuePersistenceService persistenceService,
            IValueProvider valueProvider,
            IDateTimeProvider dateTimeProvider)
        : base(valueProvider, dateTimeProvider)
        {
            _logger = logger;
            _paymentsService = paymentsService;
            _ilrPeriodEndService = ilrPeriodEndService;
            _persistenceService = persistenceService;
        }

        public override string ReportFileName { get; set; } = "Period End Metrics";

        public override string ReportTaskName => ReportTaskNameConstants.InternalReports.PeriodEndMetricsReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"In {ReportFileName} report.");

            ReportFileName = $"{ReportFileName} R{reportServiceContext.ReturnPeriod.ToString().PadLeft(2, '0')}";

            var externalFileName = GetFilename(reportServiceContext);

            var workbook = GetWorkbookFromTemplate(TemplateName);
            var worksheet = workbook.Worksheets[EarningsPaymentsTabName];

            var payments = await GetPaymentMetrics(reportServiceContext.CollectionYear, reportServiceContext.ReturnPeriod);
            var earnings = await GetEarningsMetrics(reportServiceContext.ReturnPeriod);

            SetCurrentRow(worksheet, TemplateCellRow);

            foreach (var payment in payments)
            {
                WriteExcelRecords(worksheet, new PeriodEndMetricsPaymentsMapper(), payment, null);
            }

            SetCurrentRow(worksheet, TemplateCellRow);

            foreach (var earning in earnings)
            {
                WriteExcelRecords(worksheet, new PeriodEndMetricsEarningsMapper(), earning, null);
            }

            using (var ms = new MemoryStream())
            {
                workbook.Save(ms, SaveFormat.Xlsx);
                await _persistenceService.SaveAsync($"{externalFileName}.xlsx", ms, cancellationToken);
            }
        }

        private async Task<IEnumerable<IlrMetrics>> GetEarningsMetrics(int period)
        {
            return await _ilrPeriodEndService.GetPeriodEndMetrics(period);
        }

        private async Task<IEnumerable<PaymentMetrics>> GetPaymentMetrics(int collectionYear, int period)
        {
            return await _paymentsService.GetPaymentMetrics(collectionYear, period);
        }
    }
}