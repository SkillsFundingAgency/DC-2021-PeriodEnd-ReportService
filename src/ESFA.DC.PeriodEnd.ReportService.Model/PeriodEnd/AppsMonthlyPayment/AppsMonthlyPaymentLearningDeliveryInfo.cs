using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearningDeliveryInfo
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public int AimType { get; set; }

        public int AimSeqNumber { get; set; }

        public string SWSupAimId { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int? ProgType { get; set; }

        public int? FworkCode { get; set; }

        public int? PwayCode { get; set; }

        public int? StdCode { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo> ProviderSpecDeliveryMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryFAMInfo> LearningDeliveryFams { get; set; }

        public string EPAOrganisation { get; set; }

        public int? PartnerUkPrn { get; set; }
    }
}