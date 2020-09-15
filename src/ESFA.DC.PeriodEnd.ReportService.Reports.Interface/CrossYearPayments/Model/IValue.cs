namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public interface IValue
    {
        int Period { get; }

        decimal Value { get; }
    }
}
