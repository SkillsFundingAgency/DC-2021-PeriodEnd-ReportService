
using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearningDeliveryInfo
    {
        public string Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public string AimType { get; set; }

        public string AimSeqNumber { get; set; }

        public DateTime LearnStartDate { get; set; }

        public string OrigLearnStartDate { get; set; }

        public string LearnPlanEndDate { get; set; }

        public string FundModel { get; set; }

        public string ProgType { get; set; }

        public string FworkCode { get; set; }

        public string PwayCode { get; set; }

        public string StdCode { get; set; }

        public string PartnerUkprn { get; set; }

        public string ConRefNumber { get; set; }

        public string EpaOrgId { get; set; }

        public string SwSupAimId { get; set; }

        public string CompStatus { get; set; }

        public string LearnActEndDate { get; set; }

        public string Outcome { get; set; }

        public string AchDate { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo> ProviderSpecDeliveryMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryFAMInfo> LearningDeliveryFams { get; set; }
    }
}