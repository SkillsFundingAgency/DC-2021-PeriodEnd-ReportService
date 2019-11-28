using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.EAS1920.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.DataAccess;
using ESFA.DC.ReferenceData.Organisations.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.DataAccess.Services
{
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly Func<IOrganisationsContext> _organisationsContextFunc;
        private readonly Func<IEasdbContext> _easContextFunc;
        private readonly Func<IIlr1920RulebaseContext> _ilrContextFunc;

        public ReferenceDataService(Func<IOrganisationsContext> organisationsContextFunc, Func<IEasdbContext> easContextFunc, Func<IIlr1920RulebaseContext> ilrContextFunc)
        {
            _organisationsContextFunc = organisationsContextFunc;
            _easContextFunc = easContextFunc;
            _ilrContextFunc = ilrContextFunc;
        }

        public async Task<string> GetProviderNameAsync(int ukprn, CancellationToken cancellationToken)
        {
            string providerName;

            cancellationToken.ThrowIfCancellationRequested();

            using (var context = _organisationsContextFunc.Invoke())
            {
                providerName = (await context.OrgDetails
                    .SingleOrDefaultAsync(o => o.Ukprn == ukprn, cancellationToken))
                    ?.Name;
            }

            return providerName;
        }

        public async Task<DateTime?> GetLastestEasSubmissionDateTimeAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _easContextFunc.Invoke())
            {
                return (await context.EasSubmissions.FirstOrDefaultAsync(v => v.Ukprn == ukprn.ToString(), cancellationToken))?.UpdatedOn;
            }
        }

        public async Task<string> GetLatestIlrSubmissionFileNameAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _ilrContextFunc.Invoke())
            {
                return (await context.FileDetails.Where(fd => fd.UKPRN == ukprn).OrderByDescending(f => f.SubmittedTime).FirstOrDefaultAsync(cancellationToken))?.Filename;
            }
        }
    }
}
