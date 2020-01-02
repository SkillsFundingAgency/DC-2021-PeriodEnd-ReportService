using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.DataPersist
{
    public interface IPersistReportData
    {
        Task PersistReportDataAsync<T>(List<T> models, int ukPrn, int returnPeriod, string tableName, string connectionString, CancellationToken cancellationToken) where T : AbstractReportModel;
    }
}
