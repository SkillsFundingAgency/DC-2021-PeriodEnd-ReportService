using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.AppsCoInvestment
{
    public class AppsCoInvestmentDataProvider: IAppsCoInvestmentDataProvider
    {
        private readonly IPaymentsDataProvider _paymentsDataProvider;
        private readonly ILearnerDataProvider _learnerDataProvider;
        private readonly IIlrDataProvider _ilrDataProvider;

        public AppsCoInvestmentDataProvider(
            IPaymentsDataProvider paymentsDataProvider,
            ILearnerDataProvider learnerDataProvider,
            IIlrDataProvider ilrDataProvider)
        {
            _paymentsDataProvider = paymentsDataProvider;
            _learnerDataProvider = learnerDataProvider;
            _ilrDataProvider = ilrDataProvider;
        }
        public async Task<ICollection<Payment>> GetPaymentsAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _paymentsDataProvider.GetPaymentsAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<Learner>> GetLearnersAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _learnerDataProvider.GetLearnersAsync(ukprn, cancellationToken);
        }

        public async Task<ICollection<AECApprenticeshipPriceEpisodePeriodisedValues>> GetAecPriceEpisodePeriodisedValuesAsync(int ukprn, CancellationToken cancellationToken)
        {
            return await _ilrDataProvider.GetAecPriceEpisodePeriodisedValuesAsync(ukprn, cancellationToken);
        }
    }
}
