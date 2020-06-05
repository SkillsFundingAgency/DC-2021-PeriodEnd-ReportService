using System.Collections.Generic;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class LearnerBuilder : AbstractBuilder<Learner>
    {
        public const string LearnRefNumber = "LearnRefNumber";

        public LearnerBuilder()
        {
            modelObject = new Learner()
            {
                LearnRefNumber = LearnRefNumber,
                LearningDeliveries =  new List<LearningDelivery>()
                {
                    new LearningDeliveryBuilder().Build()
                },
                ProviderSpecLearnMons = new List<ProviderSpecLearnMon>()
                {
                    new ProviderSpecLearnMonBuilder().With(m => m.ProvSpecLearnMonOccur, "A").Build(),
                    new ProviderSpecLearnMonBuilder().With(m => m.ProvSpecLearnMonOccur, "B").Build(),
                }
            };
        }
    }
}
