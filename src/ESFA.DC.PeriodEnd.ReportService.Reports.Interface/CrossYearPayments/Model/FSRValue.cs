namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public class FSRValue : IValue
    {
        public int AcademicYear { get; set; }

        public int Period { get; set; }

        public decimal Value { get; set; }
    }
}
