using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment
{
    public class AppsAdditionalPaymentLearnerInfo
    {
        public string LearnRefNumber { get; set; }

        public long ULN { get; set; }

        public ICollection<AppsAdditionalPaymentLearningDeliveryInfo> LearningDeliveries { get; set; }

        public virtual ICollection<AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo> ProviderSpecLearnerMonitorings { get; set; }
    }
}