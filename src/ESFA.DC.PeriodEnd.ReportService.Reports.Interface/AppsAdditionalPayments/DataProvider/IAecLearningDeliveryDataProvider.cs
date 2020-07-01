using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider
{
    public interface IAecLearningDeliveryDataProvider
    {
        Task<ICollection<AecLearningDelivery>> ProvideAsync(long ukprn, CancellationToken cancellationToken);
    }
}