using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class LearnerInfo
    {
        public string LearnRefNumber { get; set; }

        public ICollection<LearningDeliveryInfo> LearningDeliveries { get; set; }

        public ICollection<LearnerEmploymentStatusInfo> LearnerEmploymentStatus { get; set; }
    }
}