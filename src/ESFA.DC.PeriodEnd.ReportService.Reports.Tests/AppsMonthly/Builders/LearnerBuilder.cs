﻿using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class LearnerBuilder : AbstractBuilder<Learner>
    {
        public const string LearnRefNumber = "LearnRefNumber";
        public const string FamilyName = "FamilyName";
        public const string GivenNames = "GivenNames";

        public LearnerBuilder()
        {
            modelObject = new Learner()
            {
                LearnRefNumber = LearnRefNumber,
                FamilyName = FamilyName,
                GivenNames = GivenNames,
                LearningDeliveries =  new List<LearningDelivery>()
                {
                    new LearningDeliveryBuilder().Build()
                },
                ProviderSpecLearnMonitorings = new List<ProviderMonitoring>()
                {
                    new ProviderMonitoringBuilder().With(m => m.Occur, "A").Build(),
                    new ProviderMonitoringBuilder().With(m => m.Occur, "B").Build(),
                },
                LearnerEmploymentStatuses = new List<LearnerEmploymentStatus>()
                {
                    new LearnerEmploymentStatusBuilder().Build()
                }
            };
        }
    }
}
