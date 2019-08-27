using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearnerInfo
    {
        public string Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public string UniqueLearnerNumber { get; set; }

        public string CampId { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo> ProviderSpecLearnerMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearnerEmploymentStatusInfo> LearnerEmploymentStatus { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryInfo> LearningDeliveries { get; set; }
    }
}