using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Lars
{
    public sealed class LearningDelivery
    {
        public LearningDelivery(
            string learningDeliveryLearnAimRef,
            int learningDeliveryAimSeqNumber,
            int? fworkCode,
            int? progType,
            int? pwayCode,
            DateTime learningDeliveryLearnStartDate)
        {
            LearningDeliveryLearnAimRef = learningDeliveryLearnAimRef;
            LearningDeliveryAimSeqNumber = learningDeliveryAimSeqNumber;
            FworkCode = fworkCode;
            ProgType = progType;
            PwayCode = pwayCode;
            LearningDeliveryLearnStartDate = learningDeliveryLearnStartDate;
        }

        public string LearningDeliveryLearnAimRef { get; }

        public int LearningDeliveryAimSeqNumber { get; }

        public int? FworkCode { get; }

        public int? ProgType { get; }

        public int? PwayCode { get; }

        public DateTime LearningDeliveryLearnStartDate { get; }

        public int? FrameworkComponentType { get; set; }
    }
}
