using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess
{
    public interface IPaymentsService
    {
        Task<IEnumerable<PaymentMetrics>> GetPaymentMetrics(int collectionYear, int collectionPeriod);
    }
}