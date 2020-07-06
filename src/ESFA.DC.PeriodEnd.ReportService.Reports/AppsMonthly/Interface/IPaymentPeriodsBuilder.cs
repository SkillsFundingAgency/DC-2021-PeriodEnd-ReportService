using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface
{
    public interface IPaymentPeriodsBuilder
    {
        PaymentPeriods BuildPaymentPeriods(IEnumerable<Payment> payments);
    }
}
