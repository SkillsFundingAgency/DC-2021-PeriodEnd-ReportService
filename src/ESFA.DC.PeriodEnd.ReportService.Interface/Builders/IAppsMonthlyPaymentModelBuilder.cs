using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface IAppsMonthlyPaymentModelBuilder
    {
        IOrderedEnumerable<AppsMonthlyPaymentReportRowModel> BuildAppsMonthlyPaymentModelList(
            AppsMonthlyPaymentILRInfo ilrData,
            AppsMonthlyPaymentRulebaseInfo rulebaseData,
            AppsMonthlyPaymentDASInfo paymentsData,
            AppsMonthlyPaymentDasEarningsInfo earningsData,
            IDictionary<string, string> fcsData,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> larsData);
    }
}