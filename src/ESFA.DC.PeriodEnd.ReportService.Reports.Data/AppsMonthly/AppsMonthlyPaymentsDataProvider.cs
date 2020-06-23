using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly
{
    public class AppsMonthlyPaymentsDataProvider : IAppsMonthlyPaymentsDataProvider
    {
        private readonly IPaymentsDataProvider _paymentsDataProvider;
        private readonly ILearnerDataProvider _learnerDataProvider;
        private readonly IFcsDataProvider _fcsDataProvider;
        private readonly IIlrDataProvider _ilrDataProvider;
        private readonly ILarsLearningDeliveryProvider _larsLearningDeliveryProvider;


        public AppsMonthlyPaymentsDataProvider(IPaymentsDataProvider paymentsDataProvider,
            ILearnerDataProvider learnerDataProvider,
            IFcsDataProvider fcsDataProvider,
            IIlrDataProvider ilrDataProvider,
            ILarsLearningDeliveryProvider larsLearningDeliveryProvider)
        {
            _paymentsDataProvider = paymentsDataProvider;
            _learnerDataProvider = learnerDataProvider;
            _fcsDataProvider = fcsDataProvider;
            _ilrDataProvider = ilrDataProvider;
            _larsLearningDeliveryProvider = larsLearningDeliveryProvider;
        }

        public Task<ICollection<ContractAllocation>> GetContractAllocationsAsync(int ukprn, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Earning>> GetEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(ICollection<Learner> learners, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<AecApprenticeshipPriceEpisode>> GetPriceEpisodesAsync(int ukprn, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
