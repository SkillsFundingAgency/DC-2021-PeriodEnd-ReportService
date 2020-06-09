using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthlyModelBuilder
    {
        private const string NoContract = "No contract";

        private IDictionary<string, string> _fundingStreamPeriodLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017LevyContract1618] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceLevyFunding1618] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017LevyContract19Plus] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceLevyFunding19Plus] = FundingStreamPeriodCodeConstants.LEVY1799,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContract1618] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContractNonProcured1618] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipNonLevyContractProcured1618] = FundingStreamPeriodCodeConstants.C1618NLAP2018,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceNonLevyFunding1618] = FundingStreamPeriodCodeConstants.NONLEVY2019,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContract19Plus] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipFromMay2017NonLevyContractNonProcured19Plus] = FundingStreamPeriodCodeConstants.APPS2021,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipNonLevyContractProcured19Plus] = FundingStreamPeriodCodeConstants.ANLAP2018,
            [ReportingAimFundingLineTypeConstants.ApprenticeshipEmployerOnAppServiceNonLevyFunding19Plus] = FundingStreamPeriodCodeConstants.NONLEVY2019,
        };

        public IEnumerable<AppsMonthlyRecord> Build(
            ICollection<Payment> payments,
            ICollection<Learner> learners,
            ICollection<ContractAllocation> contractAllocations,
            ICollection<Earning> earnings,
            ICollection<LarsLearningDelivery> larsLearningDeliveries,
            ICollection<AecApprenticeshipPriceEpisode> priceEpisodes)
        {
            var contractNumberLookup = BuildContractNumbersLookup(contractAllocations);
            var learnerLookup = BuildLearnerLookup(learners);
            var earningsLookup = BuildEarningsLookup(earnings);
            var larsLearningDeliveryLookup = BuildLarsLearningDeliveryTitleLookup(larsLearningDeliveries);
            var priceEpisodesLookup = BuildPriceEpisodesLookup(priceEpisodes);

            return payments
                .GroupBy(
                    p =>
                        new RecordKey(p.LearnerReferenceNumber,
                            p.LearnerUln,
                            p.LearningAimReference,
                            p.LearningStartDate,
                            p.LearningAimProgrammeType,
                            p.LearningAimStandardCode,
                            p.LearningAimFrameworkCode,
                            p.LearningAimPathwayCode,
                            p.ReportingAimFundingLineType,
                            p.PriceEpisodeIdentifier))
                .Select(k =>
                {
                    var learner = learnerLookup.GetValueOrDefault(k.Key.LearnerReferenceNumber);
                    var learningDelivery = GetLearnerLearningDeliveryForRecord(learner, k.Key);

                    var fundingStreamPeriod = FundingStreamPeriodForReportingAimFundingLineType(k.Key.ReportingAimFundingLineType);

                    var contractNumber = contractNumberLookup.GetValueOrDefault(fundingStreamPeriod, NoContract);

                    var earning = GetEarningForRecord(k.Key, k, payments, earningsLookup);

                    var providerSpecLearnMonitorings = BuildProviderMonitorings(learner, learningDelivery);

                    var learningDeliveryTitle = larsLearningDeliveryLookup.GetValueOrDefault(k.Key.LearningAimReference);

                    var learningDeliveryFams = BuildLearningDeliveryFamsForLearningDelivery(learningDelivery);

                    var priceEpisode = priceEpisodesLookup.GetValueOrDefault(k.Key.PriceEpisodeIdentifier);

                    return new AppsMonthlyRecord()
                    {
                        RecordKey = k.Key,
                        Learner = learner,
                        LearningDelivery = learningDelivery,
                        ContractNumber = contractNumber,
                        Earning = earning,
                        ProviderSpecLearnMonitorings = providerSpecLearnMonitorings,
                        LearningDeliveryTitle = learningDeliveryTitle,
                        LearningDeliveryFams = learningDeliveryFams,
                        PriceEpisode = priceEpisode,
                    };
                });
        }
        
        public LearningDelivery GetLearnerLearningDeliveryForRecord(Learner learner, RecordKey recordKey)
        {
            // BR2 - UKPRN and LearnRefNumber is implicitly matched, not included on models
            return learner?
                .LearningDeliveries?
                .FirstOrDefault(ld =>
                    ld.ProgType == recordKey.ProgrammeType
                    && ld.StdCode == recordKey.StandardCode
                    && ld.FworkCode == recordKey.FrameworkCode
                    && ld.PwayCode == recordKey.PathwayCode
                    && ld.LearnStartDate == recordKey.LearnStartDate
                    && ld.LearnAimRef.CaseInsensitiveEquals(recordKey.LearningAimReference));
        }

        public string FundingStreamPeriodForReportingAimFundingLineType(string reportingAimFundingLineType) 
            => _fundingStreamPeriodLookup.GetValueOrDefault(reportingAimFundingLineType);

        public IDictionary<string, Learner> BuildLearnerLookup(IEnumerable<Learner> learners) 
            => learners.ToDictionary(l => l.LearnRefNumber, l => l, StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> BuildContractNumbersLookup(IEnumerable<ContractAllocation> contractAllocations)
        {
            return contractAllocations
                .GroupBy(ca => ca.FundingStreamPeriod, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    k => k.Key,
                    v => string.Join("; ", v.Select(ca => ca.ContractAllocationNumber).OrderBy(ca => ca)));
        }

        public IDictionary<Guid, Earning> BuildEarningsLookup(IEnumerable<Earning> earnings) 
            => earnings.ToDictionary(e => e.EventId, e => e);

        public IDictionary<string, string> BuildLarsLearningDeliveryTitleLookup(IEnumerable<LarsLearningDelivery> larsLearningDeliveries) 
            => larsLearningDeliveries.ToDictionary(ld => ld.LearnAimRef, ld => ld.LearnAimRefTitle, StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, AecApprenticeshipPriceEpisode> BuildPriceEpisodesLookup(IEnumerable<AecApprenticeshipPriceEpisode> priceEpisodes)
            => priceEpisodes.ToDictionary(e => e.PriceEpisodeIdentifier, e => e, StringComparer.OrdinalIgnoreCase); 

        public Payment GetLatestPaymentWithEarningEvent(IEnumerable<Payment> payments)
        {
            return payments
                .Where(p => p.EarningEventId.HasValue && p.EarningEventId != Guid.Empty)
                .OrderByDescending(p => p.CollectionPeriod)
                .ThenByDescending(p => p.DeliveryPeriod)
                .FirstOrDefault();
        }

        public Payment GetLatestRefundPaymentWithEarningEventForRecord(RecordKey recordKey, IEnumerable<Payment> payments)
        {
            // Intentionally ignored Learn Start Date on the key, Refunds don't have a Learn Start Date
            return payments.Where(p =>
                    p.EarningEventId.HasValue
                    && p.EarningEventId != Guid.Empty
                    && p.LearningAimProgrammeType == recordKey.ProgrammeType
                    && p.LearningAimStandardCode == recordKey.StandardCode
                    && p.LearningAimFrameworkCode == recordKey.FrameworkCode
                    && p.LearningAimPathwayCode == recordKey.PathwayCode
                    && p.LearnerReferenceNumber.CaseInsensitiveEquals(recordKey.LearnerReferenceNumber)
                    && p.LearningAimReference.CaseInsensitiveEquals(recordKey.LearningAimReference))
                .OrderByDescending(p => p.CollectionPeriod)
                .ThenByDescending(p => p.DeliveryPeriod)
                .FirstOrDefault();
        }

        public Earning GetEarningForPayment(IDictionary<Guid, Earning> earningsLookup, Payment payment) 
            => payment?.EarningEventId != null ? earningsLookup.GetValueOrDefault(payment.EarningEventId.Value) : null;

        public Earning GetEarningForRecord(RecordKey recordKey, IEnumerable<Payment> paymentsInRow, IEnumerable<Payment> allPayments, IDictionary<Guid, Earning> earningsLookup)
        {
            var latestPaymentWithEarningEvent = 
                GetLatestPaymentWithEarningEvent(paymentsInRow)
                ?? GetLatestRefundPaymentWithEarningEventForRecord(recordKey, allPayments);

            return GetEarningForPayment(earningsLookup, latestPaymentWithEarningEvent);
        }

        public ProviderMonitorings BuildProviderMonitorings(Learner learner, LearningDelivery learningDelivery)
        {
            if (learner?.ProviderSpecLearnMonitorings == null && learningDelivery?.ProviderSpecDeliveryMonitorings == null)
            {
                return null;
            }

            var providerMonitorings = new ProviderMonitorings();

            if (learner?.ProviderSpecLearnMonitorings != null)
            {
                providerMonitorings.LearnerA = GetProviderMonForOccur(learner.ProviderSpecLearnMonitorings, ProviderMonitorings.OccurA);
                providerMonitorings.LearnerB = GetProviderMonForOccur(learner.ProviderSpecLearnMonitorings, ProviderMonitorings.OccurB);
            }

            if (learningDelivery?.ProviderSpecDeliveryMonitorings != null)
            {
                providerMonitorings.LearningDeliveryA = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurA);
                providerMonitorings.LearningDeliveryB = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurB);
                providerMonitorings.LearningDeliveryC = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurC);
                providerMonitorings.LearningDeliveryD = GetProviderMonForOccur(learningDelivery.ProviderSpecDeliveryMonitorings, ProviderMonitorings.OccurD);
            }

            return providerMonitorings;

        }

        public LearningDeliveryFams BuildLearningDeliveryFamsForLearningDelivery(LearningDelivery learningDelivery)
        {
            if (learningDelivery?.LearningDeliveryFams != null)
            {
                var ldmFams = GetLearnDelFamCodesOfType(learningDelivery.LearningDeliveryFams, LearnDelFamTypeConstants.LDM, 6);
                
                return new LearningDeliveryFams()
                {
                    LDM1 = ldmFams[0],
                    LDM2 = ldmFams[1],
                    LDM3 = ldmFams[2],
                    LDM4 = ldmFams[3],
                    LDM5 = ldmFams[4],
                    LDM6 = ldmFams[5],
                };
            }

            return null;
        }

        private string[] GetLearnDelFamCodesOfType(IEnumerable<LearningDeliveryFam> learningDeliveryFams, string type, int count)
        {
            return learningDeliveryFams
                .Where(f => f.Type.CaseInsensitiveEquals(type))
                .Select(f => f.Code)
                .ToFixedLengthArray(count);
        }

        private string GetProviderMonForOccur(IEnumerable<ProviderMonitoring> providerSpecLearnMons, string occur)
            => providerSpecLearnMons?.FirstOrDefault(m => m.Occur.CaseInsensitiveEquals(occur))?.Mon;
    }
}
