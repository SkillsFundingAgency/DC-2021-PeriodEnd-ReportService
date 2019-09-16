using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class LearningDelivery
    {
        public int? AimSeqNumber { get; set; }
        public LearningDeliveryValue LearningDeliveryValues { get; set; }
        public List<LearningDeliveryPeriodisedValue> LearningDeliveryPeriodisedValues { get; set; }
    }
}