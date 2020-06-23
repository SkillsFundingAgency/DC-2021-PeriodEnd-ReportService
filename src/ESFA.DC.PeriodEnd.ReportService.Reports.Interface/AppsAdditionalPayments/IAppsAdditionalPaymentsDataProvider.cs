using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments
{
    public interface IAppsAdditionalPaymentsDataProvider
    {
        Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken);

        Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<AecLearningDelivery>> GetAecLearningDeliveriesAsync(int ukprn, CancellationToken cancellationToken);

        Task<ICollection<ApprenticeshipPriceEpisodePeriodisedValues>> GetPriceEpisodesAsync(int ukprn, CancellationToken cancellationToken);
    }
}
