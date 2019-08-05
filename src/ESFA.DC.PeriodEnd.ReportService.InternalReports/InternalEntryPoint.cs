using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.InternalReports
{
    public class InternalEntryPoint
    {
        private readonly IPaymentsService _paymentServices;

        public InternalEntryPoint(IPaymentsService paymentServices)
        {
            _paymentServices = paymentServices;
        }

        public async Task<bool> Callback(CancellationToken cancellationToken)
        {
            await _paymentServices.GetPaymentMetrics(1819, 12);
            return false;
        }
    }
}