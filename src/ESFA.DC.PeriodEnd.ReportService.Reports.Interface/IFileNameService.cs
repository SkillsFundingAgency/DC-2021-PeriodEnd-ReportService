using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface
{
    public interface IFileNameService
    {
        string GetFilename(IReportServiceContext reportServiceContext, string fileName, OutputTypes outputType, bool includeDateTime = true, bool includeUkprn = true);

        string GetInternalFilename(IReportServiceContext reportServiceContext, string fileName, OutputTypes outputType,
            bool includeDateTime = true, bool includeReturnPeriod = true);
    }
}
