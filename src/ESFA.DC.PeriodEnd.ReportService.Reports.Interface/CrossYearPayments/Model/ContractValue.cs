namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class ContractValue : IValue
    {
        public int Period { get; set; }

        public decimal Value { get; set; }
    }
}
