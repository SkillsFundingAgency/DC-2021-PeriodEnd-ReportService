using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentRulebaseInfo
    {
        public int UkPrn { get; set; }

        public string LearnRefNumber { get; set; }

        public List<AppsMonthlyPaymentAECApprenticeshipPriceEpisodeInfo> AecApprenticeshipPriceEpisodeInfoList { get; set; }

        public List<AppsMonthlyPaymentAECLearningDeliveryInfo> AecLearningDeliveryInfoList { get; set; }
    }
}
