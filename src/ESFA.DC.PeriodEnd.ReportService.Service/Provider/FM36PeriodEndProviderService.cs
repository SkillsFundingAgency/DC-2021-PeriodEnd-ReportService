using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class FM36PeriodEndProviderService : AbstractFundModelProviderService, IFM36PeriodEndProviderService
    {
        private readonly Func<IIlr1920RulebaseContext> _ilrRulebaseContextFactory;

        public FM36PeriodEndProviderService(
            ILogger logger,
            Func<IIlr1920RulebaseContext> ilrRulebaseContextFactory)
        : base(logger)
        {
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
        }

        public async Task<AppsMonthlyPaymentRulebaseInfo> GetFM36DataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentRulebaseInfo = new AppsMonthlyPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                var aecApprenticeshipPriceEpisodeInfos = await ilrContext.AEC_ApprenticeshipPriceEpisodes
                    .Where(x => x.UKPRN == ukPrn)
                    .Select(pe => new AECApprenticeshipPriceEpisodeInfo
                    {
                        UkPrn = pe.UKPRN,
                        AimSequenceNumber = (int)pe.PriceEpisodeAimSeqNumber,
                        LearnRefNumber = pe.LearnRefNumber,
                        PriceEpisodeActualEndDate = pe.PriceEpisodeActualEndDate,
                        PriceEpisodeAgreeId = pe.PriceEpisodeAgreeId
                    }).ToListAsync(cancellationToken);

                appsMonthlyPaymentRulebaseInfo.AECApprenticeshipPriceEpisodes.AddRange(aecApprenticeshipPriceEpisodeInfos);
            }

            return appsMonthlyPaymentRulebaseInfo;
        }
    }
}