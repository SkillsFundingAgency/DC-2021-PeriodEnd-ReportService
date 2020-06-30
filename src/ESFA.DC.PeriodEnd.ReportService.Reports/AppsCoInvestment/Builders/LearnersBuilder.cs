using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders
{
    public class LearnersBuilder : ILearnersBuilder
    {
        public IDictionary<string, Learner> BuildLearnerLookUpDictionary(ICollection<Learner> learners)
        {
            return learners
                .ToDictionary(k => k.LearnRefNumber, v => v, StringComparer.OrdinalIgnoreCase);
        }

        public ICollection<AppsCoInvestmentRecordKey> GetUniqueAppsCoInvestmentRecordKeysAsync(ICollection<Learner> learners)
        {
            return learners.SelectMany(x => x.LearningDeliveries)
                .GroupBy(ld =>
                    new AppsCoInvestmentRecordKey(
                        ld.LearnRefNumber,
                        ld.LearnStartDate,
                        ld.ProgType ?? 0,
                        ld.StdCode ?? 0,
                        ld.FworkCode ?? 0,
                        ld.PwayCode ?? 0)
                )
                .Select(g => g.Key)
                .ToList();
        }

        public Learner GetLearnerForRecord(IDictionary<string, Learner> learnerDictionary, AppsCoInvestmentRecordKey record)
        {
            if (learnerDictionary.TryGetValue(record.LearnerReferenceNumber, out var result))
            {
                return result;
            }

            return null;
        }

        public int? GetEmploymentStatus(Learner learner, DateTime? learnStartDate)
        {
            return learner?.LearnerEmploymentStatuses.Where(w => w.DateEmpStatApp <= learnStartDate).OrderByDescending(o => o.DateEmpStatApp).FirstOrDefault()?.EmpId;
        }
    }
}
