namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.Eas
{
    public class EasPaymentTypeInfo
    {
        public int PaymentId { get; set; }

        public string PaymentName { get; set; }

        public bool Fm36 { get; set; }

        public string PaymentTypeDescription { get; set; }

        public int? FundingLineId { get; set; }

        public int? AdjustmentTypeId { get; set; }
    }
}
