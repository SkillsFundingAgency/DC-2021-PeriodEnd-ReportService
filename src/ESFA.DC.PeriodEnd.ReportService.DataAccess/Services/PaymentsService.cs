using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.DataAccess.Contexts;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.DataAccess.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly Func<PaymentsContext> _contextFactory;
        private readonly ILogger _logger;

        public PaymentsService(
            Func<PaymentsContext> contextFactory,
            ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentMetrics>> GetPaymentMetrics(int collectionYear, int collectionPeriod)
        {
            List<PaymentMetrics> metrics;

            try
            {
                using (var context = _contextFactory())
                {
                    metrics = (await context.PaymentMetrics
                        .FromSql<PaymentMetricsEntity>(
                            "Metrics_Payments @academicYear, @collectionPeriod",
                            new SqlParameter("@academicYear", collectionYear),
                            new SqlParameter("@collectionPeriod", collectionPeriod)).ToListAsync())
                        .Select(pm => new PaymentMetrics
                        {
                            TransactionType = pm.TransactionType,
                            EarningsYTD = pm.EarningsYTD,
                            EarningsACT1 = pm.EarningsACT1,
                            EarningsACT2 = pm.EarningsACT2,
                            NegativeEarnings = pm.NegativeEarnings,
                            NegativeEarningsACT1 = pm.NegativeEarningsACT1,
                            NegativeEarningsACT2 = pm.NegativeEarningsACT2,
                            PaymentsYTD = pm.PaymentsYTD,
                            PaymentsACT1 = pm.PaymentsACT1,
                            PaymentsACT2 = pm.PaymentsACT2,
                            DataLockErrors = pm.DataLockErrors,
                            HeldBackCompletion = pm.HeldBackCompletion,
                            HBCPACT1 = pm.HBCPACT1,
                            HBCPACT2 = pm.HBCPACT2
                        })
                        .ToList();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }

            return metrics;
        }
    }
}