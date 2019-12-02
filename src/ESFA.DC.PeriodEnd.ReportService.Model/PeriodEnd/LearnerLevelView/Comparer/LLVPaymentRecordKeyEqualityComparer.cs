using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer
{
    public class LLVPaymentRecordKeyEqualityComparer : IEqualityComparer<LearnerLevelViewPaymentsKey>, ILLVPaymentRecordKeyEqualityComparer
    {
        public bool Equals(LearnerLevelViewPaymentsKey x, LearnerLevelViewPaymentsKey y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(LearnerLevelViewPaymentsKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
