using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class PaymentsBuilderTests
    {
        private const int _currentAcademicYear = 2021;
        private readonly DateTime _academicYearStart = new DateTime(2020, 8, 1);
        private readonly DateTime _nextAcademicYearStart = new DateTime(2021, 8, 1);

        [Fact]
        public void FilterByFundingSourceAndTransactionType_Test()
        {
            var fundingSource = 3;
            var transactionTypes = new HashSet<int>() {1, 2, 3};
            var paymentOne = new PaymentBuilder()
                .With<byte>(p => p.FundingSource, 3)
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 10)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.DeliveryPeriod, 1)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                .With<byte>(p => p.FundingSource, 2)
                .With<byte>(p => p.TransactionType, 2)
                .With(p => p.Amount, 20)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.DeliveryPeriod, 1)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentThree = new PaymentBuilder()
                .With<byte>(p => p.FundingSource, 3)
                .With<byte>(p => p.TransactionType, 1)
                .With(p => p.Amount, 30)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.DeliveryPeriod, 2)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree
            };

            var filteredPayments = NewBuilder().FilterByFundingSourceAndTransactionType(payments, fundingSource, transactionTypes);
            filteredPayments.Count().Should().Be(2);
        }

        [Fact]
        public void GetEarliestPaymentInfo_Should_Return_EarliestPayment()
        {
            var paymentOne = new PaymentBuilder().With<short>(p=>p.AcademicYear, 1920).With<byte>(p=>p.DeliveryPeriod, 1).With<byte>(p=>p.CollectionPeriod, 1).Build();
            var paymentTwo = new PaymentBuilder().With<short>(p => p.AcademicYear, 2020).With<byte>(p => p.DeliveryPeriod, 1).With<byte>(p => p.CollectionPeriod, 1).Build();
            var paymentThree = new PaymentBuilder().With<short>(p => p.AcademicYear, 1920).With<byte>(p => p.DeliveryPeriod, 2).With<byte>(p => p.CollectionPeriod, 1).Build();
            var paymentFour = new PaymentBuilder().With<short>(p => p.AcademicYear, 1920).With<byte>(p => p.DeliveryPeriod, 2).With<byte>(p => p.CollectionPeriod, 2).Build();
            var paymentFive = new PaymentBuilder().With<short>(p => p.AcademicYear, 2020).With<byte>(p => p.DeliveryPeriod, 2).With<byte>(p => p.CollectionPeriod, 1).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree,
                paymentFour,
                paymentFive,
            };
            var builder = NewBuilder();
            var earliestPayment = builder.GetEarliestPayment(payments);
            earliestPayment.Should().BeSameAs(paymentOne);
        }

        [Fact]
        public void CalculateCompletionEarningsThisFundingYear_Tests()
        {
            var aecApprenticeshipPriceEpisodePeriodisedValuesOne = new AecPriceEpisodePeriodisedValueBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .With(a => a.AimSeqNumber, 1)
                .With(a => a.Period1, 10)
                .With(a => a.Period2, 20)
                .With(a => a.Period3, 30)
                .Build();


            var aecApprenticeshipPriceEpisodePeriodisedValuesTwo = new AecPriceEpisodePeriodisedValueBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .With(a => a.AimSeqNumber, 2)
                .With(a => a.Period1, 10)
                .Build();

            var aecPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValues>()
            {
                aecApprenticeshipPriceEpisodePeriodisedValuesOne,
                aecApprenticeshipPriceEpisodePeriodisedValuesTwo
            };

            var learningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .With(a => a.AimSeqNumber, 1)
                .Build();

            var completionEarningsThisFundingYear = NewBuilder().CalculateCompletionEarningsThisFundingYear(learningDelivery, aecPriceEpisodePeriodisedValues);
            completionEarningsThisFundingYear.Should().Be(60);
        }

        [Fact]
        public void CalculateCompletionEarningsThisFundingYear_NullLearningDelivery()
        {
            var completionEarningsThisFundingYear = NewBuilder().CalculateCompletionEarningsThisFundingYear(null, null);
            completionEarningsThisFundingYear.Should().Be(0);
        }

        [Fact]
        public void CalculateCompletionPaymentsInAcademicYear_Test()
        {
            var paymentOne = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 40)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.CollectionPeriod, 2).Build();

            var paymentTwo = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 2)
                .With(p => p.Amount, 50)
                .With<short>(p => p.AcademicYear, 2021)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentThree = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 60)
                .With<short>(p => p.AcademicYear, 2021)
                .With<byte>(p => p.CollectionPeriod, 2).Build();
            
            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree
            };

            var completionEarningsThisFundingYear = NewBuilder().CalculateCompletionPaymentsInAcademicYear(payments, 2021);
            completionEarningsThisFundingYear.Should().Be(50);
        }

        [Fact]
        public void GetEmployerCoInvestmentPercentage_Test()
        {
            var paymentOne = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 40)
                .With(p=> p.SfaContributionPercentage, 0.9m).Build();

            var paymentTwo = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 2)
                .With(p => p.Amount, 50)
                .With(p => p.SfaContributionPercentage, 0.95m).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo
            };

            var employerCoInvestmentPercentage = NewBuilder().GetEmployerCoInvestmentPercentage(payments);
            employerCoInvestmentPercentage.Should().Be(10m);
        }

        [Fact]
        public void GetEmployerCoInvestmentPercentage_EmptyPaymentsList_Test()
        {
            var employerCoInvestmentPercentage = NewBuilder().GetEmployerCoInvestmentPercentage(new List<Payment>());
            employerCoInvestmentPercentage.Should().BeNull();
        }

        [Fact]
        public void GetEmployerCoInvestmentPercentage_ZeroAmount_Test()
        {
            var paymentOne = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 0)
                .With(p => p.SfaContributionPercentage, 0.9m).Build();

            var paymentTwo = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 2)
                .With(p => p.Amount, 0)
                .With(p => p.SfaContributionPercentage, 0.95m).Build();
            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo
            };

            var employerCoInvestmentPercentage = NewBuilder().GetEmployerCoInvestmentPercentage(payments);
            employerCoInvestmentPercentage.Should().BeNull();
        }

        [Fact]
        public void GetTotalPmrBetweenDates_Test()
        {
            var appFinRecords = new List<AppFinRecord>()
            {
                new AppFindRecordBuilder().With(a => a.AFinCode, 1).With<int>(a => a.AFinAmount, 10).Build(),
                new AppFindRecordBuilder().With(a => a.AFinCode, 2).With<int>(a => a.AFinAmount, 100).Build(),
                new AppFindRecordBuilder().With(a => a.AFinCode, 3).With<int>(a => a.AFinAmount, 20).Build(),
                new AppFindRecordBuilder().With(a => a.AFinDate, new DateTime(2020, 07, 31)).With(a => a.AFinCode, 1)
                    .With<int>(a => a.AFinAmount, 100).Build(),
            };

            var learningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .With(a => a.AimSeqNumber, 1)
                .With(ld => ld.AppFinRecords, appFinRecords)
                .Build();

            var totalCollectedCurrentYear = NewBuilder().GetTotalPmrBetweenDates(learningDelivery, _academicYearStart, _nextAcademicYearStart);
            totalCollectedCurrentYear.Should().Be(90);

            var totalCollectedPreviousYear = NewBuilder().GetTotalPmrBetweenDates(learningDelivery, null, _academicYearStart);
            totalCollectedPreviousYear.Should().Be(100);
        }

        [Fact]
        public void GetPeriodisedValueFromDictionaryForPeriod_Test()
        {
            IDictionary<byte, decimal> periodisedDictionary = new Dictionary<byte, decimal>();
           periodisedDictionary.Add(1,100m);
           NewBuilder().GetPeriodisedValueFromDictionaryForPeriod(periodisedDictionary,1).Should().Be(100);
        }

        [Fact]
        public void GetPeriodisedValueFromDictionaryForPeriod_InvalidPeriod_Test()
        {
            IDictionary<byte, decimal> periodisedDictionary = new Dictionary<byte, decimal>();
            periodisedDictionary.Add(1, 100m);
            NewBuilder().GetPeriodisedValueFromDictionaryForPeriod(periodisedDictionary, 2).Should().Be(0);
        }

        [Fact]
        public void TotalCoInvestmentDueFromEmployerInPreviousFundingYears_Test()
        {
            var paymentOne = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 1)
                .With(p => p.Amount, 30)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.DeliveryPeriod, 2)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 40)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.DeliveryPeriod, 2)
                .With<byte>(p => p.CollectionPeriod, 2).Build();

            var paymentThree = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 2)
                .With(p => p.Amount, 50)
                .With<short>(p => p.AcademicYear, 2021)
                .With<byte>(p => p.DeliveryPeriod, 2)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree
            };

            var totalCoInvestmentDueFromEmployerInPreviousFundingYears = NewBuilder().TotalCoInvestmentDueFromEmployerInPreviousFundingYears(payments, 2021);
            totalCoInvestmentDueFromEmployerInPreviousFundingYears.Should().Be(70);
        }


        [Fact]
        public void BuildCoInvestmentPaymentsPerPeriodDictionary_Test()
        {
            var periodOneValue = 0m;
            var periodTwoValue = 0m;
            var paymentOne = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 1)
                .With(p => p.Amount, 30)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 40)
                .With<short>(p => p.AcademicYear, 2020)
                .With<byte>(p => p.CollectionPeriod, 2).Build();

            var paymentThree = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 2)
                .With(p => p.Amount, 50)
                .With<short>(p => p.AcademicYear, 2021)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentFour = new PaymentBuilder()
                .With<byte>(p => p.TransactionType, 3)
                .With(p => p.Amount, 60)
                .With<short>(p => p.AcademicYear, 2021)
                .With<byte>(p => p.CollectionPeriod, 2).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree,
                paymentFour
            };

            var result = NewBuilder().BuildCoInvestmentPaymentsPerPeriodDictionary(payments, 2021);
            result.Count.Should().Be(2);
            result.TryGetValue(1, out periodOneValue);
            result.TryGetValue(2, out periodTwoValue);
            periodOneValue.Should().Be(50);
            periodTwoValue.Should().Be(60);
        }

        [Fact]
        public void BuildEarningsAndPayments()
        {

            var paymentOne = new PaymentBuilder()
                                    .With<byte>(p=> p.FundingSource , 2)
                                    .With<byte>(p=> p.TransactionType, 3)
                                    .With(p => p.Amount, 10)
                                    .With<short>(p => p.AcademicYear, 2020)
                                    .With<byte>(p => p.DeliveryPeriod, 1)
                                    .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                                    .With<byte>(p => p.FundingSource, 2)
                                    .With<byte>(p => p.TransactionType, 2)
                                    .With(p => p.Amount, 20)
                                    .With<short>(p => p.AcademicYear, 2020)
                                    .With<byte>(p => p.DeliveryPeriod, 1)
                                    .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentThree = new PaymentBuilder()
                                    .With<byte>(p => p.TransactionType, 1)
                                    .With(p => p.Amount, 30)
                                    .With<short>(p => p.AcademicYear, 2020)
                                    .With<byte>(p => p.DeliveryPeriod, 2)
                                    .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentFour = new PaymentBuilder()
                                    .With<byte>(p => p.TransactionType, 3)
                                    .With(p => p.Amount, 40)
                                    .With<short>(p => p.AcademicYear, 2020)
                                    .With<byte>(p => p.DeliveryPeriod, 2)
                                    .With<byte>(p => p.CollectionPeriod, 2).Build();

            var paymentFive = new PaymentBuilder()
                                    .With<byte>(p => p.TransactionType, 2)
                                    .With(p => p.Amount, 50)
                                    .With<short>(p => p.AcademicYear, 2021)
                                    .With<byte>(p => p.DeliveryPeriod, 2)
                                    .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentSix = new PaymentBuilder()
                                    .With<byte>(p => p.TransactionType, 3)
                                    .With(p => p.Amount, 60)
                                    .With<short>(p => p.AcademicYear, 2021)
                                    .With<byte>(p => p.DeliveryPeriod, 2)
                                    .With<byte>(p => p.CollectionPeriod, 2).Build();

            var allPayments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree,
                paymentFour,
                paymentFive,
                paymentSix
            };

            var filteredPayments = new List<Payment>()
            {
                paymentThree,
                paymentFour,
                paymentFive,
                paymentSix
            };

            var aecApprenticeshipPriceEpisodePeriodisedValuesOne = new AecPriceEpisodePeriodisedValueBuilder()
                                                                            .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                                                                            .With(a => a.AimSeqNumber, 1)
                                                                            .With(a => a.Period1, 10)
                                                                            .With(a => a.Period2, 20)
                                                                            .With(a => a.Period3, 30)
                                                                            .Build();


            var aecApprenticeshipPriceEpisodePeriodisedValuesTwo = new AecPriceEpisodePeriodisedValueBuilder()
                                                                            .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                                                                            .With(a => a.AimSeqNumber, 2)
                                                                            .With(a => a.Period1, 10)
                                                                            .Build();
            var aecPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValues>()
            {
                aecApprenticeshipPriceEpisodePeriodisedValuesOne,
                aecApprenticeshipPriceEpisodePeriodisedValuesTwo
            };

            var appFinRecords = new List<AppFinRecord>()
            {
                new AppFindRecordBuilder().With(a => a.AFinCode, 1).With<int>(a => a.AFinAmount, 10).Build(),
                new AppFindRecordBuilder().With(a => a.AFinCode, 2).With<int>(a => a.AFinAmount, 100).Build(),
                new AppFindRecordBuilder().With(a => a.AFinCode, 3).With<int>(a => a.AFinAmount, 20).Build(),
                new AppFindRecordBuilder().With(a => a.AFinDate, new DateTime(2020, 07, 31)).With(a => a.AFinCode, 1)
                    .With<int>(a => a.AFinAmount, 100).Build(),
            };

            var learningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber")
                .With(a => a.AimSeqNumber, 1)
                .With(ld=>ld.AppFinRecords, appFinRecords)
                .Build();
            
            var earningsAndPayments = NewBuilder().BuildEarningsAndPayments(filteredPayments, allPayments, learningDelivery,
                aecPriceEpisodePeriodisedValues, _currentAcademicYear, _academicYearStart, _nextAcademicYearStart);

            earningsAndPayments.CoInvestmentPaymentsDueFromEmployer.August.Should().Be(50);
            earningsAndPayments.CoInvestmentPaymentsDueFromEmployer.September.Should().Be(60);
            earningsAndPayments.CompletionEarningThisFundingYear.Should().Be(60);
            earningsAndPayments.CompletionPaymentsThisFundingYear.Should().Be(50);
            earningsAndPayments.TotalCoInvestmentDueFromEmployerInPreviousFundingYears.Should().Be(70);
            earningsAndPayments.TotalCoInvestmentDueFromEmployerThisFundingYear.Should().Be(110);
            earningsAndPayments.TotalPMRPreviousFundingYears.Should().Be(100);
            earningsAndPayments.TotalPMRThisFundingYear.Should().Be(90);
            earningsAndPayments.EmployerCoInvestmentPercentage.Should().Be(10.00m);
            Math.Round(earningsAndPayments.PercentageOfCoInvestmentCollected, 2).Should().Be(105.56m);
        }
        
        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Zero_For_Zero_Inputs()
        {
            decimal? totalDueCurrentYear = 0M;
            decimal? totalDuePreviousYear = 0M;
            decimal? totalCollectedCurrentYear = 0M;
            decimal? totalCollectedPreviousYear = 0M;

            var builder = NewBuilder();
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            percent.Should().Be(0);
        }

        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Max_For_Huge_Percent()
        {
            decimal max = 99999.99M;

            decimal? totalDueCurrentYear = 10M;
            decimal? totalDuePreviousYear = 10M;
            decimal? totalCollectedCurrentYear = 100000000M;
            decimal? totalCollectedPreviousYear = 0M;

            var builder = NewBuilder();
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            percent.Should().Be(max);
        }

        [Fact]
        public void GetPercentageOfInvestmentCollected_Returns_Min_For_Huge_Negative_Percent()
        {
            decimal min = -99999.99M;

            decimal? totalDueCurrentYear = 10M;
            decimal? totalDuePreviousYear = 10M;
            decimal? totalCollectedCurrentYear = -100000000M;
            decimal? totalCollectedPreviousYear = 0M;

            var builder = NewBuilder();
            var percent = builder.GetPercentageOfInvestmentCollected(
                totalDueCurrentYear,
                totalDuePreviousYear,
                totalCollectedCurrentYear,
                totalCollectedPreviousYear);

            percent.Should().Be(min);
        }

        private PaymentsBuilder NewBuilder(
            IEqualityComparer<AppsCoInvestmentRecordKey> appsCoInvestmentEqualityComparer = null)
        {
            return new PaymentsBuilder(appsCoInvestmentEqualityComparer ?? Mock.Of<IEqualityComparer<AppsCoInvestmentRecordKey>>());
        }
    }
}
