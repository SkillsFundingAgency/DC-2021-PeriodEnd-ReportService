using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.ReferenceData.FCS.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class FCSProviderService : IFCSProviderService
    {
        private readonly Func<IFcsContext> _fcsContextFunc;

        public FCSProviderService(Func<IFcsContext> fcsContextFunc)
        {
            _fcsContextFunc = fcsContextFunc;
        }

        public async Task<List<DataExtractFcsInfo>> GetFCSForDataExtractReport(IEnumerable<string> OrganisationIds, CancellationToken cancellationToken)
        {
            using (IFcsContext fcsContext = _fcsContextFunc())
            {
                return await fcsContext.Contractors
                    .Include(x => x.Contracts)
                    .Where(x => OrganisationIds.Contains(x.OrganisationIdentifier, StringComparer.OrdinalIgnoreCase))
                    .GroupBy(x => new { x.OrganisationIdentifier, x.Ukprn })
                    .Select(x => new DataExtractFcsInfo
                    {
                        OrganisationIdentifier = x.Key.OrganisationIdentifier,
                        UkPrn = x.Key.Ukprn
                    }).ToListAsync(cancellationToken);
            }
        }
    }
}
