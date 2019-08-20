
using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearningDeliveryInfo
    {
        public string UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public string AimType { get; set; }

        public string AimSeqNumber { get; set; }

        public string SWSupAimId { get; set; }

        public string LearnStartDate { get; set; }

        public string ProgType { get; set; }

        public string FworkCode { get; set; }

        public string PwayCode { get; set; }

        public string StdCode { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo> ProviderSpecDeliveryMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryFAMInfo> LearningDeliveryFams { get; set; }
        public string EPAOrganisation { get; set; }
        public string PartnerUkPrn { get; set; }
    }
}