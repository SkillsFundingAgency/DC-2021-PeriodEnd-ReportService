using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.DataPersist
{
    public class PersistReportData : IPersistReportData
    {
        private readonly IBulkInsert _bulkInsert;

        public PersistReportData(IBulkInsert bulkInsert)
        {
            _bulkInsert = bulkInsert;
        }
        public async Task PersistAppsAdditionalPaymentAsync(List<AppsMonthlyPaymentModel> monthlyPaymentModels,int ukPrn, int returnPeriod, SqlConnection sqlConnection, SqlTransaction sqlTransaction, CancellationToken cancellationToken)
        {

            using (SqlCommand command = new SqlCommand($"DELETE FROM AppsMonthlyPayment WHERE ukPrn = {ukPrn} and returnPeriod = {returnPeriod}", sqlConnection, sqlTransaction))
            {
                command.ExecuteNonQuery();
            }
            await _bulkInsert.Insert("AppsMonthlyPayment", monthlyPaymentModels, sqlConnection, sqlTransaction, cancellationToken);
        }
    }
}
