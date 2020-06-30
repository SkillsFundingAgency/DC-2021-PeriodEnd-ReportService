using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider
{
    public interface IPaymentsDataProvider
    {
        Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, CancellationToken cancellationToken);
    }
}
