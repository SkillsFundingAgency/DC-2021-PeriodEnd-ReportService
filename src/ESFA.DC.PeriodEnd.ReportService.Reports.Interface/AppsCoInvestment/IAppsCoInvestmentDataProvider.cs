using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment
{
    public interface IAppsCoInvestmentDataProvider
    {
        Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<AECApprenticeshipPriceEpisodePeriodisedValues>> GetAecPriceEpisodePeriodisedValuesAsync(int ukprn, CancellationToken cancellationToken);
    }
}
