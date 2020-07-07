using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class AppsMonthlyModelBuilder : IAppsMonthlyModelBuilder
    {
        private const string NoContract = "No contract";
        private const string NotApplicable = "Not applicable.  For more information refer to the funding reports guidance.";

        private readonly IDictionary<string, string> _fundingStreamPeriodLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

        private readonly IPaymentPeriodsBuilder _paymentPeriodsBuilder;
        private readonly ILearningDeliveryFamsBuilder _learningDeliveryFamsBuilder;
        private readonly IProviderMonitoringsBuilder _providerMonitoringsBuilder;

        public AppsMonthlyModelBuilder(
            IPaymentPeriodsBuilder paymentPeriodsBuilder,
            ILearningDeliveryFamsBuilder learningDeliveryFamsBuilder,
            IProviderMonitoringsBuilder providerMonitoringsBuilder)
        {
            _paymentPeriodsBuilder = paymentPeriodsBuilder;
            _learningDeliveryFamsBuilder = learningDeliveryFamsBuilder;
            _providerMonitoringsBuilder = providerMonitoringsBuilder;
        }

        public ICollection<AppsMonthlyRecord> Build(
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
                            p.PriceEpisodeIdentifier,
                            p.ContractType))
                .Select(k =>
                {
                    var learner = learnerLookup.GetValueOrDefault(k.Key.LearnerReferenceNumber);
                    var learningDelivery = GetLearnerLearningDeliveryForRecord(learner, k.Key);

                    var fundingStreamPeriod = FundingStreamPeriodForReportingAimFundingLineType(k.Key.ReportingAimFundingLineType);

                    var familyName = GetName(learner, l => l.FamilyName);
                    var givenNames = GetName(learner, l => l.GivenNames);

                    var contractNumber = contractNumberLookup.GetValueOrDefault(fundingStreamPeriod, NoContract);

                    var earning = GetEarningForRecord(k.Key, k, payments, earningsLookup);

                    var providerSpecLearnMonitorings = _providerMonitoringsBuilder.BuildProviderMonitorings(learner, learningDelivery);

                    var learningDeliveryTitle = larsLearningDeliveryLookup.GetValueOrDefault(k.Key.LearningAimReference);

                    var learningDeliveryFams = _learningDeliveryFamsBuilder.BuildLearningDeliveryFamsForLearningDelivery(learningDelivery);

                    var priceEpisode = priceEpisodesLookup.GetValueOrDefault(k.Key.LearnerReferenceNumber).GetValueOrDefault(k.Key.PriceEpisodeIdentifier);

                    var priceEpisodeStartDate = GetPriceEpisodeStartDateForRecord(k.Key);

                    var learnerEmploymentStatus = GetLearnerEmploymentStatus(learner, learningDelivery);

                    var paymentPeriods = _paymentPeriodsBuilder.BuildPaymentPeriods(k);

                    return new AppsMonthlyRecord()
                    {
                        RecordKey = k.Key,
                        Learner = learner,
                        LearningDelivery = learningDelivery,
                        FamilyName = familyName,
                        GivenNames = givenNames,
                        ContractNumber = contractNumber,
                        Earning = earning,
                        ProviderMonitorings = providerSpecLearnMonitorings,
                        LearningDeliveryTitle = learningDeliveryTitle,
                        LearningDeliveryFams = learningDeliveryFams,
                        PriceEpisode = priceEpisode,
                        PriceEpisodeStartDate = priceEpisodeStartDate,
                        LearnerEmploymentStatus = learnerEmploymentStatus,
                        PaymentPeriods = paymentPeriods,
                    };
                })
                .ToList();
        }

        public string GetName(Learner learner, Func<Learner, string> selector)
            => learner == null ? NotApplicable : selector(learner);

        // BR2 - UKPRN and LearnRefNumber is implicitly matched, not included on models
        public LearningDelivery GetLearnerLearningDeliveryForRecord(Learner learner, RecordKey recordKey)
            => learner?
                .LearningDeliveries?
                .FirstOrDefault(ld =>
                    ld.ProgType == recordKey.ProgrammeType
                    && ld.StdCode == recordKey.StandardCode
                    && ld.FworkCode == recordKey.FrameworkCode
                    && ld.PwayCode == recordKey.PathwayCode
                    && ld.LearnStartDate == recordKey.LearnStartDate
                    && ld.LearnAimRef.CaseInsensitiveEquals(recordKey.LearningAimReference));

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
                    v => string.Join("; ", v.Select(ca => ca.ContractAllocationNumber).OrderBy(ca => ca)),
                    StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<Guid, Earning> BuildEarningsLookup(IEnumerable<Earning> earnings) 
            => earnings.ToDictionary(e => e.EventId, e => e);

        public IDictionary<string, string> BuildLarsLearningDeliveryTitleLookup(IEnumerable<LarsLearningDelivery> larsLearningDeliveries) 
            => larsLearningDeliveries.ToDictionary(ld => ld.LearnAimRef, ld => ld.LearnAimRefTitle, StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Dictionary<string, AecApprenticeshipPriceEpisode>> BuildPriceEpisodesLookup(IEnumerable<AecApprenticeshipPriceEpisode> priceEpisodes)
            => priceEpisodes.GroupBy(g => g.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key,
                    v => v
                        .ToDictionary(pe => pe.PriceEpisodeIdentifier, value => value, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

        public Payment GetLatestPaymentWithEarningEvent(IEnumerable<Payment> payments)
            => payments
                .Where(p => p.EarningEventId.HasValue && p.EarningEventId != Guid.Empty)
                .OrderByDescending(p => p.CollectionPeriod)
                .ThenByDescending(p => p.DeliveryPeriod)
                .FirstOrDefault();

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

        public DateTime? GetPriceEpisodeStartDateForRecord(RecordKey record)
        {
            if (record.PriceEpisodeIdentifier == null || record.PriceEpisodeIdentifier.Length < 10 || record.LearningAimReference != LearnAimRefConstants.ZPROG001)
            {
                return null;
            }

            var dateSegment = record.PriceEpisodeIdentifier.Substring(record.PriceEpisodeIdentifier.Length - 10, 10);

            if (DateTime.TryParseExact(dateSegment, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            return null;
        }
        
        public LearnerEmploymentStatus GetLearnerEmploymentStatus(Learner learner, LearningDelivery learningDelivery)
        {
            if (learner?.LearnerEmploymentStatuses == null || learningDelivery == null)
            {
                return null;
            }

            return learner
                .LearnerEmploymentStatuses
                .Where(les => les.DateEmpStatApp <= learningDelivery.LearnStartDate)
                .OrderByDescending(les => les.DateEmpStatApp)
                .FirstOrDefault();
        }
    }
}
