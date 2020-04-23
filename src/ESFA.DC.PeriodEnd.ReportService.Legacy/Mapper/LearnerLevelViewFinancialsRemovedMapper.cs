using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Mapper
{
    public class LearnerLevelViewFinancialsRemovedMapper : ClassMap<LearnerLevelViewModel>
    {
        public LearnerLevelViewFinancialsRemovedMapper()
        {
            int i = 0;

            Map(m => m.PaymentLearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.PaymentUniqueLearnerNumber).Index(i++).Name("Unique learner number");

            Map(m => m.FamilyName).Index(i++).Name("Family Name");
            Map(m => m.GivenNames).Index(i++).Name("Given Names");

            Map(m => m.LearnerEmploymentStatusEmployerId).Index(i++).Name("Employer identifier (ERN)");
            Map(m => m.EmployerName).Index(i++).Name("Latest employer name");

            Map(m => m.ReasonForIssues).Index(i++).Name("Reasons for issues");
            Map(m => m.RuleDescription).Index(i++).Name("Data lock description");

            Map(m => m.PaymentFundingLineType).Index(i++).Name("Funding line type");
        }
    }
}
