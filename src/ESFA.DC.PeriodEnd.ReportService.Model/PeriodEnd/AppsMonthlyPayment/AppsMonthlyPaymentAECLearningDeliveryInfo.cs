using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentAECLearningDeliveryInfo
    {
        public int? Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public byte? AimSequenceNumber { get; set; }

        public string LearnAimRef { get; set; }

        public int? PlannedNumOnProgInstalm { get; set; }
    }
}