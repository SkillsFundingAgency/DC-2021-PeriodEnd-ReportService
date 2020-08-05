using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;
using Organisation = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model.Organisation;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.DataQuality
{
    public class DataQualityModelBuilder : IDataQualityModelBuilder
    {
        public DataQualityProviderModel Build(DataQualityProviderModel providerModel, IReportServiceContext reportServiceContext)
        {
            var orgDictionary = providerModel.Organistions.ToDictionary(k => k.Ukprn, v => v);
            var rulesDictionary = providerModel.ValidationRules.ToDictionary(k => k.Rulename, v => v.Message, StringComparer.OrdinalIgnoreCase);

            providerModel.ReturningProviders = GetReturningProviders(providerModel.FileDetails);
            PopulateOrgNames(providerModel, orgDictionary);
            PopulateReturnPeriod(providerModel.ProvidersWithMostInvalidLearners, reportServiceContext.ILRPeriodsAdjustedTimes.ToList());
            PopulateErrorMessage(providerModel.RuleViolations, rulesDictionary);

            return providerModel;
        }

        private void PopulateOrgNames(DataQualityProviderModel dataQualityProviderModel, IDictionary<long, Organisation> orgDictionary)
        {
            foreach (var provider in dataQualityProviderModel.ProvidersWithMostInvalidLearners)
            {
                if (orgDictionary.TryGetValue(provider.Ukprn, out var org))
                {
                    provider.Name = org.Name;
                    provider.Status = org.Status;
                }
            }

            foreach (var provider in dataQualityProviderModel.ProvidersWithoutValidLearners)
            {
                if (orgDictionary.TryGetValue(provider.Ukprn, out var org))
                {
                    provider.Name = org.Name;
                }
            }
        }

        private void PopulateErrorMessage(ICollection<RuleStats> ruleStats, IDictionary<string, string> rulesDictionary)
        {
            foreach (var stat in ruleStats)
            {
                rulesDictionary.TryGetValue(stat.RuleName, out var message);

                stat.ErrorMessage = message;
            }
        }

        private void PopulateReturnPeriod(ICollection<ProviderCount> providers, ICollection<ReturnPeriod> returnPeriods)
        {
            foreach (var provider in providers)
            {
                provider.LatestReturn = $"R{CalculateReturnPeriod(provider.SubmittedDateTime, returnPeriods):D2}";
            }
        }

        private int CalculateReturnPeriod(DateTime? submittedDateTime, IEnumerable<ReturnPeriod> returnPeriods)
        {
            return !submittedDateTime.HasValue
                ? 0
                : returnPeriods
                      .SingleOrDefault(x =>
                          submittedDateTime >= x.StartDateTimeUtc &&
                          submittedDateTime <= x.EndDateTimeUtc)
                      ?.PeriodNumber ?? 99;
        }

        private ICollection<DataQualityModel> GetReturningProviders(IEnumerable<FilePeriodInfo> fileDetails)
        {
            List<DataQualityModel> returningProviders = new List<DataQualityModel>();
            var fd = fileDetails.ToList();

            var fds = fd.GroupBy(x => x.UKPRN)
                .Select(x => new
                {
                    Ukrpn = x.Key,
                    Files = x.Select(y => y.Filename).Count()
                });

            returningProviders.Add(new DataQualityModel
            {
                Description = "Total Returning Providers",
                NoOfProviders = fds.Count(),
                NoOfValidFilesSubmitted = fd.Count,
                EarliestValidSubmission = null,
                LastValidSubmission = null
            });

            var fdCs = fd
                .GroupBy(x => new { x.PeriodNumber })
                .Select(x => new
                {
                    PeriodNumber = x.Key.PeriodNumber,
                    Collection = $"R{x.Key.PeriodNumber:D2}",
                    Earliest = x.Min(y => y.SubmittedTime ?? DateTime.MaxValue),
                    Latest = x.Max(y => y.SubmittedTime ?? DateTime.MinValue),
                    Providers = x.Select(y => y.UKPRN).Distinct().Count(),
                    Valid = x.Count()
                })
                .OrderByDescending(x => x.PeriodNumber);

            foreach (var f in fdCs)
            {
                returningProviders.Add(new DataQualityModel
                {
                    Description = "Returning Providers per Period",
                    Collection = f.Collection,
                    NoOfProviders = f.Providers,
                    NoOfValidFilesSubmitted = f.Valid,
                    EarliestValidSubmission = f.Earliest,
                    LastValidSubmission = f.Latest
                });
            }

            return returningProviders;
        }
    }
}
