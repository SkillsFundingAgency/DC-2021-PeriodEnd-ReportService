using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport.Eas;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class ReferenceDataVersion
    {
        public EmployersVersion Employers { get; set; }

        public LarsVersion LarsVersion { get; set; }

        public OrganisationsVersion OrganisationsVersion { get; set; }

        public PostcodesVersion PostcodesVersion { get; set; }

        public EasUploadDateTime EasUploadDateTime { get; set; }
    }
}