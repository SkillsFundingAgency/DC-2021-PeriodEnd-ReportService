using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class AppsCoInvestmentModelBuilderTests
    {

        [Fact]
        public void GetRelevantLearners_Test()
        {
            var appFinRecordsPmr = new List<AppFinRecord>()
            {
                new AppFindRecordBuilder().With(a=>a.AFinType, "PMR").With(a => a.AFinCode, 1).Build(),
                new AppFindRecordBuilder().With(a=>a.AFinType, "PMR").With(a => a.AFinCode, 2).Build()
            };

            var appFinRecordsNonPmr = new List<AppFinRecord>()
            {
                new AppFindRecordBuilder().With(a=>a.AFinType, "TNP").With(a => a.AFinCode, 1).With<int>(a => a.AFinAmount, 10).Build(),
            };

            var learningDeliveryOne = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber1")
                .With(ld => ld.FundModel , 36)
                .With(a => a.AimSeqNumber, 1)
                .With(ld => ld.AppFinRecords, appFinRecordsPmr)
                .Build();

            var learningDeliveryTwo = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber2")
                .With(ld => ld.FundModel, 36)
                .With(a => a.AimSeqNumber, 2)
                .Build();

            var nonPmrlearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber3")
                .With(ld => ld.FundModel, 36)
                .With(ld => ld.AppFinRecords, appFinRecordsNonPmr)
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                learningDeliveryOne,
                learningDeliveryTwo,
                nonPmrlearningDelivery
            };

            var nonPmrLearningDeliveries = new List<LearningDelivery>
            {
                nonPmrlearningDelivery
            };

            var paymentOne = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber3")
                .With<byte>(p => p.FundingSource, 3)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber4")
                .With<byte>(p => p.FundingSource, 3)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentThree = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber5")
                .With<byte>(p => p.FundingSource, 2)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree
            };

            var learnerOne = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber1")
                                .With(l => l.LearningDeliveries, learningDeliveries)
                                .Build();

            var learnerTwo = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber2")
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var learnerThree = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber3")
                .With(l => l.LearningDeliveries, nonPmrLearningDeliveries)
                .Build();

            var learnerFour = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber4")
                .Build();

            var learnerFive = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber5")
                .Build();

            var learners = new List<Learner>
            {
                learnerOne,
                learnerTwo,
                learnerThree,
                learnerFour,
                learnerFive
            };

            var relevantLearners = NewBuilder().GetRelevantLearners(learners, payments).ToList();
            relevantLearners.Count().Should().Be(3);
            relevantLearners.Contains(learnerOne.LearnRefNumber).Should().BeTrue();
            relevantLearners.Contains(learnerTwo.LearnRefNumber).Should().BeTrue();
            relevantLearners.Contains(learnerThree.LearnRefNumber).Should().BeTrue();
            relevantLearners.Contains(learnerFour.LearnRefNumber).Should().BeFalse();
            relevantLearners.Contains(learnerFive.LearnRefNumber).Should().BeFalse();
        }

        [Fact]
        public void EmployerCoInvestmentPaymentFilter_Test()
        {
            var paymentOne = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber1")
                .With<byte>(p => p.FundingSource, 3)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber2")
                .With<byte>(p => p.FundingSource, 3)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentThree = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber3")
                .With<byte>(p => p.FundingSource, 2)
                .With<byte>(p => p.CollectionPeriod, 1).Build();
            
            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree
            };

            NewBuilder().EmployerCoInvestmentPaymentFilter(payments, "LearningRefNumber1").Should().BeTrue();
            NewBuilder().EmployerCoInvestmentPaymentFilter(payments, "LearningRefNumber2").Should().BeTrue();
            NewBuilder().EmployerCoInvestmentPaymentFilter(payments, "LearningRefNumber3").Should().BeFalse();
        }

        [Fact]
        public void CompletionPaymentFilter_Test()
        {
            var paymentOne = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber1")
                .With<byte>(p => p.TransactionType, 3)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentTwo = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber2")
                .With<byte>(p => p.TransactionType, 3)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var paymentThree = new PaymentBuilder()
                .With(p => p.LearnerReferenceNumber, "LearningRefNumber3")
                .With<byte>(p => p.TransactionType, 2)
                .With<byte>(p => p.CollectionPeriod, 1).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
                paymentThree
            };

            NewBuilder().CompletionPaymentFilter(payments, "LearningRefNumber1").Should().BeTrue();
            NewBuilder().CompletionPaymentFilter(payments, "LearningRefNumber2").Should().BeTrue();
            NewBuilder().CompletionPaymentFilter(payments, "LearningRefNumber3").Should().BeFalse();
        }

        [Fact]
        public void GetName()
        {
            var learner = new LearnerBuilder().With(l => l.FamilyName, "FamilyName").Build();

            NewBuilder().GetName(learner, l => l.FamilyName).Should().Be("FamilyName");
        }

        [Fact]
        public void GetName_NullLearner()
        {
            NewBuilder().GetName(null, l => l.FamilyName).Should().Be("Not applicable.  For more information refer to the funding reports guidance.");
        }

        [Fact]
        public void GetName_NullName()
        {
            var learner = new LearnerBuilder().With(l => l.FamilyName, null).Build();

            NewBuilder().GetName(learner, l => l.FamilyName).Should().BeNull();
        }

        [Fact]
        public void PMRAppFinRecordFilter_Test()
        {
            var appFinRecordsPmr = new List<AppFinRecord>()
            {
                new AppFindRecordBuilder().With(a=>a.AFinType, "PMR").With(a => a.AFinCode, 1).Build(),
                new AppFindRecordBuilder().With(a=>a.AFinType, "PMR").With(a => a.AFinCode, 2).Build()
            };

            var appFinRecordsNonPmr = new List<AppFinRecord>()
            {
                new AppFindRecordBuilder().With(a=>a.AFinType, "TNP").With(a => a.AFinCode, 1).With<int>(a => a.AFinAmount, 10).Build(),
            };

            var learningDeliveryOne = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber1")
                .With(ld => ld.FundModel, 36)
                .With(a => a.AimSeqNumber, 1)
                .With(ld => ld.AppFinRecords, appFinRecordsPmr)
                .Build();

            var learningDeliveryTwo = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber2")
                .With(ld => ld.FundModel, 36)
                .With(a => a.AimSeqNumber, 2)
                .Build();

            var nonPmrlearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnRefNumber, "LearningRefNumber3")
                .With(ld => ld.FundModel, 36)
                .With(ld => ld.AppFinRecords, appFinRecordsNonPmr)
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                learningDeliveryOne,
                learningDeliveryTwo,
                nonPmrlearningDelivery
            };

            var nonPmrLearningDeliveries = new List<LearningDelivery>
            {
                nonPmrlearningDelivery
            };

            var learnerOne = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber1")
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var learnerTwo = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber2")
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var learnerThree = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber3")
                .With(l => l.LearningDeliveries, nonPmrLearningDeliveries)
                .Build();

            var learnerFour = new LearnerBuilder()
                .With(l => l.LearnRefNumber, "LearningRefNumber4")
                .Build();

            var learners = new List<Learner>
            {
                learnerOne,
                learnerTwo,
                learnerThree,
                learnerFour
            };

            NewBuilder().PMRAppFinRecordFilter(learners, "LearningRefNumber1").Should().BeTrue();
            NewBuilder().PMRAppFinRecordFilter(learners, "LearningRefNumber2").Should().BeTrue();
            NewBuilder().PMRAppFinRecordFilter(learners, "LearningRefNumber3").Should().BeFalse();
            NewBuilder().PMRAppFinRecordFilter(learners, "LearningRefNumber4").Should().BeFalse();

        }

        [Fact]
        public void NonZeroCompletionEarningsFilter_Test()
        {
            var aecApprenticeshipPriceEpisodePeriodisedValuesOne = new AecPriceEpisodePeriodisedValueBuilder()
                .With(a => a.LearnRefNumber, "LearningRefNumber1")
                .With(a => a.Period1, 10)
                .With(a => a.AttributeName, "PriceEpisodeCompletionPayment")
                .Build();


            var aecApprenticeshipPriceEpisodePeriodisedValuesTwo = new AecPriceEpisodePeriodisedValueBuilder()
                .With(a => a.LearnRefNumber, "LearningRefNumber2")
                .With(a => a.Period1, 20)
                .With(a => a.AttributeName, "PriceEpisodeCompletionPayment")
                .Build();

            var aecApprenticeshipPriceEpisodePeriodisedValuesThree = new AecPriceEpisodePeriodisedValueBuilder()
                .With(a => a.LearnRefNumber, "LearningRefNumber3")
                .With(a => a.AttributeName, "PriceEpisodeBalancePayment")
                .Build();

            var aecPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValues>()
            {
                aecApprenticeshipPriceEpisodePeriodisedValuesOne,
                aecApprenticeshipPriceEpisodePeriodisedValuesTwo,
                aecApprenticeshipPriceEpisodePeriodisedValuesThree
            };

            NewBuilder().NonZeroCompletionEarningsFilter(aecPriceEpisodePeriodisedValues, "LearningRefNumber1").Should().BeTrue();
            NewBuilder().NonZeroCompletionEarningsFilter(aecPriceEpisodePeriodisedValues, "LearningRefNumber2").Should().BeTrue();
            NewBuilder().NonZeroCompletionEarningsFilter(aecPriceEpisodePeriodisedValues, "LearningRefNumber3").Should().BeFalse();
        }

        [Fact]
        public void RecordKeysUnion_Test()
        {
            List<string> learnRefNumbers = new List<string>() { "055300807083", "055300807081" };

            List<AppsCoInvestmentRecordKey> appsKeys = new List<AppsCoInvestmentRecordKey>()
            {
                new AppsCoInvestmentRecordKey("055300807083",null,3, 0, 462, 1 ),
                new AppsCoInvestmentRecordKey("055300807083",null,3, 0, 466, 1 ),
                new AppsCoInvestmentRecordKey("055300807083",new DateTime(2019, 2, 21),3, 0, 462, 1 )
            };

            List<AppsCoInvestmentRecordKey> ilrKeys = new List<AppsCoInvestmentRecordKey>()
            {
                new AppsCoInvestmentRecordKey("055300807083",new DateTime(2019, 2, 21),3, 0, 462, 1 )
            };

            var result = NewBuilder(new AppsCoInvestmentRecordKeyEqualityComparer()).UnionKeys(learnRefNumbers, ilrKeys, appsKeys);

            result.Count().Should().Be(3);
        }

        private AppsCoInvestmentModelBuilder NewBuilder(
            IEqualityComparer<AppsCoInvestmentRecordKey> appsCoInvestmentEqualityComparer = null,
            IPaymentsBuilder paymentsBuilder = null,
            ILearnersBuilder learnersBuilder = null,
            ILearningDeliveriesBuilder learningDeliveriesBuilder = null)
        {
            return new AppsCoInvestmentModelBuilder(
                appsCoInvestmentEqualityComparer ?? Mock.Of<IEqualityComparer<AppsCoInvestmentRecordKey>>(),
                paymentsBuilder ?? Mock.Of<IPaymentsBuilder>(),
                learnersBuilder ?? Mock.Of<ILearnersBuilder>(),
                learningDeliveriesBuilder ?? Mock.Of<ILearningDeliveriesBuilder>());
        }
    }
}
