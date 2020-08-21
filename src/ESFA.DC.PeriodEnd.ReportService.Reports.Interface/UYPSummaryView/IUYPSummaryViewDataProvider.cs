using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView
{
    public interface IUYPSummaryViewDataProvider
    {
        Task<ICollection<Payment>> GetDASPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken);

        Task<ICollection<LearningDeliveryEarning>> GetLearnerDeliveryEarningsAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<PriceEpisodeEarning>> GetPriceEpisodeEarningsAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<Learner>> GetILRLearnerInfoAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<CoInvestmentInfo>> GetCoinvestmentsAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<DataLock>> GetDASDataLockAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<HBCPInfo>> GetHBCPInfoAsync(int ukprn, CancellationToken cancellationToken);

        Task<IDictionary<long, string>> GetLegalEntityNameAsync(int ukprn, IEnumerable<long> apprenticeshipIds, CancellationToken cancellationToken);
    }
}
