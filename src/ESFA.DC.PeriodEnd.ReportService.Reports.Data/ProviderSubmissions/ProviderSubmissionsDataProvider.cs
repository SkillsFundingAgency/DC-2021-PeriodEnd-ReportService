using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.ProviderSubmissions
{
    public class ProviderSubmissionsDataProvider : IProviderSubmissionsDataProvider
    {
        private readonly IJobManagementDataProvider _jobManagementDataProvider;
        private readonly IIlrDataProvider _ilrDataProvider;
        private readonly IOrganisationDataProvider _organisationDataProvider;

        public ProviderSubmissionsDataProvider(IJobManagementDataProvider jobManagementDataProvider, IIlrDataProvider ilrDataProvider, IOrganisationDataProvider organisationDataProvider)
        {
            _jobManagementDataProvider = jobManagementDataProvider;
            _ilrDataProvider = ilrDataProvider;
            _organisationDataProvider = organisationDataProvider;
        }

        public async Task<ProviderSubmissionsReferenceData> ProvideAsync(IReportServiceContext reportServiceContext)
        {
            var collectionId = await _jobManagementDataProvider.ProvideCollectionIdAsync($"ILR{reportServiceContext.CollectionYear}");

            var providerReturnsTask = _jobManagementDataProvider.ProvideReturnersAndPeriodsAsync(collectionId, reportServiceContext.ReturnPeriod);

            var expectedReturnersTask = _jobManagementDataProvider.ProvideExpectedReturnersUKPRNsAsync(collectionId);

            var actualReturnersTask = _jobManagementDataProvider.ProvideActualReturnersUKPRNsAsync(collectionId, reportServiceContext.ReturnPeriod);

            await Task.WhenAll(
                providerReturnsTask,
                expectedReturnersTask,
                actualReturnersTask);

            var fileDetails = await _ilrDataProvider.ProvideAsync(providerReturnsTask.Result);

            var ukprns = providerReturnsTask.Result.Select(x => x.Ukprn).Union(expectedReturnersTask.Result.Select(x => x.Ukprn)).ToList();

            var orgDetails = await _organisationDataProvider.ProvideAsync(ukprns);

            return new ProviderSubmissionsReferenceData
            {
                ProviderReturns = providerReturnsTask.Result,
                FileDetails = fileDetails,
                OrgDetails = orgDetails,
                ExpectedReturns = expectedReturnersTask.Result,
                ActualReturners = actualReturnersTask.Result,
                ILRPeriodsAdjustedTimes = reportServiceContext.ILRPeriodsAdjustedTimes.ToList(),
                ReturnPeriod = reportServiceContext.ReturnPeriod
            };
        }
    }
}
