using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface ILearnerLevelViewModelBuilder
    {
        IReadOnlyList<LearnerLevelViewModel> BuildLearnerLevelViewModelList(
            int ukprn,
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            LearnerLevelViewDASDataLockInfo learnerLevelViewDASDataLockInfo,
            IDictionary<LearnerLevelViewPaymentsKey, List<AppsMonthlyPaymentDasPaymentModel>> paymentsDictionary,
            IDictionary<string, List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> aECPriceEpisodeDictionary,
            IDictionary<string, List<AECLearningDeliveryPeriodisedValuesInfo>> aECLearningDeliveryDictionary,
            int returnPeriod);
    }
}
