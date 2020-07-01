using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model
{
    public class AecLearningDelivery
    {
        // Key Fields
        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int? ProgType { get; set; }

        public int? StdCode { get; set; }

        public int? FworkCode { get; set; }

        public int? PwayCode { get; set; }

        // Data Fields

        public int AimSequenceNumber { get; set; }

        public int? LearnDelEmpIdFirstAdditionalPaymentThreshold { get; set; }

        public int? LearnDelEmpIdSecondAdditionalPaymentThreshold { get; set; }
    }
}
