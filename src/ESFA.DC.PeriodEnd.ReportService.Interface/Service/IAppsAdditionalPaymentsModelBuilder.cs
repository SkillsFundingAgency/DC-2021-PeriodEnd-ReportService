using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd
{
    public interface IAppsAdditionalPaymentsModelBuilder
    {
        IEnumerable<AppsAdditionalPaymentsModel> BuildModel(AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo, AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo, AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo);
    }
}