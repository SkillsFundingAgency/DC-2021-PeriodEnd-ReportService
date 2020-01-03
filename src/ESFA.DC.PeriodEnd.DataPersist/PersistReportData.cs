using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.DataPersist
{
    public class PersistReportData : IPersistReportData
    {
        private readonly IBulkInsert _bulkInsert;
        private readonly ILogger _logger;

        public PersistReportData(IBulkInsert bulkInsert, ILogger logger)
        {
            _bulkInsert = bulkInsert;
            _logger = logger;
        }

        public async Task PersistReportDataAsync<T>(List<T> models, int ukPrn, int returnPeriod, string tableName, string connectionString, CancellationToken cancellationToken) 
            where T : AbstractReportModel
        {
            AbstractReportModel.ReturnPeriodSetter = returnPeriod;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand($"DELETE FROM {tableName} WHERE ukPrn = {ukPrn} and returnPeriod = {returnPeriod}", sqlConnection, sqlTransaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        await _bulkInsert.Insert(tableName, models, sqlConnection, sqlTransaction, cancellationToken);
                        sqlTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Persisting {tableName} failed attempting to rollback - {ex.Message}");
                        sqlTransaction.Rollback();
                        _logger.LogDebug($"Persisting {tableName} successfully rolled back");

                        throw;
                    }
                }
            }
        }
    }
}
