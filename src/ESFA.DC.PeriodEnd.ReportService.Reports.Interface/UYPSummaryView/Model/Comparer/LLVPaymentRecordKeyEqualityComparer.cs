namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
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
