using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports.Reports
{
    public class PeriodEndMetricsReport : IInternalReport
    {
        private readonly ILogger _logger;
        private readonly IPaymentsService _paymentsService;
        private readonly IPeriodEndQueryService1920 _ilrPeriodEndService;
        private readonly IStreamableKeyValuePersistenceService _persistenceService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public PeriodEndMetricsReport(
            ILogger logger,
            IPaymentsService paymentsService,
            IPeriodEndQueryService1920 ilrPeriodEndService,
            IStreamableKeyValuePersistenceService persistenceService,
            IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
            _paymentsService = paymentsService;
            _ilrPeriodEndService = ilrPeriodEndService;
            _persistenceService = persistenceService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task GenerateReport(CancellationToken cancellationToken)
        {
        }
    }
}