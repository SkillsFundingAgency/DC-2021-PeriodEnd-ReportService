namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public struct EasPaymentValue
    {
        public EasPaymentValue(decimal? paymentValue, int? devolvedAreaSof)
        {
            PaymentValue = paymentValue;
            DevolvedAreaSof = devolvedAreaSof;
        }

        public decimal? PaymentValue { get; set; }
        public int? DevolvedAreaSof { get; set; }
    }
}
