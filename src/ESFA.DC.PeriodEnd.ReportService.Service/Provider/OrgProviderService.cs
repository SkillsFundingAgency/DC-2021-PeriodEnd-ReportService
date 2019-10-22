using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.ReferenceData.Organisations.Model;
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

        public async Task<List<OrgDetail>> GetOrgDetailsForUKPRNsAsync(
            List<long> uKPRNs,
            CancellationToken cancellationToken)
        {
            if ((uKPRNs?.Count ?? 0) == 0)
            {
                return null;
            }

            using (var orgContext = _orgContextFactory())
            {
                return await orgContext.OrgDetails
                    .Join(uKPRNs, o => o.Ukprn, u => u, (o, u) => o)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
