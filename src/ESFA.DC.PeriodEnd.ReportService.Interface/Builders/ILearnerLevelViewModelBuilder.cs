using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface ILearnerLevelViewModelBuilder
    {
        IReadOnlyList<LearnerLevelViewModel> BuildLearnerLevelViewModelList(
             AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
             AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo,
             AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
             AppsMonthlyPaymentDasEarningsInfo appsMonthlyPaymentDasEarningsInfo,
             AppsMonthlyPaymentFcsInfo appsMonthlyPaymentFcsInfo,
             IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList,
             int returnPeriod);
    }
}
