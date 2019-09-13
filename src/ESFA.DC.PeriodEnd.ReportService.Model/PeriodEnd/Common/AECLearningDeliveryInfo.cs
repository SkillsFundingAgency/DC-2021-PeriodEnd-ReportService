using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common
{
    public class AECLearningDeliveryInfo
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public AECLearningDeliveryValuesInfo LearningDeliveryValues { get; set; }

        public int? LearnDelEmpIdFirstAdditionalPaymentThreshold { get; set; }

        public int? LearnDelEmpIdSecondAdditionalPaymentThreshold { get; set; }

        public DateTime? AppAdjLearnStartDate { get; set; }
    }
}
