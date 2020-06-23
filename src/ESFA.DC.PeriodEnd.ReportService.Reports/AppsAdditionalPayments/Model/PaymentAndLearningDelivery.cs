using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model
{
    public class PaymentAndLearningDelivery
    {
        public Payment Payment { get; set; }
        public AecLearningDelivery LearningDelivery { get; set; }
    }
}