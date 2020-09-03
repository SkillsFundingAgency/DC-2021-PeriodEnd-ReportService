using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.DataProvider
{
    public interface IILRPaymentsDataProvider
    {
        Task<ICollection<Learner>> GetLearnerAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<LearningDeliveryEarning>> GetLearnerDeliveryEarningsAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<PriceEpisodeEarning>> GetPriceEpisodeEarningsAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<CoInvestmentInfo>> GetCoinvestmentsAsync(int ukprn, CancellationToken cancellationToken);
    }
}
