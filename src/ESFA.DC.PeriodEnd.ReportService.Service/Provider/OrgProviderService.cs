using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.Org;
using ESFA.DC.ReferenceData.Organisations.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class OrgProviderService : IOrgProviderService
    {
        private readonly Func<IOrganisationsContext> _orgContextFactory;

        public OrgProviderService(Func<IOrganisationsContext> orgContextFactory)
        {
            _orgContextFactory = orgContextFactory;
        }

        public async Task<IDictionary<long, string>> GetOrgDetailsDictionaryForUKPRNSAsync(List<long> ukprns, CancellationToken cancellationToken)
        {
            List<OrgModel> orgModels = new List<OrgModel>();

            if ((ukprns?.Count ?? 0) == 0)
            {
                return new Dictionary<long, string>();
            }

            int count = ukprns.Count;
            int pageSize = 1000;

            using (var orgContext = _orgContextFactory())
            {
                for (int i = 0; i < count; i += pageSize)
                {
                    List<OrgModel> orgs = await orgContext.OrgDetails
                        .Where(x => ukprns.Skip(i).Take(pageSize).Contains(x.Ukprn))
                        .Select(x => new OrgModel { Ukprn = x.Ukprn, Name = x.Name, Status = x.Status })
                        .ToListAsync(cancellationToken);

                    orgModels.AddRange(orgs);
                }
            }

            return orgModels.GroupBy(x => x.Ukprn).ToDictionary(x => x.Key, x => x.FirstOrDefault()?.Name);
        }

        public async Task<IEnumerable<OrgModel>> GetOrgDetailsForUKPRNsAsync(
            List<long> uKPRNs,
            CancellationToken cancellationToken)
        {
            List<OrgModel> orgModels = new List<OrgModel>();

            if ((uKPRNs?.Count ?? 0) == 0)
            {
                return orgModels;
            }

            int count = uKPRNs.Count;
            int pageSize = 1000;

            using (var orgContext = _orgContextFactory())
            {
                for (int i = 0; i < count; i += pageSize)
                {
                    List<OrgModel> orgs = await orgContext.OrgDetails
                        .Where(x => uKPRNs.Skip(i).Take(pageSize).Contains(x.Ukprn))
                        .Select(x => new OrgModel { Ukprn = x.Ukprn, Name = x.Name, Status = x.Status })
                        .ToListAsync(cancellationToken);

                    orgModels.AddRange(orgs);
                }
            }

            return orgModels;
        }
    }
}
