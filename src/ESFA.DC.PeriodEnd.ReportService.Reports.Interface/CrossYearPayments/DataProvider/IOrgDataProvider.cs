using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.DataProvider
{
    public interface IOrgDataProvider
    {
        Task<string> ProvideAsync(long ukprn);
    }
}
