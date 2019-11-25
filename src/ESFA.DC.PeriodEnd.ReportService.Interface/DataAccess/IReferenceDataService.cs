using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess
{
    public interface IReferenceDataService
    {
        Task<string> GetProviderNameAsync(int ukprn, CancellationToken cancellationToken);

        Task<DateTime?> GetLastestEasSubmissionDateTimeAsync(int ukprn, CancellationToken cancellationToken);

        Task<string> GetLatestIlrSubmissionFileNameAsync(int ukprn, CancellationToken cancellationToken);
    }
}
