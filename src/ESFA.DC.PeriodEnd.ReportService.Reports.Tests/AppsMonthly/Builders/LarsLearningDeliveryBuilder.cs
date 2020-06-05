using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class LarsLearningDeliveryBuilder : AbstractBuilder<LarsLearningDelivery>
    {
        public const string LearnAimRefTitle = "LearnAimRefTitle";
        public const string LearnAimRef = "LearnAimRef";
        
        public LarsLearningDeliveryBuilder()
        {
            modelObject = new LarsLearningDelivery()
            {
                LearnAimRefTitle = LearnAimRefTitle,
                LearnAimRef = LearnAimRef,
            };
        }
    }
}
