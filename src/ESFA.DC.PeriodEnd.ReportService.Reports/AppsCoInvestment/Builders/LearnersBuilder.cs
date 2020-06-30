using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
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
                    new
                    {
                        ld.LearnRefNumber,
                        ld.LearnStartDate,
                        ProgType = ld.ProgType ?? 0,
                        StdCode = ld.StdCode ?? 0,
                        FworkCode = ld.FworkCode ?? 0,
                        PwayCode = ld.PwayCode ?? 0,
                    })
                .Select(
                    g =>
                        new AppsCoInvestmentRecordKey(
                            g.Key.LearnRefNumber,
                            g.Key.LearnStartDate,
                            g.Key.ProgType,
                            g.Key.StdCode,
                            g.Key.FworkCode,
                            g.Key.PwayCode))
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
