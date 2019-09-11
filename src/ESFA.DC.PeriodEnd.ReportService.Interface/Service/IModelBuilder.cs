namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IModelBuilder<out T>
    {
        T Build(IReportServiceContext reportServiceContext, IReportServiceDependentData reportServiceDependentData);
    }
}
