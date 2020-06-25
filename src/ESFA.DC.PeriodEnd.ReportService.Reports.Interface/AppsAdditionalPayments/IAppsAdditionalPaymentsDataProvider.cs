using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments
{
    public interface IAppsAdditionalPaymentsDataProvider
    {
        Task<ICollection<Payment>> GetPaymentsAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task<ICollection<Learner>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task<ICollection<AecLearningDelivery>> GetAecLearningDeliveriesAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task<ICollection<ApprenticeshipPriceEpisodePeriodisedValues>> GetPriceEpisodesAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
