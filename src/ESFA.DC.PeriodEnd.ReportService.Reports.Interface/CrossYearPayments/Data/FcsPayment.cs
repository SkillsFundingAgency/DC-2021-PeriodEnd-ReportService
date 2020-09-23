namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data
{
    public class FcsPayment
    {
        public string FspCode { get; set; }

        public string ContractAllocationNumber { get; set; }

        public int Period { get; set; }

        public string Type { get; set; }

        public decimal Value { get; set; }
    }
}
