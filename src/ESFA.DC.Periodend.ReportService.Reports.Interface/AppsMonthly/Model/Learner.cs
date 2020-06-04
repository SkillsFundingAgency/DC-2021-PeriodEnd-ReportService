using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class Learner
    {
        public string LearnRefNumber { get; set; }

        public List<LearningDelivery> LearningDeliveries { get; set; }
    }
}
