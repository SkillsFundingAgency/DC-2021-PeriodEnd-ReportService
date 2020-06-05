using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;
using FluentAssertions;
using Xunit;
using Payment = ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model.Payment;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class AppsMonthlyModelBuilderTests
    {
        [Fact]
        public void Grouping()
        {
            var paymentOne = new Payment();
            var paymentTwo = new Payment();


            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            NewBuilder().Build(payments, Array.Empty<Learner>(), Array.Empty<ContractAllocation>(), Array.Empty<Earning>(), Array.Empty<LarsLearningDelivery>()).Should().HaveCount(1);
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey()
        {
            var matchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef,"LearnAimRef")
                .Build();

            var nonMatchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef, "NotLearnAimRef")
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                matchingLearningDelivery,
                nonMatchingLearningDelivery,
            };

            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40, 
                10, 
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(learner, recordKey).Should().BeSameAs(matchingLearningDelivery);
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey_NoMatch()
        {
            var nonMatchingLearningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef, "NotLearnAimRef")
                .Build();

            var learningDeliveries = new List<LearningDelivery>()
            {
                nonMatchingLearningDelivery,
            };

            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, learningDeliveries)
                .Build();

            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(learner, recordKey).Should().BeNull();
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey_NullLearner()
        {
            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(null, recordKey).Should().BeNull();
        }

        [Fact]
        public void GetLearningDeliveryForRecordKey_NullLearningDeliveries()
        {
            var learner = new LearnerBuilder()
                .With(l => l.LearningDeliveries, null)
                .Build();

            var recordKey = new RecordKey(
                "1",
                123456789,
                "LearnAimRef",
                new DateTime(2020, 8, 1),
                20,
                40,
                10,
                30,
                "ReportingAimFundingLineType", "PriceEpisodeIdentifier");

            NewBuilder().GetLearnerLearningDeliveryForRecord(learner, recordKey).Should().BeNull();
        }

        [Theory]
        [InlineData("16-18 Apprenticeship (From May 2017) Levy Contract", "LEVY1799")]
        [InlineData("16-18 Apprenticeship (Employer on App Service) Levy funding", "LEVY1799")]
        [InlineData("19+ Apprenticeship (From May 2017) Levy Contract",	"LEVY1799")]
        [InlineData("19+ Apprenticeship (Employer on App Service) Levy funding", "LEVY1799")]
        [InlineData("16-18 Apprenticeship (From May 2017) Non-Levy Contract", "APPS2021")]
        [InlineData("16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)", "APPS2021")]
        [InlineData("16-18 Apprenticeship Non-Levy Contract (procured)", "16-18NLAP2018")]
        [InlineData("16-18 Apprenticeship (Employer on App Service) Non-Levy funding", "NONLEVY2019")]
        [InlineData("19+ Apprenticeship (From May 2017) Non-Levy Contract", "APPS2021")]
        [InlineData("19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)", "APPS2021")]
        [InlineData("19+ Apprenticeship Non-Levy Contract (procured)", "ANLAP2018")]
        [InlineData("19+ Apprenticeship (Employer on App Service) Non-Levy funding", "NONLEVY2019")]
        [InlineData("NotAMatch", null)]
        public void FundingStreamPeriodForReportingAimFundingLineType(string reportingAimFundingLineType, string fundingStreamPeriod)
        {
            NewBuilder().FundingStreamPeriodForReportingAimFundingLineType(reportingAimFundingLineType).Should().Be(fundingStreamPeriod);
        }

        [Fact]
        public void GetLatestPaymentWithEarningEvent_CollectionPeriod()
        {
            var paymentOne = new PaymentBuilder().With<byte>(p => p.CollectionPeriod, 2).Build();
            var paymentTwo = new PaymentBuilder().With<byte>(p => p.CollectionPeriod, 3).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            NewBuilder().GetLatestPaymentWithEarningEvent(payments).Should().BeSameAs(paymentTwo);
        }

        [Fact]
        public void GetLatestPaymentWithEarningEvent_DeliveryPeriod()
        {
            var paymentOne = new PaymentBuilder().With<byte>(p => p.CollectionPeriod, 2).With<byte>(p => p.DeliveryPeriod, 2).Build();
            var paymentTwo = new PaymentBuilder().With<byte>(p => p.CollectionPeriod, 2).With<byte>(p => p.DeliveryPeriod, 3).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            NewBuilder().GetLatestPaymentWithEarningEvent(payments).Should().BeSameAs(paymentTwo);
        }


        [Fact]
        public void GetLatestPaymentWithEarningEvent_Empty()
        {
            NewBuilder().GetLatestPaymentWithEarningEvent(Enumerable.Empty<Payment>()).Should().BeNull();
        }

        [Fact]
        public void GetLatestPaymentWithEarningEvent_NoEarningEvents()
        {
            var paymentOne = new PaymentBuilder().With(p => p.EarningEventId, null).Build();
            var paymentTwo = new PaymentBuilder().With(p => p.EarningEventId, new Guid()).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            NewBuilder().GetLatestPaymentWithEarningEvent(payments).Should().BeNull();
        }

        [Fact]
        public void GetLatestRefundPaymentWithEarningEvent()
        {
            var paymentOne = new PaymentBuilder()
                .With(p => p.LearningStartDate, null)
                .Build();
            var paymentTwo = new PaymentBuilder()
                .With(p => p.LearningStartDate, null)
                .With(p => p.LearnerReferenceNumber, "NotLearnerReferenceNumber").Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            var recordKey = new RecordKey(
                PaymentBuilder.LearnerReferenceNumber,
                PaymentBuilder.LearnerUln,
                PaymentBuilder.LearningAimReference,
                PaymentBuilder.LearningStartDate,
                PaymentBuilder.ProgrammeType,
                PaymentBuilder.StandardCode,
                PaymentBuilder.FrameworkCode,
                PaymentBuilder.PathwayCode,
                PaymentBuilder.ReportingAimFundingLineType,
                PaymentBuilder.PriceEpisodeIdentifier);

            NewBuilder().GetLatestRefundPaymentWithEarningEventForRecord(recordKey, payments).Should().BeSameAs(paymentOne);
        }
        
        [Fact]
        public void GetLatestRefundPaymentWithEarningEvent_Empty()
        {
            NewBuilder().GetLatestRefundPaymentWithEarningEventForRecord(new RecordKey(), Enumerable.Empty<Payment>()).Should().BeNull();
        }

        [Fact]
        public void GetLatestRefundPaymentWithEarningEvent_NoEarningEvents()
        {
            var paymentOne = new PaymentBuilder().With(p => p.EarningEventId, null).Build();
            var paymentTwo = new PaymentBuilder().With(p => p.EarningEventId, Guid.Empty).Build();

            var payments = new List<Payment>()
            {
                paymentOne,
                paymentTwo,
            };

            NewBuilder().GetLatestRefundPaymentWithEarningEventForRecord(new RecordKey(), payments).Should().BeNull();
        }


        [Fact]
        public void GetEarningForPayment()
        {
            var payment = new PaymentBuilder().With(p => p.EarningEventId, EarningBuilder.EventId).Build();

            var matchingEarning = new EarningBuilder().Build();
            var nonMatchingEarning = new EarningBuilder().Build();

            var earningsLookup = new Dictionary<Guid, Earning>()
            {
                [Guid.Empty] = nonMatchingEarning,
                [EarningBuilder.EventId] = matchingEarning,
            };

            NewBuilder().GetEarningForPayment(earningsLookup, payment).Should().BeSameAs(matchingEarning);
        }

        [Fact]
        public void GetEarningForPayment_NullPayment()
        {
            NewBuilder().GetEarningForPayment(new Dictionary<Guid, Earning>(), null).Should().BeNull();
        }

        [Fact]
        public void GetEarningForPayment_NullEarningEventId()
        {
            var payment = new PaymentBuilder().With(e => e.EarningEventId, null).Build();

            NewBuilder().GetEarningForPayment(new Dictionary<Guid, Earning>(), payment).Should().BeNull();
        }

        [Fact]
        public void GetEarningForPayment_NoEarnings()
        {
            var payment = new PaymentBuilder().Build();

            NewBuilder().GetEarningForPayment(new Dictionary<Guid, Earning>(), payment).Should().BeNull();
        }

        [Fact]
        public void GetEarningForPayment_NoMatchingEarnings()
        {
            var payment = new PaymentBuilder().Build();

            var earningsLookup = new Dictionary<Guid, Earning>()
            {
                [Guid.Empty] = new EarningBuilder().Build(),
            };

            NewBuilder().GetEarningForPayment(earningsLookup, payment).Should().BeNull();
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner()
        {
            var providerSpecDelMonA = new ProviderSpecLearnMonBuilder().With(m => m.ProvSpecLearnMonOccur, "A").With(m => m.ProvSpecLearnMon, "MonA").Build();
            var providerSpecDelMonB = new ProviderSpecLearnMonBuilder().With(m => m.ProvSpecLearnMonOccur, "B").With(m => m.ProvSpecLearnMon, "MonB").Build();

            var providerSpecDelMons = new List<ProviderSpecLearnMon>()
            {
                providerSpecDelMonA,
                providerSpecDelMonB,
            };

            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMons, providerSpecDelMons).Build();

            var providerSpecDelLearnMonitorings = NewBuilder().BuildProviderSpecLearnMonitoringsForLearner(learner);

            providerSpecDelLearnMonitorings.A.Should().Be("MonA");
            providerSpecDelLearnMonitorings.B.Should().Be("MonB");
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NonMatching()
        {
            var providerSpecDelMonC = new ProviderSpecLearnMonBuilder().With(m => m.ProvSpecLearnMonOccur, "C").Build();
            var providerSpecDelMonD = new ProviderSpecLearnMonBuilder().With(m => m.ProvSpecLearnMonOccur, "D").Build();

            var providerSpecDelMons = new List<ProviderSpecLearnMon>()
            {
                providerSpecDelMonC,
                providerSpecDelMonD,
            };

            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMons, providerSpecDelMons).Build();

            var providerSpecDelLearnMonitorings = NewBuilder().BuildProviderSpecLearnMonitoringsForLearner(learner);

            providerSpecDelLearnMonitorings.A.Should().BeNull();
            providerSpecDelLearnMonitorings.B.Should().BeNull();
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NullLearner()
        {
            NewBuilder().BuildProviderSpecLearnMonitoringsForLearner(null).Should().BeNull();
        }
        
        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NullProviderSpecDelMons()
        {
            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMons, null).Build();

            NewBuilder().BuildProviderSpecLearnMonitoringsForLearner(learner).Should().BeNull();
        }
        
        [Fact]
        public void BuildLearnerLookup()
        {
            var learnerOne = new LearnerBuilder().With(l => l.LearnRefNumber, "One").Build();
            var learnerTwo = new LearnerBuilder().With(l => l.LearnRefNumber, "Two").Build();

            var learners = new List<Learner>()
            {
                learnerOne,
                learnerTwo,
            };

            var learnerLookup = NewBuilder().BuildLearnerDictionary(learners);

            learnerLookup.Should().HaveCount(2);

            learnerLookup["One"].Should().Be(learnerOne);
            learnerLookup["Two"].Should().Be(learnerTwo);
        }

        [Fact]
        public void BuildLearnerLookup_Empty()
        {
            NewBuilder().BuildLearnerDictionary(Enumerable.Empty<Learner>()).Should().BeEmpty();
        }

        [Fact]
        public void BuildLarsLearningDeliveryTitleLookup()
        {
            var larsLearningDeliveryOne = new LarsLearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef, "LearnAimRef1")
                .With(ld => ld.LearnAimRefTitle, "LearnAimRefTitle1")
                .Build();

            var larsLearningDeliveryTwo = new LarsLearningDeliveryBuilder()
                .With(ld => ld.LearnAimRef, "LearnAimRef2")
                .With(ld => ld.LearnAimRefTitle, "LearnAimRefTitle2")
                .Build();

            var larsLearningDeliveries = new List<LarsLearningDelivery>()
            {
                larsLearningDeliveryOne,
                larsLearningDeliveryTwo,
            };

            var larsLearningDeliveryTitleLookup =  NewBuilder().BuildLarsLearningDeliveryTitleLookup(larsLearningDeliveries);

            larsLearningDeliveryTitleLookup.Should().HaveCount(2);

            larsLearningDeliveryTitleLookup["LearnAimRef1"].Should().Be("LearnAimRefTitle1");
            larsLearningDeliveryTitleLookup["LearnAimRef2"].Should().Be("LearnAimRefTitle2");
        }
        
        [Fact]
        public void BuildLarsLearningDeliveryTitleLookup_Empty()
        {
            NewBuilder().BuildLarsLearningDeliveryTitleLookup(Enumerable.Empty<LarsLearningDelivery>()).Should().BeEmpty();
        }

        [Fact]
        public void BuildContractNumberLookup()
        {
            var contractAllocations = new List<ContractAllocation>()
            {
                new ContractAllocationBuilder().With(ca => ca.FundingStreamPeriod, "One").With(ca => ca.ContractAllocationNumber, "ABC").Build(),
                new ContractAllocationBuilder().With(ca => ca.FundingStreamPeriod, "One").With(ca => ca.ContractAllocationNumber, "DEF").Build(),
                new ContractAllocationBuilder().With(ca => ca.FundingStreamPeriod, "Two").With(ca => ca.ContractAllocationNumber, "GHI").Build(),
            };

            var contractNumberLookup = NewBuilder().BuildContractNumbersLookup(contractAllocations);

            contractNumberLookup.Should().HaveCount(2);
            contractNumberLookup["One"].Should().Be("ABC; DEF");

            contractNumberLookup["Two"].Should().Be("GHI");
        }

        [Fact]
        public void BuildContractNumberLookup_EmptySet()
        {
            NewBuilder().BuildContractNumbersLookup(Enumerable.Empty<ContractAllocation>()).Should().BeEmpty();
        }

        [Fact]
        public void BuildEarningsLookup()
        {
            var eventIdOne = Guid.NewGuid();
            var eventIdtwo = Guid.NewGuid();

            var earningOne = new EarningBuilder().With(e => e.EventId, eventIdOne).Build();
            var earningTwo = new EarningBuilder().With(e => e.EventId, eventIdtwo).Build();

            var earnings = new List<Earning>()
            {
                earningOne,
                earningTwo,
            };

            var earningsLookup = NewBuilder().BuildEarningsLookup(earnings);

            earningsLookup.Should().HaveCount(2);
            earningsLookup[eventIdOne].Should().BeSameAs(earningOne);
            earningsLookup[eventIdtwo].Should().BeSameAs(earningTwo);
        }

        [Fact]
        public void BuildEarningsLookup_EmptySet()
        {
            NewBuilder().BuildEarningsLookup(Enumerable.Empty<Earning>()).Should().BeEmpty();
        }

        private AppsMonthlyModelBuilder NewBuilder()
        {
            return new AppsMonthlyModelBuilder();
        }
    }
}
