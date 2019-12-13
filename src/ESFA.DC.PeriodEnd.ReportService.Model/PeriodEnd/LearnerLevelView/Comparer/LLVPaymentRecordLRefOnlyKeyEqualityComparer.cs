namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView.Comparer
{
    public class LLVPaymentRecordLRefOnlyKeyEqualityComparer : ILLVPaymentRecordLRefOnlyKeyEqualityComparer
    {
        public bool Equals(LearnerLevelViewPaymentsKey x, LearnerLevelViewPaymentsKey y)
        {
            return x.LearnerReferenceNumber == y.LearnerReferenceNumber;
        }

        public int GetHashCode(LearnerLevelViewPaymentsKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
