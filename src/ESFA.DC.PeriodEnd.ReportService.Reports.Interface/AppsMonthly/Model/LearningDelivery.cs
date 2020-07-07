using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class LearningDelivery
    {
        // Key Fields

        public string LearnAimRef { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int ProgType { get; set; }

        public int StdCode { get; set; }

        public int FworkCode { get; set; }

        public int PwayCode { get; set; }

        // Data Fields

        public int AimSequenceNumber { get; set; }

        public DateTime? OrigLearnStartDate { get; set; }

        public DateTime? LearnPlanEndDate { get; set; }

        public int? CompStatus { get; set; }

        public DateTime? LearnActEndDate { get; set; }

        public DateTime? AchDate { get; set; }

        public int? Outcome { get; set; }

        public int? AimType { get; set; }

        public string SWSupAimId { get; set; }

        public string EPAOrgId { get; set; }

        public int? PartnerUkprn { get; set; }

        public List<LearningDeliveryFam> LearningDeliveryFams { get; set; }

        public List<ProviderMonitoring> ProviderSpecDeliveryMonitorings { get; set; }

        public AecLearningDelivery AecLearningDelivery { get; set; }
    }
}
