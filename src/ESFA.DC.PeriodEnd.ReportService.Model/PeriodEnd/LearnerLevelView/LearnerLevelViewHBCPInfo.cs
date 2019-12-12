using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView
{
    public class LearnerLevelViewHBCPInfo
    {
        public long UkPrn { get; set; }

        public List<LearnerLevelViewHBCPModel> HBCPModels { get; set; }
    }
}
