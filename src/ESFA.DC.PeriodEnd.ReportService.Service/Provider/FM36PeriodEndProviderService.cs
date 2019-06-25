using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class FM36PeriodEndProviderService : AbstractFundModelProviderService, IFM36PeriodEndProviderService
    {
        private readonly Func<IIlr1819RulebaseContext> _ilrRulebaseContextFactory;
        private readonly SemaphoreSlim _getDataLock = new SemaphoreSlim(1, 1);
        private bool _loadedDataAlready;
        private FM36Global _fundingOutputs;

        public FM36PeriodEndProviderService(
            ILogger logger,
            IStreamableKeyValuePersistenceService storage,
            IJsonSerializationService jsonSerializationService,
            Func<IIlr1819RulebaseContext> ilrRulebaseContextFactory)
        : base(storage, jsonSerializationService, logger)
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