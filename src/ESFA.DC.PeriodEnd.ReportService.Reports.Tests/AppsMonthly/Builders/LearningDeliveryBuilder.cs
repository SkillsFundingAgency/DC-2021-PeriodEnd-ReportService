using System;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class LearningDeliveryBuilder : AbstractBuilder<LearningDelivery>
    {
        public LearningDeliveryBuilder()
        {
            modelObject = new LearningDelivery()
            {
                AimSequenceNumber = 1,
                FworkCode = 10,
                LearnAimRef = "LearnAimRef",
                LearnStartDate = new DateTime(2020, 8, 1),
                ProgType = 20,
                PwayCode = 30,
                StdCode = 40,
            };
        }
    }
}
