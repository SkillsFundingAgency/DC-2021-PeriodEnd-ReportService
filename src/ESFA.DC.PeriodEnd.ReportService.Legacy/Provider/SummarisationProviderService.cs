using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.Summarisation.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Provider
{
    public sealed class SummarisationProviderService : ISummarisationProviderService
    {
        private readonly Func<ISummarisationContext> _summarisationContextFunc;

        public SummarisationProviderService(Func<ISummarisationContext> summarisationContextFunc)
        {
            _summarisationContextFunc = summarisationContextFunc;
        }

        public async Task<IEnumerable<DataExtractModel>> GetSummarisedActualsForDataExtractReport(
            string collectionType,
            IReadOnlyCollection<string> collectionReturnCodes,
            CancellationToken cancellationToken)
        {
            using (ISummarisationContext summarisationContext = _summarisationContextFunc())
            {
                return await summarisationContext.SummarisedActuals
                    .Include(x => x.CollectionReturn)
                    .Where(x => (x.CollectionReturn.CollectionType == collectionType || x.CollectionReturn.CollectionType == "ESF" || x.CollectionReturn.CollectionType == "APPS")
                                && collectionReturnCodes.Contains(x.CollectionReturn.CollectionReturnCode))
                    .Select(x => new DataExtractModel
                    {
                        Id = x.ID,
                        ActualValue = x.ActualValue,
                        ActualVolume = x.ActualVolume,
                        CollectionReturnCode = x.CollectionReturn.CollectionReturnCode,
                        CollectionType = x.CollectionReturn.CollectionType,
                        ContractAllocationNumber = x.ContractAllocationNumber,
                        DeliverableCode = x.DeliverableCode,
                        FundingStreamPeriodCode = x.FundingStreamPeriodCode,
                        OrganisationId = x.OrganisationId,
                        Period = x.Period,
                        PeriodTypeCode = x.PeriodTypeCode,
                        UoPCode = x.UoPCode
                    })
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
