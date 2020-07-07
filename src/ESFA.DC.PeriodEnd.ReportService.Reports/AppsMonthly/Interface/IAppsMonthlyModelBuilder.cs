using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface
{
    public interface IAppsMonthlyModelBuilder
    {
        ICollection<AppsMonthlyRecord> Build(
            ICollection<Payment> payments,
            ICollection<Learner> learners,
            ICollection<ContractAllocation> contractAllocations,
            ICollection<Earning> earnings,
            ICollection<LarsLearningDelivery> larsLearningDeliveries,
            ICollection<AecApprenticeshipPriceEpisode> priceEpisodes);
    }
}
