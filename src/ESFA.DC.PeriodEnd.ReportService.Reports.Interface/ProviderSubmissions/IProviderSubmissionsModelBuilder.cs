using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions
{
    public interface IProviderSubmissionsModelBuilder
    {
        ICollection<ProviderSubmissionModel> Build(ProviderSubmissionsReferenceData referenceData);
    }
}
