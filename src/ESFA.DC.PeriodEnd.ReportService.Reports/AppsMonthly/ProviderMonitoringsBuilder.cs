using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class ProviderMonitoringsBuilder : IProviderMonitoringsBuilder
    {
        public ProviderMonitorings BuildProviderMonitorings(Learner learner, LearningDelivery learningDelivery)
        {
            if (learner?.ProviderSpecLearnMonitorings == null && learningDelivery?.ProviderSpecDeliveryMonitorings == null)
            {
                return null;
            }

            var providerMonitorings = new ProviderMonitorings();

            if (learner?.ProviderSpecLearnMonitorings != null)
            {
                providerMonitorings.LearnerA = GetProviderMonForOccur(learner.ProviderSpecLearnMonitorings, ProviderMonitorings.OccurA);
                providerMonitorings.LearnerB = GetProviderMonForOccur(learner.ProviderSpecLearnMonitorings, ProviderMonitorings.OccurB);
            }

            if (learningDelivery?.ProviderSpecDeliveryMonitorings != null)
            {
                providerMonitorings.LearningDeliveryA = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurA);
                providerMonitorings.LearningDeliveryB = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurB);
                providerMonitorings.LearningDeliveryC = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurC);
                providerMonitorings.LearningDeliveryD = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurD);
            }

            return providerMonitorings;
        }

        private string GetProviderMonForOccur(IEnumerable<ProviderMonitoring> providerSpecLearnMons, string occur)
            => providerSpecLearnMons?.FirstOrDefault(m => m.Occur.CaseInsensitiveEquals(occur))?.Mon;
    }
}
