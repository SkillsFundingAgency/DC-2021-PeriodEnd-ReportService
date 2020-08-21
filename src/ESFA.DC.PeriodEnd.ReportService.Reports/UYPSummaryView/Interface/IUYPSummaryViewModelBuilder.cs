using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.Interface
{
    public interface IUYPSummaryViewModelBuilder
    {
        ICollection<LearnerLevelViewModel> Build(
            ICollection<Payment> payments,
            ICollection<Learner> learners,
            ICollection<LearningDeliveryEarning> ldEarnings,
            ICollection<PriceEpisodeEarning> peEarnings,
            ICollection<CoInvestmentInfo> coInvestmentInfo,
            ICollection<DataLock> datalocks,
            ICollection<HBCPInfo> hbcpInfo,
            IDictionary<long, string> legalEntityNameDictionary,
            int returnPeriod,
            int ukprn);
    }
}
