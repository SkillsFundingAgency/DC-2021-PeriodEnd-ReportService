using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports
{
    public class InternalEntryPoint
    {
        private readonly IPeriodEndQueryService1920 _ilr1920MetricsService;
        private readonly IPaymentsService _paymentServices;

        public InternalEntryPoint(
            IPeriodEndQueryService1920 ilr1920MetricsService,
            IPaymentsService paymentServices)
        {
            _ilr1920MetricsService = ilr1920MetricsService;
            _paymentServices = paymentServices;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            await _paymentServices.GetPaymentMetrics(1819, 12);

            await _ilr1920MetricsService.GetPeriodEndMetrics(70);

            return true;
        }
    }
}