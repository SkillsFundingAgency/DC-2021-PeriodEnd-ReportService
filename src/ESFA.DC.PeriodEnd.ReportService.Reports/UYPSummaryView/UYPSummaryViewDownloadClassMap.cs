using System;
using System.Linq.Expressions;
using System.Text;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView
{
    public class UYPSummaryViewDownloadClassMap : AbstractClassMap<LearnerLevelViewModel>
    {
        public UYPSummaryViewDownloadClassMap()
        {
            MapIndex(m => m.PaymentLearnerReferenceNumber).Name("Learner reference number");
            MapIndex(m => m.PaymentUniqueLearnerNumbers).Name("Unique learner number");

            MapIndex(m => m.FamilyName).Name("Family Name");
            MapIndex(m => m.GivenNames).Name("Given Names");

            MapIndex(m => m.LearnerEmploymentStatusEmployerId).Name("Employer identifier (ERN)");
            MapIndex(m => m.EmployerName).Name("Latest employer name");

            MapIndex(m => m.ReasonForIssues).Name("Reasons for issues");
            MapIndex(m => m.RuleDescription).Name("Data lock description");

            MapIndex(m => m.PaymentFundingLineType).Name("Funding line type");
        }
    }
}
