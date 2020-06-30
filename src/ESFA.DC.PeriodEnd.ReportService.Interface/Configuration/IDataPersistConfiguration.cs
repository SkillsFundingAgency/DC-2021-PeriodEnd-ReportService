namespace ESFA.DC.PeriodEnd.ReportService.Interface.Configuration
{
    public interface IDataPersistConfiguration
    {
        string ReportDataConnectionString { get; }

        string DataPersistFeatureEnabled { get; }
    }
}
