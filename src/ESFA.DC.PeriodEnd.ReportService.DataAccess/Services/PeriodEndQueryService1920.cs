using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.ILR1920.DataStore.EF.StoredProc;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ActCountReport;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.PeriodEndMetrics;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.DataAccess.Services
{
    public class PeriodEndQueryService1920 : IPeriodEndQueryService1920
    {
        private readonly Func<ILR1920_DataStoreEntitiesStoredProc> _contextFactory;

        public PeriodEndQueryService1920(Func<ILR1920_DataStoreEntitiesStoredProc> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<IlrMetrics>> GetPeriodEndMetrics(int periodId)
        {
            List<IlrMetrics> metrics;

            using (var context = _contextFactory())
            {
                metrics = (await context.PeriodEndMetrics
                    .FromSql("GetPeriodEndMetrics @periodId", new SqlParameter("@periodId", periodId))
                    .ToListAsync())
                    .Select(entity => new IlrMetrics
                    {
                        TransactionType = entity.TransactionType,
                        EarningsYTD = entity.EarningsYTD,
                        EarningsACT1 = entity.EarningsACT1,
                        EarningsACT2 = entity.EarningsACT2
                    }).ToList();
            }

            return metrics;
        }

        public async Task<IEnumerable<ActCountModel>> GetActCounts()
        {
            List<ActCountModel> actAccountsModel;

            using (var context = _contextFactory())
            {
                actAccountsModel = (await context.ActCounts
                        .AsNoTracking()
                        .FromSql("GetACTCounts")
                        .ToListAsync())
                    .Select(entity => new ActCountModel()
                    {
                        Ukprn = entity.UkPrn,
                        ActCountOne = entity.LearnersAct1,
                        ActCountTwo = entity.LearnersAct2
                    }).ToList();
            }

            return actAccountsModel;
        }
    }
}