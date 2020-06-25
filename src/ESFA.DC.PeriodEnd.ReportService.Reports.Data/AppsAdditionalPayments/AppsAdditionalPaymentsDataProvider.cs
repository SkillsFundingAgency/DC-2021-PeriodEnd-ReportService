using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentsDataProvider : IAppsAdditionalPaymentsDataProvider
    {
        private readonly IPaymentsDataProvider _paymentsDataProvider;
        private readonly ILearnerDataProvider _learnerDataProvider;
        private readonly IAecLearningDeliveryDataProvider _aecLearningDeliveryDataProvider;
        private readonly IAppsPriceEpisodePeriodisedValuesDataProvider _appsPriceEpisodePeriodisedValuesDataProvider;

        public AppsAdditionalPaymentsDataProvider(
            IPaymentsDataProvider paymentsDataProvider,
            ILearnerDataProvider learnerDataProvider,
            IAecLearningDeliveryDataProvider aecLearningDeliveryDataProvider,
            IAppsPriceEpisodePeriodisedValuesDataProvider appsPriceEpisodePeriodisedValuesDataProvider)
        {
            _paymentsDataProvider = paymentsDataProvider;
            _learnerDataProvider = learnerDataProvider;
            _aecLearningDeliveryDataProvider = aecLearningDeliveryDataProvider;
            _appsPriceEpisodePeriodisedValuesDataProvider = appsPriceEpisodePeriodisedValuesDataProvider;
        }

        public async Task<ICollection<Payment>> GetPaymentsAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.ProvideAsync(reportServiceContext.CollectionYear,
                reportServiceContext.Ukprn, cancellationToken);
        }

        public async Task<ICollection<Learner>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return await _learnerDataProvider.ProvideAsync(reportServiceContext.Ukprn, cancellationToken);
        }

        public async Task<ICollection<AecLearningDelivery>> GetAecLearningDeliveriesAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return await _aecLearningDeliveryDataProvider.ProvideAsync(reportServiceContext.Ukprn, cancellationToken);
        }

        public async Task<ICollection<ApprenticeshipPriceEpisodePeriodisedValues>> GetPriceEpisodesAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            return await _appsPriceEpisodePeriodisedValuesDataProvider.ProvideAsync(reportServiceContext.Ukprn, cancellationToken);
        }
    }
}