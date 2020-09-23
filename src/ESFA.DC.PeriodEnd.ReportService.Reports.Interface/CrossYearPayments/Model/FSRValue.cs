namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class FSRValue : IValue
    {
        public int AcademicYear { get; set; }

        public int DeliveryPeriod { get; set; }

        public int CollectionPeriod { get; set; }

        public decimal Value { get; set; }
    }
}
