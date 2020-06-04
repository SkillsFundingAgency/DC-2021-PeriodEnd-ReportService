using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model
{
    public class AppsMonthlyRecord
    {
        public RecordKey RecordKey { get; set; }

        public Learner Learner { get; set; }

        public LearningDelivery LearningDelivery { get; set; }
    }
}
