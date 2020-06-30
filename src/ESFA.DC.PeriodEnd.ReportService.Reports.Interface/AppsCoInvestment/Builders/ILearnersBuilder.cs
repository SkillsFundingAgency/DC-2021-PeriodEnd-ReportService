using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders
{
    public interface ILearnersBuilder
    {
        IDictionary<string, Learner> BuildLearnerLookUpDictionary(ICollection<Learner> learners);

        ICollection<AppsCoInvestmentRecordKey> GetUniqueAppsCoInvestmentRecordKeysAsync(ICollection<Learner> learners);

        Learner GetLearnerForRecord(IDictionary<string, Learner> learnerDictionary, AppsCoInvestmentRecordKey record);

        int? GetEmploymentStatus(Learner learner, DateTime? learnStartDate);
    }
}
