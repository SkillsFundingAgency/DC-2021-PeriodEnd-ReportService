using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class AppsCoInvestmentILRInfo
    {
        public int UkPrn { get; set; }

        public List<LearnerInfo> Learners { get; set; }
    }
}
