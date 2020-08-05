using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.DataQuality
{
    public class DataQualityModelBuilder : IDataQualityModelBuilder
    {
        public DataQualityProviderModel Build(DataQualityProviderModel providerModel)
        {
            var orgDictionary = providerModel.Organistions.ToDictionary(k => k.Ukprn, v => v);
            providerModel.ReturningProviders = GetReturningProviders(providerModel.FileDetails);
            PopulateOrgNames(providerModel, orgDictionary);

            return providerModel;
        }

        private void PopulateOrgNames(DataQualityProviderModel dataQualityProviderModel, IDictionary<long, Organisation> orgDictionary)
        {
            foreach (var provider in dataQualityProviderModel.ProvidersWithMostInvalidLearners)
            {
                if (orgDictionary.TryGetValue(provider.Ukprn, out var org))
                {
                    provider.OrgName = org.Name;
                    provider.Status = org.Status;
                }
            }

            foreach (var provider in dataQualityProviderModel.ProvidersWithoutValidLearners)
            {
                if (orgDictionary.TryGetValue(provider.Ukprn, out var org))
                {
                    provider.OrgName = org.Name;
                }
            }
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
