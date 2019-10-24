using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.ReferenceData.Organisations.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class ProviderSubmissionsModelBuilder : IProviderSubmissionsModelBuilder
    {
        private readonly IIlrPeriodEndProviderService _ilrPeriodEndProviderService;

        public ProviderSubmissionsModelBuilder(IIlrPeriodEndProviderService ilrPeriodEndProviderService)
        {
            _ilrPeriodEndProviderService = ilrPeriodEndProviderService;
        }

        public IEnumerable<ProviderSubmissionModel> BuildModel(
            IEnumerable<FileDetail> fileDetails,
            IEnumerable<OrgDetail> orgDetails,
            IEnumerable<long> expectedReturners,
            IEnumerable<long> actualReturners,
            IEnumerable<ReturnPeriod> returnPeriods)
        {
            return fileDetails
                .Select(x => new ProviderSubmissionModel
                {
                    Ukprn = x.UKPRN,
                    SubmittedDateTime = x.SubmittedTime.GetValueOrDefault(),
                    TotalErrors = x.TotalErrorCount.GetValueOrDefault(),
                    TotalInvalid = x.TotalInvalidLearnersSubmitted.GetValueOrDefault(),
                    TotalValid = x.TotalValidLearnersSubmitted.GetValueOrDefault(),
                    TotalWarnings = x.TotalWarningCount.GetValueOrDefault(),
                    Name = GetOrgNameForUKPRN(x.UKPRN, orgDetails),
                    Expected = expectedReturners.Any(e => e == x.UKPRN),
                    Returned = actualReturners.Any(e => e == x.UKPRN),
                    LatestReturn = $"R{_ilrPeriodEndProviderService.GetPeriodReturn(x.SubmittedTime, returnPeriods):D2}"
                })
                .ToList();
        }

        private string GetOrgNameForUKPRN(int ukprn, IEnumerable<OrgDetail> orgDetails)
        {
            string name = string.Empty;
            if (orgDetails.Any(o => o.Ukprn == ukprn))
            {
                name = orgDetails.Where(o => o.Ukprn == ukprn).Single().Name;
            }

            return name;
        }
    }
}
