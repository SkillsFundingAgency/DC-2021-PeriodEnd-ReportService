using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders
{
    public class PaymentsBuilder : IPaymentsBuilder
    {
        private readonly IEqualityComparer<AppsCoInvestmentRecordKey> _appsCoInvestmentEqualityComparer;

        public PaymentsBuilder(IEqualityComparer<AppsCoInvestmentRecordKey> appsCoInvestmentEqualityComparer)
        {
            _appsCoInvestmentEqualityComparer = appsCoInvestmentEqualityComparer;
        }

        public IDictionary<AppsCoInvestmentRecordKey, List<Payment>> BuildPaymentsLookupDictionary(ICollection<Payment> payments)
        {
            return payments
                .GroupBy(
                    p => new AppsCoInvestmentRecordKey(p.LearnerReferenceNumber, p.LearningStartDate, p.LearningAimProgrammeType, p.LearningAimStandardCode, p.LearningAimFrameworkCode, p.LearningAimPathwayCode), _appsCoInvestmentEqualityComparer)
                .ToDictionary(k => k.Key, v => v.ToList(), _appsCoInvestmentEqualityComparer);
        }

        public ICollection<AppsCoInvestmentRecordKey> GetUniqueCombinationsOfKeyFromPaymentsAsync(ICollection<Payment> payments)
        {
            return payments
                .GroupBy(p =>
                    new AppsCoInvestmentRecordKey(
                        p.LearnerReferenceNumber,
                        p.LearningStartDate,
                        p.LearningAimProgrammeType,
                        p.LearningAimStandardCode,
                        p.LearningAimFrameworkCode,
                        p.LearningAimPathwayCode))
                .Select(
                    g =>
                        new AppsCoInvestmentRecordKey(
                            g.Key.LearnerReferenceNumber,
                            g.Key.LearningStartDate,
                            g.Key.ProgrammeType,
                            g.Key.StandardCode,
                            g.Key.FrameworkCode,
                            g.Key.PathwayCode))
                .ToList();
        }

        public IEnumerable<Payment> GetPaymentsForRecord(IDictionary<AppsCoInvestmentRecordKey, List<Payment>> paymentsDictionary, AppsCoInvestmentRecordKey record)
        {
            if (paymentsDictionary.TryGetValue(record, out var result))
            {
                return result;
            }

            return Enumerable.Empty<Payment>();
        }

        public EarningsAndPayments BuildEarningsAndPayments(IEnumerable<Payment> filteredPayments, IEnumerable<Payment> allPayments, LearningDelivery learningDelivery, ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecPriceEpisodePeriodisedValues, int currentAcademicYear, DateTime academicYearStart, DateTime nextAcademicYearStart)
        {
            var filteredPaymentsList = filteredPayments.ToList();
            var totalsByPeriodDictionary = BuildCoInvestmentPaymentsPerPeriodDictionary(filteredPaymentsList, currentAcademicYear);
            var totalDueCurrentYear = totalsByPeriodDictionary.Sum(d => d.Value);
            var totalDuePreviousYear = TotalCoInvestmentDueFromEmployerInPreviousFundingYears(filteredPaymentsList, currentAcademicYear);
            var totalCollectedCurrentYear = GetTotalPmrBetweenDates(learningDelivery, academicYearStart, nextAcademicYearStart);
            var totalCollectedPreviousYear = GetTotalPmrBetweenDates(learningDelivery, null, academicYearStart);

            var coInvestmentPaymentsFromEmployer = new CoInvestmentPaymentsDueFromEmployer()
            {
                August = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 1),
                September = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 2),
                October = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 3),
                November = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 4),
                December = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 5),
                January = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 6),
                February = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 7),
                March = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 8),
                April = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 9),
                May = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 10),
                June = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 11),
                July = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 12),
                R13 = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 13),
                R14 = GetPeriodisedValueFromDictionaryForPeriod(totalsByPeriodDictionary, 14)
            };

            var earningsAndPayments = new EarningsAndPayments()
            {
                TotalPMRPreviousFundingYears = totalCollectedPreviousYear,
                TotalCoInvestmentDueFromEmployerInPreviousFundingYears = totalDuePreviousYear,
                TotalPMRThisFundingYear = totalCollectedCurrentYear,
                TotalCoInvestmentDueFromEmployerThisFundingYear = totalDueCurrentYear,
                EmployerCoInvestmentPercentage = GetEmployerCoInvestmentPercentage(filteredPaymentsList),
                PercentageOfCoInvestmentCollected = GetPercentageOfInvestmentCollected(totalDueCurrentYear, totalDuePreviousYear, totalCollectedCurrentYear, totalCollectedPreviousYear),
                CompletionEarningThisFundingYear = CalculateCompletionEarningsThisFundingYear(learningDelivery, aecPriceEpisodePeriodisedValues),
                CompletionPaymentsThisFundingYear = CalculateCompletionPaymentsInAcademicYear(allPayments, currentAcademicYear),
                CoInvestmentPaymentsDueFromEmployer = coInvestmentPaymentsFromEmployer
            };

            return earningsAndPayments;
        }

        public IEnumerable<Payment> FilterByFundingSourceAndTransactionType(IEnumerable<Payment> payments, int fundingSource, HashSet<int> transactionTypes)
        {
            return payments.Where(p => p.FundingSource == fundingSource && transactionTypes.Contains(p.TransactionType));
        }

        public decimal CalculateCompletionEarningsThisFundingYear(LearningDelivery learningDelivery, ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecApprenticeshipPriceEpisodePeriodisedValues)
        {
            if (learningDelivery != null)
            {
                return aecApprenticeshipPriceEpisodePeriodisedValues?
                           .Where(p =>
                               p.LearnRefNumber == learningDelivery.LearnRefNumber
                               && p.AimSeqNumber == learningDelivery.AimSeqNumber
                               && p.Periods != null)
                           .SelectMany(p => p.Periods)
                           .Sum()
                       ?? 0;
            }

            return 0;
        }

        public decimal CalculateCompletionPaymentsInAcademicYear(IEnumerable<Payment> payments, int currentAcademicYear)
        {
            var paymentsList = payments.Where(p => p.AcademicYear == currentAcademicYear && p.TransactionType == DASPayments.TransactionType.Completion).ToList();
            return paymentsList.Sum(r => r.Amount);
        }

        public decimal TotalCoInvestmentDueFromEmployerInPreviousFundingYears(IEnumerable<Payment> payments, int currentAcademicYear)
        {
            return payments.Where(p => p.AcademicYear < currentAcademicYear).Sum(p => p.Amount);
        }

        public IDictionary<byte, decimal> BuildCoInvestmentPaymentsPerPeriodDictionary(IEnumerable<Payment> payments, int currentAcademicYear)
        {
            return payments?
                .Where(p => p.AcademicYear == currentAcademicYear)
                .GroupBy(p => p.CollectionPeriod)
                .ToDictionary(p => p.Key, p => p.Sum(i => i.Amount));
        }

        public Payment GetEarliestPayment(IEnumerable<Payment> payments)
        {
            return payments?
                .OrderBy(p => p.AcademicYear)
                .ThenBy(p => p.DeliveryPeriod)
                .ThenBy(p => p.CollectionPeriod)
                .FirstOrDefault();
        }

        public decimal? GetEmployerCoInvestmentPercentage(IEnumerable<Payment> payments)
        {
            var paymentsList = payments.ToList();
            if (!paymentsList.Any() || paymentsList.All(p => p.Amount == 0))
            {
                return null;
            }

            return (1 - paymentsList
                        .GroupBy(g => new { g.DeliveryPeriod, g.AcademicYear })
                        .Select(s => new { AggAmount = s.Sum(a => a.Amount), SFaContrib = s.Min(m => m.SfaContributionPercentage) })
                        .Where(w => w.AggAmount != 0)
                        .Select(s => s.SFaContrib)
                        .DefaultIfEmpty()
                        .Min())
                   * 100;
        }

        public decimal GetPeriodisedValueFromDictionaryForPeriod(IDictionary<byte, decimal> periodisedDictionary, byte period)
        {
            if (periodisedDictionary.TryGetValue(period, out decimal value))
            {
                return value;
            }

            return decimal.Zero;
        }

        public decimal? GetTotalPmrBetweenDates(LearningDelivery learningDelivery, DateTime? startDate, DateTime? endDate)
        {
            var pmrsQuery = learningDelivery?.AppFinRecords ?? Enumerable.Empty<AppFinRecord>();

            if (startDate.HasValue)
            {
                pmrsQuery = pmrsQuery.Where(r => r.AFinDate >= startDate);
            }

            if (endDate.HasValue)
            {
                pmrsQuery = pmrsQuery.Where(r => r.AFinDate < endDate);
            }

            pmrsQuery = pmrsQuery.Where(r => r.AFinType.CaseInsensitiveEquals(FinTypes.PMR));

            var pmrs = pmrsQuery.ToList();

            var positive = pmrs.Where(p => p.AFinCode == 1 || p.AFinCode == 2).Sum(p => p.AFinAmount);
            var negative = pmrs.Where(p => p.AFinCode == 3).Sum(p => p.AFinAmount);

            return positive - negative;
        }

        public decimal GetPercentageOfInvestmentCollected(decimal? totalDueCurrentYear, decimal? totalDuePreviousYear, decimal? totalCollectedCurrentYear, decimal? totalCollectedPreviousYear)
        {
            const decimal maxPercent = 99999.99M;
            const decimal minPercent = -99999.99M;

            var totalDue = (totalDuePreviousYear ?? 0) + (totalDueCurrentYear ?? 0);

            if (totalDue == 0)
            {
                return 0;
            }

            var totalCollected = (totalCollectedPreviousYear ?? 0) + (totalCollectedCurrentYear ?? 0);
            var percent = (totalCollected / totalDue) * 100;
            percent = percent > maxPercent ? maxPercent : percent;
            percent = percent < minPercent ? minPercent : percent;

            return percent;
        }

    }
}
