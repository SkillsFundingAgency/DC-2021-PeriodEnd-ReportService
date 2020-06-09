using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model
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

        public List<LearningDeliveryFam> LearningDeliveryFams { get; set; }

        public List<ProviderMonitoring> ProviderSpecDeliveryMonitorings { get; set; }

        public AecLearningDelivery AecLearningDelivery { get; set; }
    }
}
