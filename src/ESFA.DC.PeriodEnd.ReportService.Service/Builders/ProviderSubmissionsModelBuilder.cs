using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Model.Org;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public sealed class ProviderSubmissionsModelBuilder : IProviderSubmissionsModelBuilder
    {
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;

        public ProviderSubmissionsModelBuilder(IIlrPeriodEndProviderService ilrPeriodEndProviderService)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
        }

        public IEnumerable<ProviderSubmissionModel> BuildModel(
            List<ProviderSubmissionModel> models,
            IEnumerable<OrgModel> orgDetails,
            List<OrganisationCollectionModel> expectedReturners,
            IEnumerable<long> actualReturners,
            IEnumerable<ReturnPeriod> periods,
            int currentPeriod)
        {
            ReturnPeriod period = periods.Single(x => x.PeriodNumber == currentPeriod);

            foreach (ProviderSubmissionModel providerSubmissionModel in models)
            {
                providerSubmissionModel.Expected = expectedReturners.Any(x => x.Ukprn == providerSubmissionModel.Ukprn && x.Expected(period.StartDateTimeUtc, period.EndDateTimeUtc));
                providerSubmissionModel.Returned = actualReturners.Any(x => x == providerSubmissionModel.Ukprn);
                GetLatestReturn(providerSubmissionModel, periods, currentPeriod);
            }

            foreach (var org in expectedReturners)
            {
                if (models.Any(x => x.Ukprn == org.Ukprn))
                {
                    continue;
                }

                models.Add(new ProviderSubmissionModel
                {
                    Expected = org.Expected(period.StartDateTimeUtc, period.EndDateTimeUtc),
                    LatestReturn = string.Empty,
                    Name = string.Empty,
                    Returned = false,
                    SubmittedDateTime = DateTime.MinValue,
                    TotalErrors = 0,
                    TotalInvalid = 0,
                    TotalValid = 0,
                    TotalWarnings = 0,
                    Ukprn = org.Ukprn
                });
            }

            foreach (var orgDetail in orgDetails)
            {
                models.Single(x => x.Ukprn == orgDetail.Ukprn).Name = orgDetail.Name;
            }

            return models;
        }

        private void GetLatestReturn(ProviderSubmissionModel providerSubmissionModel, IEnumerable<ReturnPeriod> returnPeriods, int collectionPeriod)
        {
            int returnPeriod = returnPeriods
                                   .SingleOrDefault(x => x.StartDateTimeUtc <= providerSubmissionModel.SubmittedDateTime && x.EndDateTimeUtc >= providerSubmissionModel.SubmittedDateTime)
                                   ?.PeriodNumber ?? 0;

            providerSubmissionModel.LatestReturn = $"R{returnPeriod.ToString().PadLeft(2, '0')}";
            providerSubmissionModel.Returned = returnPeriod == collectionPeriod;
        }
    }
}
