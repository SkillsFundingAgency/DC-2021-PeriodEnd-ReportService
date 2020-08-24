using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.ProviderSubmissions
{
    public class ProviderSubmissionsModelBuilder : IProviderSubmissionsModelBuilder
    {
        public ICollection<ProviderSubmissionModel> Build(ProviderSubmissionsReferenceData referenceData)
        {
            ReturnPeriod period = referenceData.ILRPeriodsAdjustedTimes.Single(x => x.PeriodNumber == referenceData.ReturnPeriod);

            var organisationNameLookup = referenceData.OrgDetails.ToDictionary(o => o.Ukprn, o => o.Name);
            var fileDetailsLookup = referenceData.FileDetails.ToDictionary(f => f.Filename, f => f, StringComparer.OrdinalIgnoreCase);
            var actualReturnersLookup = new HashSet<long>(referenceData.ActualReturners);
            var modelUkprns = new HashSet<long>(referenceData.FileDetails.Select(m => m.Ukprn));

            var providerSubmissionModels = referenceData.ProviderReturns
                .Select(p =>
                {
                    var ukprn = p.Ukprn;

                    var providerSubmissionModel = new ProviderSubmissionModel();

                    providerSubmissionModel.Ukprn = p.Ukprn;
                    providerSubmissionModel.ReturnPeriod = p.ReturnPeriod;

                    providerSubmissionModel.Expected = referenceData.ExpectedReturns.Any(x => x.Ukprn == ukprn && ReturnExpected(x, period.StartDateTimeUtc, period.EndDateTimeUtc));
                    providerSubmissionModel.Returned = actualReturnersLookup.Contains(ukprn);
                    providerSubmissionModel.LatestReturn = $"R{providerSubmissionModel.ReturnPeriod.ToString().PadLeft(2, '0')}";
                    providerSubmissionModel.Returned = providerSubmissionModel.ReturnPeriod == referenceData.ReturnPeriod;

                    providerSubmissionModel.Name = organisationNameLookup.GetValueOrDefault(ukprn);

                    var fileDetails = fileDetailsLookup.GetValueOrDefault(p.FileName.Replace(".ZIP", ".XML"));

                    if (fileDetails != null)
                    {
                        providerSubmissionModel.SubmittedDateTime = fileDetails.SubmittedTime;
                        providerSubmissionModel.TotalErrors = fileDetails.TotalErrorCount;
                        providerSubmissionModel.TotalInvalid = fileDetails.TotalInvalidLearnersSubmitted;
                        providerSubmissionModel.TotalValid = fileDetails.TotalValidLearnersSubmitted;
                        providerSubmissionModel.TotalWarnings = fileDetails.TotalWarningCount;
                    }

                    return providerSubmissionModel;
                }).ToList();

            foreach (var org in referenceData.ExpectedReturns.Where(o => !modelUkprns.Contains(o.Ukprn)))
            {
                providerSubmissionModels.Add(new ProviderSubmissionModel
                {
                    Expected = ReturnExpected(org, period.StartDateTimeUtc, period.EndDateTimeUtc),
                    LatestReturn = string.Empty,
                    Name = organisationNameLookup.GetValueOrDefault(org.Ukprn),
                    Returned = false,
                    SubmittedDateTime = DateTime.MinValue,
                    TotalErrors = 0,
                    TotalInvalid = 0,
                    TotalValid = 0,
                    TotalWarnings = 0,
                    Ukprn = org.Ukprn
                });
            }

            return providerSubmissionModels;
        }

        private bool ReturnExpected(OrganisationCollection organisationCollectionModel, DateTime periodStart, DateTime periodEnd)
        {
            if (!organisationCollectionModel.End.HasValue)
            {
                return true;
            }

            return organisationCollectionModel.End > periodStart
                   && (organisationCollectionModel.Start == null || organisationCollectionModel.Start < periodEnd);
        }
    }
}
