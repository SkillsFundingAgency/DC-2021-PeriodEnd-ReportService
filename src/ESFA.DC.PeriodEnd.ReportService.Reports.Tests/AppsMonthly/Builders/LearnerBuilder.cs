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
                ProviderSpecLearnMonitorings = new List<ProviderMonitoring>()
                {
                    new ProviderMonitoringBuilder().With(m => m.Occur, "A").Build(),
                    new ProviderMonitoringBuilder().With(m => m.Occur, "B").Build(),
                }
            };
        }
    }
}
