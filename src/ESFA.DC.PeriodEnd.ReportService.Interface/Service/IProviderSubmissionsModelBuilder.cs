using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Model.Org;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IProviderSubmissionsModelBuilder
    {
        IEnumerable<ProviderSubmissionModel> BuildModel(
            List<ProviderSubmissionModel> models,
            IEnumerable<OrgModel> orgDetails,
            List<OrganisationCollectionModel> expectedReturners,
            IEnumerable<long> actualReturners,
            IEnumerable<ReturnPeriod> periods,
            int currentPeriod);
    }
}
