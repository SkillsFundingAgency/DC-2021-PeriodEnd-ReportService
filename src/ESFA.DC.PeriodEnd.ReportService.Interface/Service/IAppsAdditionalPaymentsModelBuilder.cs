using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IAppsAdditionalPaymentsModelBuilder
    {
        IEnumerable<AppsAdditionalPaymentsModel> BuildModel(
            IList<AppsAdditionalPaymentLearnerInfo> appsAdditionalPaymentIlrInfo,
            IList<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> apprenticeshipPriceEpisodes,
            IList<AECLearningDeliveryInfo> rulebaseLearningDeliveries,
            IList<DASPaymentInfo> appsAdditionalPaymentDasPaymentsInfo,
            IDictionary<long, string> legalNameDictionary,
            int ukPrn);
    }
}