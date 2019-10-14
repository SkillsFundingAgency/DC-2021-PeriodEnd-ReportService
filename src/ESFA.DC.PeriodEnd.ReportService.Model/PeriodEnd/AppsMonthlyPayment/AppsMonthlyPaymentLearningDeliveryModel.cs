using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearningDeliveryModel
    {
        public int? Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public int? AimType { get; set; }

        public byte? AimSeqNumber { get; set; }

        public DateTime? LearnStartDate { get; set; }

        public DateTime? OrigLearnStartDate { get; set; }

        public DateTime? LearnPlanEndDate { get; set; }

        public int? FundModel { get; set; }

        public int ProgType { get; set; }

        public int FworkCode { get; set; }

        public int PwayCode { get; set; }

        public int StdCode { get; set; }

        public int? PartnerUkprn { get; set; }

        public string ConRefNumber { get; set; }

        public string EpaOrgId { get; set; }

        public string SwSupAimId { get; set; }

        public int? CompStatus { get; set; }

        public DateTime? LearnActEndDate { get; set; }

        public int? Outcome { get; set; }

        public DateTime? AchDate { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo> ProviderSpecDeliveryMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryFAMInfo> LearningDeliveryFams { get; set; }
    }
}