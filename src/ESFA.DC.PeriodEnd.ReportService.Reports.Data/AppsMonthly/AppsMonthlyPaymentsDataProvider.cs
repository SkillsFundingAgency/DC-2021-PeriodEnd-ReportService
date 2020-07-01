using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly
{
    public class AppsMonthlyPaymentsDataProvider : IAppsMonthlyPaymentsDataProvider
    {
        private readonly IPaymentsDataProvider _paymentsDataProvider;
        private readonly ILearnerDataProvider _learnerDataProvider;
        private readonly IFcsDataProvider _fcsDataProvider;
        private readonly IIlrDataProvider _ilrDataProvider;
        private readonly ILarsLearningDeliveryProvider _larsLearningDeliveryProvider;
        private readonly IJsonSerializationService _jsonSerializationService;


        public AppsMonthlyPaymentsDataProvider(IPaymentsDataProvider paymentsDataProvider,
            ILearnerDataProvider learnerDataProvider,
            IFcsDataProvider fcsDataProvider,
            IIlrDataProvider ilrDataProvider,
            ILarsLearningDeliveryProvider larsLearningDeliveryProvider,
            IJsonSerializationService jsonSerializationService)
        {
            _paymentsDataProvider = paymentsDataProvider;
            _learnerDataProvider = learnerDataProvider;
            _fcsDataProvider = fcsDataProvider;
            _ilrDataProvider = ilrDataProvider;
            _larsLearningDeliveryProvider = larsLearningDeliveryProvider;
            _jsonSerializationService = jsonSerializationService;
        }

        public async Task<ICollection<ContractAllocation>> GetContractAllocationsAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await  _fcsDataProvider.GetContractAllocationsAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<Earning>> GetEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetEarningsAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(ICollection<Learner> learners, CancellationToken cancellationToken)
        {
            var learnAimRefs = _jsonSerializationService.Serialize(learners.SelectMany(l => l.LearningDeliveries.Select(ld => ld.LearnAimRef)).Distinct());

            return await _larsLearningDeliveryProvider.GetLarsLearningDeliveriesAsync(learnAimRefs, cancellationToken);
        }

        public async Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _learnerDataProvider.GetLearnersAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetPaymentsAsync(ukprn, academicYear, cancellationToken);
        }

        public async Task<ICollection<AecApprenticeshipPriceEpisode>> GetPriceEpisodesAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _ilrDataProvider.GetPriceEpisodesAsync(ukprn, cancellationToken);
        }
    }
}
