using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView
{
    public class LearnerLevelViewFM36Info
    {
        public int UkPrn { get; set; }

        public List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> AECApprenticeshipPriceEpisodePeriodisedValues { get; set; }

        public List<AECLearningDeliveryPeriodisedValuesInfo> AECLearningDeliveryPeriodisedValuesInfo { get; set; }

        public List<LearnerLevelViewEarningsFLT> AECPriceEpisodeFLTsInfo { get; set; }
    }
}
