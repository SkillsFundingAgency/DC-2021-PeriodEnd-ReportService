using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsMonthly
{
    public class AppsMonthlyPaymentsDataProvider : IAppsMonthlyPaymentsDataProvider
    {
        public Task<ICollection<ContractAllocation>> GetContractAllocationsAsync(int ukprn, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Earning>> GetEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<LarsLearningDelivery>> GetLarsLearningDeliveriesAsync(ICollection<string> learnAimRefs, CancellationToken cancellationToken)
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
