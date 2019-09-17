using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearnerModel
    {
        public int? Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public long? UniqueLearnerNumber { get; set; }

        public string CampId { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecLearnerMonitoringInfo> ProviderSpecLearnerMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearnerEmploymentStatusInfo> LearnerEmploymentStatus { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryModel> LearningDeliveries { get; set; }
    }
}