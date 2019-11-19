namespace ESFA.DC.PeriodEnd.ReportService.Interface.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; }

        string FCSConnectionString { get; }

        string ILRDataStoreConnectionString { get; }

        string ILRReferenceDataConnectionString { get; }

        string LarsConnectionString { get; }

        string OrgConnectionString { get; }

        string SummarisedActualsConnectionString { get; }

        string JobQueueManagerConnectionString { get; }

        string EasConnectionString { get; }

        string PeriodEndReportServiceDBCommandTimeout { get; }
    }
}
