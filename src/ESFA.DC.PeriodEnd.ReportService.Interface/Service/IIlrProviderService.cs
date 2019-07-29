namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    using System.Threading;
    using System.Threading.Tasks;
    using ESFA.DC.ILR.Model.Interface;
    using ESFA.DC.PeriodEnd.ReportService.Model.ILR;

    public interface IIlrProviderService
    {
        Task<IMessage> GetIlrFile(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task<ILRSourceFileInfo> GetLastSubmittedIlrFile(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
