using System;
using System.Globalization;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Mapper
{
    public class LearnerLevelViewFinancialsRemovedMapper : ClassMap<LearnerLevelViewModel>
    {
        public LearnerLevelViewFinancialsRemovedMapper()
        {
            int i = 0;

            Map(m => m.PaymentLearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.PaymentUniqueLearnerNumber).Index(i++).Name("Unique learner number");

            Map(m => m.PaymentFundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.LearnerEmploymentStatusEmployerId).Index(i++).Name("Employer identifier on employment status date");

            Map(m => m.Ukprn).Index(i++).Name("UKPRN");
            Map(m => m.FamilyName).Index(i++).Name("Family Name");
            Map(m => m.GivenNames).Index(i++).Name("Given Names");

            Map(m => m.RuleDescription).Index(i++).Name("Rule description");
            Map(m => m.ReasonForIssues).Index(i++).Name("Reasons for issues");
        }
    }
}
