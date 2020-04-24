using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Reports
{
    public interface ILegacyReport
    {
        string ReportTaskName { get; }

        string ReportFileName { get; }

        string GetFilename(IReportServiceContext reportServiceContext);

        string GetCustomFilename(IReportServiceContext reportServiceContext, string postFix);

        string GetFilenameForInternalReport(IReportServiceContext reportServiceContext);

        string GetZipFilename(IReportServiceContext reportServiceContext);

        Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken);

        bool IsMatch(string reportTaskName);
    }
}
