using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly
{
    public class PaymentPeriodsBuilder : IPaymentPeriodsBuilder
    {
        private const int PeriodsCount = 14;

        private static IEnumerable<byte> FundingSourceLevyPayments { get; } = new HashSet<byte>() { 1, 5 };
        private static IEnumerable<byte> FundingSourceCoInvestmentPayments { get; } = new HashSet<byte>() { 2 };
        private static IEnumerable<byte> FundingSourceCoInvestmentDueFromEmployer { get; } = new HashSet<byte>() { 3 };
        private static IEnumerable<byte> TransactionTypesLevyPayments { get; } = new HashSet<byte>() { 1, 2, 3 };
        private static IEnumerable<byte> TransactionTypesCoInvestmentPayments { get; } = new HashSet<byte>() { 1, 2, 3 };
        private static IEnumerable<byte> TransactionTypesCoInvestmentDueFromEmployer { get; } = new HashSet<byte>() { 1, 2, 3 };
        private static IEnumerable<byte> TransactionTypesEmployerAdditionalPayments { get; } = new HashSet<byte>() { 4, 6 };
        private static IEnumerable<byte> TransactionTypesProviderAdditionalPayments { get; } = new HashSet<byte>() { 5, 7 };
        private static IEnumerable<byte> TransactionTypesApprenticeshipAdditionalPayments { get; } = new HashSet<byte>() { 16 };
        private static IEnumerable<byte> TransactionTypesEnglishAndMathsPayments { get; } = new HashSet<byte>() { 13, 14 };
        private static IEnumerable<byte> TransactionTypesLearningSupportPayments { get; } = new HashSet<byte>() { 8, 9, 10, 11, 12, 15 };

        public PaymentPeriods BuildPaymentPeriods(IEnumerable<Payment> payments)
        {
            var periodisedPaymentPeriodLines = BuildPeriodisedPaymentPeriods(payments, PeriodsCount);

            return new PaymentPeriods
            {
                R01 = periodisedPaymentPeriodLines[0],
                R02 = periodisedPaymentPeriodLines[1],
                R03 = periodisedPaymentPeriodLines[2],
                R04 = periodisedPaymentPeriodLines[3],
                R05 = periodisedPaymentPeriodLines[4],
                R06 = periodisedPaymentPeriodLines[5],
                R07 = periodisedPaymentPeriodLines[6],
                R08 = periodisedPaymentPeriodLines[7],
                R09 = periodisedPaymentPeriodLines[8],
                R10 = periodisedPaymentPeriodLines[9],
                R11 = periodisedPaymentPeriodLines[10],
                R12 = periodisedPaymentPeriodLines[11],
                R13 = periodisedPaymentPeriodLines[12],
                R14 = periodisedPaymentPeriodLines[13],
                Total = BuildTotalPaymentPeriodLines(periodisedPaymentPeriodLines)
            };
        }

        public PaymentPeriodLines[] BuildPeriodisedPaymentPeriods(IEnumerable<Payment> payments, int periodsCount)
        {
            var periodisedPayments = payments
                .Where(p => p.Amount != 0)
                .GroupBy(p => p.CollectionPeriod)
                .ToDictionary(
                    p => p.Key,
                    p => p.ToArray());

            return Enumerable
                .Range(1, periodsCount)
                .Select(i => BuildPaymentPeriodLinesForPayments(periodisedPayments.GetValueOrDefault((byte)i, Array.Empty<Payment>())))
                .ToArray();
        }

        public PaymentPeriodLines BuildTotalPaymentPeriodLines(ICollection<PaymentPeriodLines> paymentPeriodLines)
        {
            return new PaymentPeriodLines()
            {
                ApprenticeAdditional = paymentPeriodLines.Sum(p => p.ApprenticeAdditional),
                CoInvestment = paymentPeriodLines.Sum(p => p.CoInvestment),
                CoInvestmentDueFromEmployer = paymentPeriodLines.Sum(p => p.CoInvestmentDueFromEmployer),
                EmployerAdditional = paymentPeriodLines.Sum(p => p.EmployerAdditional),
                EnglishAndMaths = paymentPeriodLines.Sum(p => p.EnglishAndMaths),
                Levy = paymentPeriodLines.Sum(p => p.Levy),
                LearningSupportDisadvantageAndFrameworkUplifts = paymentPeriodLines.Sum(p => p.LearningSupportDisadvantageAndFrameworkUplifts),
                ProviderAdditional = paymentPeriodLines.Sum(p => p.ProviderAdditional),
                Total = paymentPeriodLines.Sum(p => p.Total),
            };
        }

        public PaymentPeriodLines BuildPaymentPeriodLinesForPayments(ICollection<Payment> nonZeroPayments)
        {
            var paymentPeriodLines = new PaymentPeriodLines();

            if (!nonZeroPayments.Any())
            {
                return paymentPeriodLines;
            }

            var levy = GetAmountForPaymentsForFundingSourceAndTransactionType(nonZeroPayments, FundingSourceLevyPayments, TransactionTypesLevyPayments);
            var coInvestment = GetAmountForPaymentsForFundingSourceAndTransactionType(nonZeroPayments, FundingSourceCoInvestmentPayments, TransactionTypesCoInvestmentPayments);
            var coInvestmentDueFromEmployer = GetAmountForPaymentsForFundingSourceAndTransactionType(nonZeroPayments, FundingSourceCoInvestmentDueFromEmployer, TransactionTypesCoInvestmentDueFromEmployer);
            var employerAdditionalPayments = GetAmountForPaymentsForTransactionType(nonZeroPayments, TransactionTypesEmployerAdditionalPayments);
            var providerAdditionalPayments = GetAmountForPaymentsForTransactionType(nonZeroPayments, TransactionTypesProviderAdditionalPayments);
            var apprenticeAdditionalPayments = GetAmountForPaymentsForTransactionType(nonZeroPayments, TransactionTypesApprenticeshipAdditionalPayments);
            var englishAndMathsPayments = GetAmountForPaymentsForTransactionType(nonZeroPayments, TransactionTypesEnglishAndMathsPayments);
            var paymentsForLearningSupportDisadvantageAndFrameworkUplifts = GetAmountForPaymentsForTransactionType(nonZeroPayments, TransactionTypesLearningSupportPayments);

            var total =
                levy
                + coInvestment
                + employerAdditionalPayments
                + providerAdditionalPayments
                + apprenticeAdditionalPayments
                + englishAndMathsPayments
                + paymentsForLearningSupportDisadvantageAndFrameworkUplifts;

            paymentPeriodLines.Levy = levy;
            paymentPeriodLines.CoInvestment = coInvestment;
            paymentPeriodLines.CoInvestmentDueFromEmployer = coInvestmentDueFromEmployer;
            paymentPeriodLines.EmployerAdditional = employerAdditionalPayments;
            paymentPeriodLines.ProviderAdditional = providerAdditionalPayments;
            paymentPeriodLines.ApprenticeAdditional = apprenticeAdditionalPayments;
            paymentPeriodLines.EnglishAndMaths = englishAndMathsPayments;
            paymentPeriodLines.LearningSupportDisadvantageAndFrameworkUplifts = paymentsForLearningSupportDisadvantageAndFrameworkUplifts;
            paymentPeriodLines.Total = total;
            
            return paymentPeriodLines;
        }

        public decimal GetAmountForPaymentsForFundingSourceAndTransactionType(IEnumerable<Payment> payments, IEnumerable<byte> fundingSources, IEnumerable<byte> transactionTypes)
        {
            return payments
                .Where(p =>
                    fundingSources.Contains(p.FundingSource)
                    && transactionTypes.Contains(p.TransactionType))
                .Sum(p => p.Amount);
        }
        
        public decimal GetAmountForPaymentsForTransactionType(IEnumerable<Payment> payments, IEnumerable<byte> transactionTypes)
        {
            return payments
                .Where(p => transactionTypes.Contains(p.TransactionType))
                .Sum(p => p.Amount);
        }
    }
}
