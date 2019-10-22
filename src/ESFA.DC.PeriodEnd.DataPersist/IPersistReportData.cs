using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.DataPersist
{
    public interface IPersistReportData
    {
        Task PersistAppsAdditionalPaymentAsync(List<AppsMonthlyPaymentModel> monthlyPaymentModels, int ukPrn, int returnPeriod, string connectionString, CancellationToken cancellationToken);
    }
}
