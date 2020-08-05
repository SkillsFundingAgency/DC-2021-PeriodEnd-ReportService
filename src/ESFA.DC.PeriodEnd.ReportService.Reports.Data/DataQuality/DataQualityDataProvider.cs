using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.DataProvider;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Data.DataQuality
{
    public class DataQualityDataProvider
    {
        private readonly IJobManagementDataProvider _jobManagementDataProvider;
        private readonly IIlrDataProvider _ilrDataProvider;
        private readonly IOrganisationDataProvider _organisationDataProvider;

        public DataQualityDataProvider(IJobManagementDataProvider jobManagementDataProvider, IIlrDataProvider ilrDataProvider, IOrganisationDataProvider organisationDataProvider)
        {
            _jobManagementDataProvider = jobManagementDataProvider;
            _ilrDataProvider = ilrDataProvider;
            _organisationDataProvider = organisationDataProvider;
        }

        public async Task<DataQualityProviderModel> ProvideAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var ukprns = new List<long>();

            var collectionId = await _jobManagementDataProvider.ProvideCollectionIdAsync($"ILR{reportServiceContext.CollectionYear}");

            var fileDetails = await _jobManagementDataProvider.ProvideFilePeriodInfoForCollectionAsync(collectionId);

            var ruleViolations = await _ilrDataProvider.ProvideTop20RuleViolationsAsync();

            var providersWithoutValidLearners = await _ilrDataProvider.ProvideProvidersWithoutValidLearners(cancellationToken);
            var providersWithMostInvalidLearners = await _ilrDataProvider.ProvideProvidersWithMostInvalidLearners(cancellationToken);

            ukprns.AddRange(providersWithoutValidLearners.Select(x => x.Ukprn));
            ukprns.AddRange(providersWithMostInvalidLearners.Select(x => x.Ukprn));

            var organisations = await _organisationDataProvider.ProvideAsync(ukprns);

            return new DataQualityProviderModel
            {
                CollectionId = collectionId, FileDetails = fileDetails, RuleViolations = ruleViolations,
                ProvidersWithoutValidLearners = providersWithoutValidLearners,
                ProvidersWithMostInvalidLearners = providersWithMostInvalidLearners,
                Organistions = organisations
            };
        }
    }
}
