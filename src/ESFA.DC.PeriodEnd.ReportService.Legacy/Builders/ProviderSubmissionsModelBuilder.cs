using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Builders
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
            IDictionary<long, string> orgDetails,
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
                GetLatestReturn(providerSubmissionModel, currentPeriod);
            }

            foreach (var org in expectedReturners)
            {
                if (models.Any(x => x.Ukprn == org.Ukprn))
                {
                    continue;
                }

                orgDetails.TryGetValue(org.Ukprn, out var orgName);

                models.Add(new ProviderSubmissionModel
                {
                    Expected = org.Expected(period.StartDateTimeUtc, period.EndDateTimeUtc),
                    LatestReturn = string.Empty,
                    Name = orgName ?? string.Empty,
                    Returned = false,
                    SubmittedDateTime = DateTime.MinValue,
                    TotalErrors = 0,
                    TotalInvalid = 0,
                    TotalValid = 0,
                    TotalWarnings = 0,
                    Ukprn = org.Ukprn
                });
            }

            return models;
        }

        private void GetLatestReturn(ProviderSubmissionModel providerSubmissionModel, int collectionPeriod)
        {
            providerSubmissionModel.LatestReturn = $"R{providerSubmissionModel.ReturnPeriod.ToString().PadLeft(2, '0')}";
            providerSubmissionModel.Returned = providerSubmissionModel.ReturnPeriod == collectionPeriod;
        }
    }
}
