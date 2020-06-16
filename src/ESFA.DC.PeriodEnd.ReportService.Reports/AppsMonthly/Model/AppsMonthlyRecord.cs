using System;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model
{
    public class AppsMonthlyRecord
    {
        public RecordKey RecordKey { get; set; }

        public Learner Learner { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public LearningDelivery LearningDelivery { get; set; }

        public string ContractNumber { get; set; }

        public Earning Earning { get; set; }

        public ProviderMonitorings ProviderSpecLearnMonitorings { get; set; }

        public string LearningDeliveryTitle { get; set; }

        public LearningDeliveryFams LearningDeliveryFams { get; set; }

        public AecApprenticeshipPriceEpisode PriceEpisode { get; set; }

        public DateTime? PriceEpisodeStartDate { get; set; }

        public LearnerEmploymentStatus LearnerEmploymentStatus { get; set; }

        public PaymentPeriods PaymentPeriods { get; set; }
    }
}
