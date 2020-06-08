using System;
using System.Collections.Generic;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class LearningDeliveryBuilder : AbstractBuilder<LearningDelivery>
    {
        private const string LDM = "LDM";

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
                LearningDeliveryFams = new List<LearningDeliveryFam>()
                {
                    new LearningDeliveryFamBuilder().With(f => f.Type, LDM).With(f => f.Code, "1").Build(),
                    new LearningDeliveryFamBuilder().With(f => f.Type, LDM).With(f => f.Code, "2").Build(),
                    new LearningDeliveryFamBuilder().With(f => f.Type, LDM).With(f => f.Code, "3").Build(),
                    new LearningDeliveryFamBuilder().With(f => f.Type, LDM).With(f => f.Code, "4").Build(),
                    new LearningDeliveryFamBuilder().With(f => f.Type, LDM).With(f => f.Code, "5").Build(),
                    new LearningDeliveryFamBuilder().With(f => f.Type, LDM).With(f => f.Code, "6").Build(),
                }
            };
        }
    }
}
