using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.DataProvider;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.UYPSummaryView
{
    public class UYPSummaryViewDataProvider : IUYPSummaryViewDataProvider
    {
        private readonly IPaymentsDataProvider _paymentsDataProvider;
        private readonly IILRPaymentsDataProvider _ilrPaymentsDataPRovider;

        public UYPSummaryViewDataProvider(
            IPaymentsDataProvider paymentsDataProvider,
            IILRPaymentsDataProvider ilrPaymentsDataPRovider)
        {
            _ilrPaymentsDataPRovider = ilrPaymentsDataPRovider;
            _paymentsDataProvider = paymentsDataProvider;
        }

        public async Task<ICollection<Payment>> GetDASPaymentsAsync(int ukprn, int academicYear, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetPaymentsAsync(ukprn, academicYear, cancellationToken);
        }

        public async Task<ICollection<LearningDeliveryEarning>> GetLearnerDeliveryEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _ilrPaymentsDataPRovider.GetLearnerDeliveryEarningsAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<PriceEpisodeEarning>> GetPriceEpisodeEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _ilrPaymentsDataPRovider.GetPriceEpisodeEarningsAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<Learner>> GetILRLearnerInfoAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _ilrPaymentsDataPRovider.GetLearnerAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<CoInvestmentInfo>> GetCoinvestmentsAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _ilrPaymentsDataPRovider.GetCoinvestmentsAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<DataLock>> GetDASDataLockAsync(int ukprn, int academicYear, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetDASDataLockAsync(ukprn, academicYear, cancellationToken);
        }

        public async Task<ICollection<HBCPInfo>> GetHBCPInfoAsync(int ukprn, int academicYear, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetHBCPInfoAsync(ukprn, academicYear, cancellationToken);
        }

        public async Task<IDictionary<long, string>> GetLegalEntityNameAsync(int ukprn, IEnumerable<long> apprenticeshipIds, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetLegalEntityNameAsync(ukprn, apprenticeshipIds, cancellationToken);
        }
    }
}
