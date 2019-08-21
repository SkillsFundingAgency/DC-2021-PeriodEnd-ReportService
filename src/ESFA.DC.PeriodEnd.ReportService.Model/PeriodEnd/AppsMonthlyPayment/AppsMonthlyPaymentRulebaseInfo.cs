using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentRulebaseInfo
    {
        public int UkPrn { get; set; }

        public string LearnRefNumber { get; set; }

        public List<AECApprenticeshipPriceEpisodeInfo> AECApprenticeshipPriceEpisodes { get; set; }
    }
}
