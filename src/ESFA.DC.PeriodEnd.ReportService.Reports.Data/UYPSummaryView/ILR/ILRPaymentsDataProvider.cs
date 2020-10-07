using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.UYPSummaryView.ILR
{
    public class ILRPaymentsDataProvider : IILRPaymentsDataProvider
    {
        private readonly Func<SqlConnection> _sqlConnectionFunc;

        private readonly string learnerSql = "SELECT LearnRefNumber, FamilyName, GivenNames, ULN FROM Valid.Learner WHERE Ukprn = @ukprn";
        private readonly string learnerEmpStatSql = "SELECT LearnRefNumber, EmpStat, DateEmpStatApp, EmpId FROM Valid.LearnerEmploymentStatus WHERE Ukprn = @ukprn";

        private readonly string learnerDeliveryEarningsSql = @"SELECT LD.LearnRefNumber, LD.LearnDelMathEng, LD.AimSeqNumber, LDPV.AttributeName, Period_1, Period_2, Period_3,
                                                                    Period_4, Period_5, Period_6, Period_7, Period_8, Period_9, Period_10, Period_11, Period_12
                                                                FROM Rulebase.AEC_LearningDelivery LD INNER JOIN Rulebase.AEC_LearningDelivery_PeriodisedValues LDPV 
                                                                ON LDPV.UKPRN = LD.UKPRN AND LDPV.LearnRefNumber = LD.LearnRefNumber AND LDPV.AimSeqNumber = LD.AimSeqNumber AND LD.UKPRN = LDPV.UKPRN
                                                                WHERE LD.Ukprn = @ukprn";

        private readonly string priceEpisodeEarningsSql = @"SELECT DISTINCT LearnRefNumber, AttributeName, PriceEpisodeIdentifier, Period_1, Period_2, 
                                                                   Period_3, Period_4, Period_5, Period_6, Period_7, Period_8, Period_9, Period_10, Period_11, Period_12
                                                                FROM Rulebase.AEC_ApprenticeshipPriceEpisode_PeriodisedValues PEPV 
                                                                WHERE PEPV.Ukprn = @ukprn";

        private readonly string coInvestmentInfoSql = $@"SELECT DISTINCT L.LearnRefNumber, LD.LearnAimRef, AFP.AFinDate, AFP.AFinType, AFP.AFinCode, AFP.AFinAmount
                                                                FROM Valid.Learner L INNER JOIN Valid.LearningDelivery LD ON L.LearnRefNumber = LD.LearnRefNumber AND L.UKPRN = LD.UKPRN
                                                                    INNER JOIN Valid.AppFinRecord AFP ON L.LearnRefNumber = AFP.LearnRefNumber AND L.UKPRN = AFP.UKPRN
                                                                WHERE L.Ukprn = @ukprn
                                                                    AND LD.LearnAimRef = 'ZPROG001'
                                                                    AND AFP.AFinDate >= '{DateConstants.BeginningOfYear}'
                                                                    AND AFP.AFinDate <= '{DateConstants.EndOfYear}'
                                                                    AND AFP.AFinType = '{FinTypes.PMR}'";

        public ILRPaymentsDataProvider(Func<SqlConnection> sqlConnectionFunc)
        {
            _sqlConnectionFunc = sqlConnectionFunc;
        }

        public async Task<ICollection<Learner>> GetLearnerAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<Learner>(learnerSql, new { ukprn });
                var resultEmpStat = await connection.QueryAsync<LearnerEmploymentStatus>(learnerEmpStatSql, new { ukprn });

                // Return data by combining the two result sets
                foreach (Learner learner in result)
                {
                    learner.LearnerEmploymentStatuses = resultEmpStat.Where(les => les.LearnRefNumber == learner.LearnRefNumber).ToList();
                }

                return result.ToList();
            }
        }

        public async Task<ICollection<LearningDeliveryEarning>> GetLearnerDeliveryEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<LearningDeliveryEarning>(learnerDeliveryEarningsSql, new { ukprn });

                return result.ToList();
            }
        }

        public async Task<ICollection<PriceEpisodeEarning>> GetPriceEpisodeEarningsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<PriceEpisodeEarning>(priceEpisodeEarningsSql, new { ukprn });

                return result.ToList();
            }
        }

        public async Task<ICollection<CoInvestmentInfo>> GetCoinvestmentsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var connection = _sqlConnectionFunc())
            {
                var result = await connection.QueryAsync<CoInvestmentInfo>(coInvestmentInfoSql, new { ukprn });

                return result.ToList();
            }
        }

    }
}
