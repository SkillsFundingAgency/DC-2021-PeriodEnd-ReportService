namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer
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
