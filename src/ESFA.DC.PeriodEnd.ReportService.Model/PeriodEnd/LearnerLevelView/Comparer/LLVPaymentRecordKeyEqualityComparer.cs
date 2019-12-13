namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView.Comparer
{
    public class LLVPaymentRecordKeyEqualityComparer : ILLVPaymentRecordKeyEqualityComparer
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
