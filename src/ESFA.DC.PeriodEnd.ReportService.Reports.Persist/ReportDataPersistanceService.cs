﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.BulkCopy.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Persist
{
    public class ReportDataPersistanceService<T> : IReportDataPersistanceService<T>
    {
        private readonly IBulkInsert _bulkInsert;
        private readonly Func<SqlConnection> _sqlConnectionFunc;
        private readonly ILogger _logger;
        private readonly string _tableName;

        private const string CleanUpSql = "DELETE FROM {0} WHERE Ukprn = @ukprn AND ReturnPeriod = @returnPeriod";
        private const string ExistsSql = "SELECT TOP 1 1 FROM {0}  WITH (NOLOCK) WHERE Ukprn = @ukprn AND ReturnPeriod = @returnPeriod";

        public ReportDataPersistanceService(string tableName, IBulkInsert bulkInsert, Func<SqlConnection> sqlConnectionFunc, ILogger logger)
        {
            _bulkInsert = bulkInsert;
            _sqlConnectionFunc = sqlConnectionFunc;
            _logger = logger;
            _tableName = tableName;
        }

        public async Task PersistAsync(IReportServiceContext reportServiceContext, IEnumerable<T> reportModels, CancellationToken cancellationToken)
        {
            if (!reportServiceContext.DataPersistFeatureEnabled)
            {
                return;
            }

            using (var connection = _sqlConnectionFunc())
            {
                await connection.OpenAsync(cancellationToken);

                //check if anything exists to be deleted
                var dataExists = await connection.ExecuteScalarAsync<bool>(string.Format(ExistsSql, _tableName), new { reportServiceContext.Ukprn, reportServiceContext.ReturnPeriod });

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (dataExists)
                        {
                            _logger.LogInfo($"Clean up previous data in {_tableName} for ukprn {reportServiceContext.Ukprn}");
                            await connection.ExecuteAsync(string.Format(CleanUpSql, _tableName), new {reportServiceContext.Ukprn, reportServiceContext.ReturnPeriod}, transaction);
                        }
                        else
                        {
                            _logger.LogInfo($"No existing data found for {reportServiceContext.Ukprn} in {_tableName}, no cleanup needed");
                        }

                        _logger.LogInfo($"Persisting report data into {_tableName}");
                        await _bulkInsert.Insert(_tableName, reportModels, connection, transaction, cancellationToken);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        _logger.LogError(e.Message);
                        throw;
                    }
                }
            }
        }
    }
}
