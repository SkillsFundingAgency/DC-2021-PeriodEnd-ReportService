using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment
{
    public class AppsAdditionalPaymentRulebaseInfo
    {
        public int UkPrn { get; set; }

        public List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> AECApprenticeshipPriceEpisodePeriodisedValues { get; set; }

        public List<AECLearningDeliveryInfo> AECLearningDeliveries { get; set; }
    }
}
