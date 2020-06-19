using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class PaymentPeriodsBuilderTests
    {
        private readonly HashSet<byte> _fundingSourceLevyPayments = new HashSet<byte>() { 1, 5 };
        private readonly HashSet<byte> _fundingSourceCoInvestmentPayments = new HashSet<byte>() { 2 };
        private readonly HashSet<byte> _fundingSourceCoInvestmentDueFromEmployer = new HashSet<byte>() { 3 };
        private readonly HashSet<byte> _transactionTypesLevyPayments = new HashSet<byte>() { 1, 2, 3 };
        private readonly HashSet<byte> _transactionTypesCoInvestmentPayments = new HashSet<byte>() { 1, 2, 3 };
        private readonly HashSet<byte> _transactionTypesCoInvestmentDueFromEmployer = new HashSet<byte>() { 1, 2, 3 };
        private readonly HashSet<byte> _transactionTypesEmployerAdditionalPayments = new HashSet<byte>() { 4, 6 };
        private readonly HashSet<byte> _transactionTypesProviderAdditionalPayments = new HashSet<byte>() { 5, 7 };
        private readonly HashSet<byte> _transactionTypesApprenticeshipAdditionalPayments = new HashSet<byte>() { 16 };
        private readonly HashSet<byte> _transactionTypesEnglishAndMathsPayments = new HashSet<byte>() { 13, 14 };
        private readonly HashSet<byte> _transactionTypesLearningSupportPayments = new HashSet<byte>() { 8, 9, 10, 11, 12, 15 };

        [Fact]
        public void GetAmountForPaymentsForTransactionType()
        {
            var paymentOne = new PaymentBuilder().With<byte>(p => p.TransactionType, 1).With<byte>(p => p.FundingSource, 2).With(p => p.Amount, 1.1m).Build();
            var paymentTwo = new PaymentBuilder().With<byte>(p => p.TransactionType, 2).With<byte>(p => p.FundingSource, 2).With(p => p.Amount, 1.2m).Build();
            var paymentThree = new PaymentBuilder().With<byte>(p => p.TransactionType, 1).With<byte>(p => p.FundingSource, 2).With(p => p.Amount, 1.3m).Build();
            var paymentFour = new PaymentBuilder().With<byte>(p => p.TransactionType, 2).With<byte>(p => p.FundingSource, 2).With(p => p.Amount, 1.4m).Build();
            var paymentFive = new PaymentBuilder().With<byte>(p => p.TransactionType, 3).With<byte>(p => p.FundingSource, 2).With(p => p.Amount, 1.5m).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree,
                paymentFour,
                paymentFive,
            };

            NewBuilder().GetAmountForPaymentsForTransactionType(payments, new List<byte>() {1, 3,}).Should().Be(3.9m);
        }

        [Fact]
        public void GetAmountForPaymentsForFundingSourceAndTransactionType()
        {
            var paymentOne = new PaymentBuilder().With<byte>(p => p.TransactionType, 1).With<byte>(p => p.FundingSource, 1).With(p => p.Amount, 1.1m).Build();
            var paymentTwo = new PaymentBuilder().With<byte>(p => p.TransactionType, 2).With<byte>(p => p.FundingSource, 2).With(p => p.Amount, 1.2m).Build();
            var paymentThree = new PaymentBuilder().With<byte>(p => p.TransactionType, 1).With<byte>(p => p.FundingSource, 3).With(p => p.Amount, 1.3m).Build();
            var paymentFour = new PaymentBuilder().With<byte>(p => p.TransactionType, 2).With<byte>(p => p.FundingSource, 4).With(p => p.Amount, 1.4m).Build();
            var paymentFive = new PaymentBuilder().With<byte>(p => p.TransactionType, 3).With<byte>(p => p.FundingSource, 5).With(p => p.Amount, 1.5m).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree,
                paymentFour,
                paymentFive,
            };

            NewBuilder().GetAmountForPaymentsForFundingSourceAndTransactionType(payments, new List<byte>() { 1 },  new List<byte>() { 1, 3, }).Should().Be(1.1m);
        }

        [Fact]
        public void BuildPaymentPeriodLinesForPayments()
        {
            var paymentCombinations = new List<Payment>();

            var fundingSources =
                _fundingSourceLevyPayments
                    .Union(_fundingSourceCoInvestmentPayments)
                    .Union(_fundingSourceCoInvestmentDueFromEmployer).ToList();

            var transactionTypes =
                _transactionTypesLevyPayments
                    .Union(_transactionTypesCoInvestmentPayments)
                    .Union(_transactionTypesCoInvestmentDueFromEmployer)
                    .Union(_transactionTypesEmployerAdditionalPayments)
                    .Union(_transactionTypesProviderAdditionalPayments)
                    .Union(_transactionTypesApprenticeshipAdditionalPayments)
                    .Union(_transactionTypesEnglishAndMathsPayments)
                    .Union(_transactionTypesLearningSupportPayments).ToList();

            foreach (var fundingSource in fundingSources)
            {
                foreach (var transactionType in transactionTypes)
                {
                    paymentCombinations.Add(
                        new PaymentBuilder()
                            .With(p => p.FundingSource, fundingSource)
                            .With(p => p.TransactionType, transactionType)
                            .With(p => p.Amount, 1m)
                            .Build());
                }
            }

            var paymentPeriodLines = NewBuilder().BuildPaymentPeriodLinesForPayments(paymentCombinations);

            paymentPeriodLines.ApprenticeAdditional.Should().Be(4);
            paymentPeriodLines.CoInvestment.Should().Be(3);
            paymentPeriodLines.CoInvestmentDueFromEmployer.Should().Be(3);
            paymentPeriodLines.EmployerAdditional.Should().Be(8);
            paymentPeriodLines.EnglishAndMaths.Should().Be(8);
            paymentPeriodLines.Levy.Should().Be(6);
            paymentPeriodLines.LearningSupportDisadvantageAndFrameworkUplifts.Should().Be(24);
            paymentPeriodLines.ProviderAdditional.Should().Be(8);
            paymentPeriodLines.Total.Should().Be(61);
        }

        [Fact]
        public void BuildPaymentPeriodLinesForPayments_Empty()
        {
            var paymentPeriodLines = NewBuilder().BuildPaymentPeriodLinesForPayments(Array.Empty<Payment>());

            paymentPeriodLines.ApprenticeAdditional.Should().Be(0);
            paymentPeriodLines.CoInvestment.Should().Be(0);
            paymentPeriodLines.CoInvestmentDueFromEmployer.Should().Be(0);
            paymentPeriodLines.EmployerAdditional.Should().Be(0);
            paymentPeriodLines.EnglishAndMaths.Should().Be(0);
            paymentPeriodLines.Levy.Should().Be(0);
            paymentPeriodLines.LearningSupportDisadvantageAndFrameworkUplifts.Should().Be(0);
            paymentPeriodLines.ProviderAdditional.Should().Be(0);
            paymentPeriodLines.Total.Should().Be(0);
        }

        [Fact]
        public void BuildPeriodisedPaymentPeriodLines()
        {
            var paymentCombinations = new List<Payment>();

            var fundingSources =
                _fundingSourceLevyPayments
                    .Union(_fundingSourceCoInvestmentPayments)
                    .Union(_fundingSourceCoInvestmentDueFromEmployer).ToList();

            var transactionTypes =
                _transactionTypesLevyPayments
                    .Union(_transactionTypesCoInvestmentPayments)
                    .Union(_transactionTypesCoInvestmentDueFromEmployer)
                    .Union(_transactionTypesEmployerAdditionalPayments)
                    .Union(_transactionTypesProviderAdditionalPayments)
                    .Union(_transactionTypesApprenticeshipAdditionalPayments)
                    .Union(_transactionTypesEnglishAndMathsPayments)
                    .Union(_transactionTypesLearningSupportPayments).ToList();

            for (byte i = 1; i <= 14; i++)
            {
                if (i % 2 == 0)
                {
                    foreach (var fundingSource in fundingSources)
                    {
                        foreach (var transactionType in transactionTypes)
                        {
                            paymentCombinations.Add(
                                new PaymentBuilder()
                                    .With(p => p.CollectionPeriod, i)
                                    .With(p => p.FundingSource, fundingSource)
                                    .With(p => p.TransactionType, transactionType)
                                    .With(p => p.Amount, 1m)
                                    .Build());
                        }
                    }
                }
            }

            var periodisedPaymentPeriodLines = NewBuilder().BuildPeriodisedPaymentPeriods(paymentCombinations, 14);

            for (var i = 0; i <= 13; i++)
            {
                var paymentPeriodLines = periodisedPaymentPeriodLines[i];

                if (i % 2 == 0)
                {
                    paymentPeriodLines.ApprenticeAdditional.Should().Be(0);
                    paymentPeriodLines.CoInvestment.Should().Be(0);
                    paymentPeriodLines.CoInvestmentDueFromEmployer.Should().Be(0);
                    paymentPeriodLines.EmployerAdditional.Should().Be(0);
                    paymentPeriodLines.EnglishAndMaths.Should().Be(0);
                    paymentPeriodLines.Levy.Should().Be(0);
                    paymentPeriodLines.LearningSupportDisadvantageAndFrameworkUplifts.Should().Be(0);
                    paymentPeriodLines.ProviderAdditional.Should().Be(0);
                    paymentPeriodLines.Total.Should().Be(0);
                }
                else
                {
                    paymentPeriodLines.ApprenticeAdditional.Should().Be(4);
                    paymentPeriodLines.CoInvestment.Should().Be(3);
                    paymentPeriodLines.CoInvestmentDueFromEmployer.Should().Be(3);
                    paymentPeriodLines.EmployerAdditional.Should().Be(8);
                    paymentPeriodLines.EnglishAndMaths.Should().Be(8);
                    paymentPeriodLines.Levy.Should().Be(6);
                    paymentPeriodLines.LearningSupportDisadvantageAndFrameworkUplifts.Should().Be(24);
                    paymentPeriodLines.ProviderAdditional.Should().Be(8);
                    paymentPeriodLines.Total.Should().Be(61);
                }
            }
        }

        [Fact]
        public void BuildTotalPaymentPeriodLines()
        {
            var periodisedPaymentPeriodLines = Enumerable.Range(1, 14)
                .Select(i =>
                    new PaymentPeriodLines()
                    {
                        ApprenticeAdditional = 1,
                        CoInvestment = 2,
                        CoInvestmentDueFromEmployer = 3,
                        EmployerAdditional = 4,
                        EnglishAndMaths = 5,
                        Levy = 6,
                        LearningSupportDisadvantageAndFrameworkUplifts = 7,
                        ProviderAdditional = 8,
                        Total = 9,
                    }).ToArray();

            var totalPaymentPeriodLines = NewBuilder().BuildTotalPaymentPeriodLines(periodisedPaymentPeriodLines);

            totalPaymentPeriodLines.ApprenticeAdditional.Should().Be(14);
            totalPaymentPeriodLines.CoInvestment.Should().Be(28);
            totalPaymentPeriodLines.CoInvestmentDueFromEmployer.Should().Be(42);
            totalPaymentPeriodLines.EmployerAdditional.Should().Be(56);
            totalPaymentPeriodLines.EnglishAndMaths.Should().Be(70);
            totalPaymentPeriodLines.Levy.Should().Be(84);
            totalPaymentPeriodLines.LearningSupportDisadvantageAndFrameworkUplifts.Should().Be(98);
            totalPaymentPeriodLines.ProviderAdditional.Should().Be(112);
            totalPaymentPeriodLines.Total.Should().Be(126);
        }

        [Fact]
        public void BuildPaymentPeriods()
        {
            var paymentCombinations = new List<Payment>();

            var fundingSources =
                _fundingSourceLevyPayments
                    .Union(_fundingSourceCoInvestmentPayments)
                    .Union(_fundingSourceCoInvestmentDueFromEmployer).ToList();

            var transactionTypes =
                _transactionTypesLevyPayments
                    .Union(_transactionTypesCoInvestmentPayments)
                    .Union(_transactionTypesCoInvestmentDueFromEmployer)
                    .Union(_transactionTypesEmployerAdditionalPayments)
                    .Union(_transactionTypesProviderAdditionalPayments)
                    .Union(_transactionTypesApprenticeshipAdditionalPayments)
                    .Union(_transactionTypesEnglishAndMathsPayments)
                    .Union(_transactionTypesLearningSupportPayments).ToList();

            for (byte i = 1; i <= 14; i++)
            {
                foreach (var fundingSource in fundingSources)
                {
                    foreach (var transactionType in transactionTypes)
                    {
                        paymentCombinations.Add(
                            new PaymentBuilder()
                                .With(p => p.CollectionPeriod, i)
                                .With(p => p.FundingSource, fundingSource)
                                .With(p => p.TransactionType, transactionType)
                                .With(p => p.Amount, i)
                                .Build());
                    }
                }
            }

            var paymentPeriods = NewBuilder().BuildPaymentPeriods(paymentCombinations);

            var total = 61;
            var periodCount = 1;

            paymentPeriods.R01.Total.Should().Be(total * periodCount++);
            paymentPeriods.R02.Total.Should().Be(total * periodCount++);
            paymentPeriods.R03.Total.Should().Be(total * periodCount++);
            paymentPeriods.R04.Total.Should().Be(total * periodCount++);
            paymentPeriods.R05.Total.Should().Be(total * periodCount++);
            paymentPeriods.R06.Total.Should().Be(total * periodCount++);
            paymentPeriods.R07.Total.Should().Be(total * periodCount++);
            paymentPeriods.R08.Total.Should().Be(total * periodCount++);
            paymentPeriods.R09.Total.Should().Be(total * periodCount++);
            paymentPeriods.R10.Total.Should().Be(total * periodCount++);
            paymentPeriods.R11.Total.Should().Be(total * periodCount++);
            paymentPeriods.R12.Total.Should().Be(total * periodCount++);
            paymentPeriods.R13.Total.Should().Be(total * periodCount++);
            paymentPeriods.R14.Total.Should().Be(total * periodCount);

            paymentPeriods.Total.Total.Should().Be(6405);
        }

        private PaymentPeriodsBuilder NewBuilder()
        {
            return new PaymentPeriodsBuilder();
        }
    }
}
