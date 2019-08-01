namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment
{
    public class AECLearningDeliveryInfo
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public int? LearnDelEmpIdFirstAdditionalPaymentThreshold { get; set; }

        public int? LearnDelEmpIdSecondAdditionalPaymentThreshold { get; set; }
    }
}
