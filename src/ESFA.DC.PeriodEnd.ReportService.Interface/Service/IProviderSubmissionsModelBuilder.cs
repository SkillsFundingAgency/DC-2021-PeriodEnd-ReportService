using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.ReferenceData.Organisations.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IProviderSubmissionsModelBuilder
    {
        IEnumerable<ProviderSubmissionModel> BuildModel(
            IEnumerable<FileDetail> fileDetails,
            IEnumerable<OrgDetail> orgDetails,
            IEnumerable<long> expectedReturners,
            IEnumerable<long> actualReturners,
            IEnumerable<ReturnPeriod> returnPeriods);
    }
}
