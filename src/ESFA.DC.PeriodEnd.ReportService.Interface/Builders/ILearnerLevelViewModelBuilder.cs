using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface ILearnerLevelViewModelBuilder
    {
        IReadOnlyList<LearnerLevelViewModel> BuildLearnerLevelViewModelList(
             AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
             AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
             AppsMonthlyPaymentDasEarningsInfo appsMonthlyPaymentDasEarningsInfo,
             AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
             IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList,
             LearnerLevelViewFM36Info learnerLevelViewFM36Info,
             int returnPeriod);
    }
}
