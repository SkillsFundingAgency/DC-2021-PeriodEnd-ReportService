using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.DataProvider
{
    public interface IReferenceDataProvider
    {
        Task<(string providerName, DateTime? easSubmissionDateTime, string ilrSubmissionFileName)> ProvideAsync(long ukprn, CancellationToken cancellationToken);
    }
}
