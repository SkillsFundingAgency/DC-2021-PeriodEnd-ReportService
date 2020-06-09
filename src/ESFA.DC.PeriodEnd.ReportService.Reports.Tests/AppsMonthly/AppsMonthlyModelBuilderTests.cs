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
using ProviderMonitoring = ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model.ProviderMonitoring;

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

            NewBuilder()
                .Build(
                    payments,
                    Array.Empty<Learner>(),
                    Array.Empty<ContractAllocation>(),
                    Array.Empty<Earning>(), 
                    Array.Empty<LarsLearningDelivery>(),
                    Array.Empty<AecApprenticeshipPriceEpisode>())
                .Should()
                .HaveCount(1);
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
        public void BuildProviderMonitorings()
        {
            var providerLearnerMonA = new ProviderMonitoringBuilder().With(m => m.Occur, "A").With(m => m.Mon, "MonA").Build();
            var providerLearnerMonB = new ProviderMonitoringBuilder().With(m => m.Occur, "B").With(m => m.Mon, "MonB").Build();

            var providerLearnerMonitorings = new List<ProviderMonitoring>()
            {
                providerLearnerMonA,
                providerLearnerMonB,
            };

            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMonitorings, providerLearnerMonitorings).Build();

            var providerLearnDelMonA = new ProviderMonitoringBuilder().With(m => m.Occur, "A").With(m => m.Mon, "DelMonA").Build();
            var providerLearnDelMonB = new ProviderMonitoringBuilder().With(m => m.Occur, "B").With(m => m.Mon, "DelMonB").Build();
            var providerLearnDelMonC = new ProviderMonitoringBuilder().With(m => m.Occur, "C").With(m => m.Mon, "DelMonC").Build();
            var providerLearnDelMonD = new ProviderMonitoringBuilder().With(m => m.Occur, "D").With(m => m.Mon, "DelMonD").Build();

            var providerLearnDelMonitorings = new List<ProviderMonitoring>()
            {
                providerLearnDelMonA,
                providerLearnDelMonB,
                providerLearnDelMonC,
                providerLearnDelMonD,
            };

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.ProviderSpecDeliveryMonitorings, providerLearnDelMonitorings).Build();

            var providerMonitorings = NewBuilder().BuildProviderMonitorings(learner, learningDelivery);

            providerMonitorings.LearnerA.Should().Be("MonA");
            providerMonitorings.LearnerB.Should().Be("MonB");

            providerMonitorings.LearningDeliveryA.Should().Be("DelMonA");
            providerMonitorings.LearningDeliveryB.Should().Be("DelMonB");
            providerMonitorings.LearningDeliveryC.Should().Be("DelMonC");
            providerMonitorings.LearningDeliveryD.Should().Be("DelMonD");
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NonMatching()
        {
            var providerLearnerMonC = new ProviderMonitoringBuilder().With(m => m.Occur, "C").Build();
            var providerLearnerMonD = new ProviderMonitoringBuilder().With(m => m.Occur, "D").Build();

            var providerLearnerMons = new List<ProviderMonitoring>()
            {
                providerLearnerMonC,
                providerLearnerMonD,
            };

            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMonitorings, providerLearnerMons).Build();

            var providerLearnDelMonE = new ProviderMonitoringBuilder().With(m => m.Occur, "E").With(m => m.Mon, "DelMonE").Build();
            var providerLearnDelMonF = new ProviderMonitoringBuilder().With(m => m.Occur, "F").With(m => m.Mon, "DelMonF").Build();
            var providerLearnDelMonG = new ProviderMonitoringBuilder().With(m => m.Occur, "G").With(m => m.Mon, "DelMonG").Build();
            var providerLearnDelMonH = new ProviderMonitoringBuilder().With(m => m.Occur, "H").With(m => m.Mon, "DelMonH").Build();

            var providerLearnDelMonitorings = new List<ProviderMonitoring>()
            {
                providerLearnDelMonE,
                providerLearnDelMonF,
                providerLearnDelMonG,
                providerLearnDelMonH,
            };

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.ProviderSpecDeliveryMonitorings, providerLearnDelMonitorings).Build();

            var providerMonitorings = NewBuilder().BuildProviderMonitorings(learner, learningDelivery);

            providerMonitorings.LearnerA.Should().BeNull();
            providerMonitorings.LearnerB.Should().BeNull();

            providerMonitorings.LearningDeliveryA.Should().BeNull();
            providerMonitorings.LearningDeliveryB.Should().BeNull();
            providerMonitorings.LearningDeliveryC.Should().BeNull();
            providerMonitorings.LearningDeliveryD.Should().BeNull();
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NullLearner()
        {
            var providerLearnDelMonA = new ProviderMonitoringBuilder().With(m => m.Occur, "A").With(m => m.Mon, "DelMonA").Build();
            var providerLearnDelMonB = new ProviderMonitoringBuilder().With(m => m.Occur, "B").With(m => m.Mon, "DelMonB").Build();
            var providerLearnDelMonC = new ProviderMonitoringBuilder().With(m => m.Occur, "C").With(m => m.Mon, "DelMonC").Build();
            var providerLearnDelMonD = new ProviderMonitoringBuilder().With(m => m.Occur, "D").With(m => m.Mon, "DelMonD").Build();

            var providerLearnDelMonitorings = new List<ProviderMonitoring>()
            {
                providerLearnDelMonA,
                providerLearnDelMonB,
                providerLearnDelMonC,
                providerLearnDelMonD,
            };

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.ProviderSpecDeliveryMonitorings, providerLearnDelMonitorings).Build();

            var providerMonitorings = NewBuilder().BuildProviderMonitorings(null, learningDelivery);

            providerMonitorings.LearnerA.Should().BeNull();
            providerMonitorings.LearnerB.Should().BeNull();

            providerMonitorings.LearningDeliveryA.Should().Be("DelMonA");
            providerMonitorings.LearningDeliveryB.Should().Be("DelMonB");
            providerMonitorings.LearningDeliveryC.Should().Be("DelMonC");
            providerMonitorings.LearningDeliveryD.Should().Be("DelMonD");
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NullLearningDelivery()
        {
            var providerLearnerMonA = new ProviderMonitoringBuilder().With(m => m.Occur, "A").With(m => m.Mon, "MonA").Build();
            var providerLearnerMonB = new ProviderMonitoringBuilder().With(m => m.Occur, "B").With(m => m.Mon, "MonB").Build();

            var providerLearnerMonitorings = new List<ProviderMonitoring>()
            {
                providerLearnerMonA,
                providerLearnerMonB,
            };

            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMonitorings, providerLearnerMonitorings).Build();

            var providerMonitorings = NewBuilder().BuildProviderMonitorings(learner, null);

            providerMonitorings.LearnerA.Should().Be("MonA");
            providerMonitorings.LearnerB.Should().Be("MonB");

            providerMonitorings.LearningDeliveryA.Should().BeNull();
            providerMonitorings.LearningDeliveryB.Should().BeNull();
            providerMonitorings.LearningDeliveryC.Should().BeNull();
            providerMonitorings.LearningDeliveryD.Should().BeNull();
        }

        [Fact]
        public void BuildProviderSpecLearnMonitoringsForLearner_NullProviderMons()
        {
            var learner = new LearnerBuilder().With(l => l.ProviderSpecLearnMonitorings, null).Build();

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.ProviderSpecDeliveryMonitorings, null).Build();

            NewBuilder().BuildProviderMonitorings(learner, learningDelivery).Should().BeNull();
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

            var learnerLookup = NewBuilder().BuildLearnerLookup(learners);

            learnerLookup.Should().HaveCount(2);

            learnerLookup["One"].Should().Be(learnerOne);
            learnerLookup["Two"].Should().Be(learnerTwo);
        }

        [Fact]
        public void BuildLearnerLookup_Empty()
        {
            NewBuilder().BuildLearnerLookup(Enumerable.Empty<Learner>()).Should().BeEmpty();
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

        [Fact]
        public void BuildLearningDeliveryFamsForLearningDelivery_Empty()
        {
            var learningDelivery = new LearningDeliveryBuilder()
                .With(ld => ld.LearningDeliveryFams, new List<LearningDeliveryFam>())
                .Build();

            var learningDeliveryFams = NewBuilder().BuildLearningDeliveryFamsForLearningDelivery(learningDelivery);

            learningDeliveryFams.LDM1.Should().BeNull();
            learningDeliveryFams.LDM2.Should().BeNull();
            learningDeliveryFams.LDM3.Should().BeNull();
            learningDeliveryFams.LDM4.Should().BeNull();
            learningDeliveryFams.LDM5.Should().BeNull();
            learningDeliveryFams.LDM6.Should().BeNull();
        }

        [Fact]
        public void BuildLearningDeliveryFamsForLearningDelivery_One()
        {
            var ldm = "LDM";

            var learningDelivery = new LearningDeliveryBuilder()
                .With(
                    ld => ld.LearningDeliveryFams,
                    new List<LearningDeliveryFam>()
                    {
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "1").Build(),
                    })
                .Build();

            var learningDeliveryFams = NewBuilder().BuildLearningDeliveryFamsForLearningDelivery(learningDelivery);

            learningDeliveryFams.LDM1.Should().Be("1");
            learningDeliveryFams.LDM2.Should().BeNull();
            learningDeliveryFams.LDM3.Should().BeNull();
            learningDeliveryFams.LDM4.Should().BeNull();
            learningDeliveryFams.LDM5.Should().BeNull();
            learningDeliveryFams.LDM6.Should().BeNull();
        }

        [Fact]
        public void BuildLearningDeliveryFamsForLearningDelivery_Six()
        {
            var ldm = "LDM";

            var learningDelivery = new LearningDeliveryBuilder()
                .With(
                    ld => ld.LearningDeliveryFams,
                    new List<LearningDeliveryFam>()
                    {
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "1").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "2").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "3").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "4").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "5").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "6").Build(),
                    })
                .Build();

            var learningDeliveryFams = NewBuilder().BuildLearningDeliveryFamsForLearningDelivery(learningDelivery);

            learningDeliveryFams.LDM1.Should().Be("1");
            learningDeliveryFams.LDM2.Should().Be("2");
            learningDeliveryFams.LDM3.Should().Be("3");
            learningDeliveryFams.LDM4.Should().Be("4");
            learningDeliveryFams.LDM5.Should().Be("5");
            learningDeliveryFams.LDM6.Should().Be("6");
        }

        [Fact]
        public void BuildLearningDeliveryFamsForLearningDelivery_Seven()
        {
            var ldm = "LDM";

            var learningDelivery = new LearningDeliveryBuilder()
                .With(
                    ld => ld.LearningDeliveryFams,
                    new List<LearningDeliveryFam>()
                    {
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "1").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "2").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "3").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "4").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "5").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "6").Build(),
                        new LearningDeliveryFamBuilder().With(f => f.Type, ldm).With(f => f.Code, "7").Build(),
                    })
                .Build();

            var learningDeliveryFams = NewBuilder().BuildLearningDeliveryFamsForLearningDelivery(learningDelivery);

            learningDeliveryFams.LDM1.Should().Be("1");
            learningDeliveryFams.LDM2.Should().Be("2");
            learningDeliveryFams.LDM3.Should().Be("3");
            learningDeliveryFams.LDM4.Should().Be("4");
            learningDeliveryFams.LDM5.Should().Be("5");
            learningDeliveryFams.LDM6.Should().Be("6");
        }

        [Fact]
        public void BuildLearningDeliveryFamsForLearningDelivery_NullLearningDelivery()
        {
            NewBuilder().BuildLearningDeliveryFamsForLearningDelivery(null).Should().BeNull();
        }

        [Fact]
        public void BuildLearningDeliveryFamsForLearningDelivery_NullLearningDeliveryFams()
        {
            NewBuilder().BuildLearningDeliveryFamsForLearningDelivery(new LearningDeliveryBuilder().With(ld => ld.LearningDeliveryFams, null).Build()).Should().BeNull();
        }

        [Fact]
        public void GetPriceEpisodeStartDateForRecord()
        {
            var priceEpisodeIdentifier = "SomeStuff-31/12/2019";

            var recordKey = new RecordKey(null, 1, "ZPROG001", null, 1, 1, 1, 1, null, priceEpisodeIdentifier);

            NewBuilder().GetPriceEpisodeStartDateForRecord(recordKey).Should().Be(new DateTime(2019, 12,31));
        }

        [Fact]
        public void GetPriceEpisodeStartDateForRecord_NullPriceEpisodeIdentifier()
        {
            var recordKey = new RecordKey(null, 1, "ZPROG001", null, 1, 1, 1, 1, null, null);

            NewBuilder().GetPriceEpisodeStartDateForRecord(recordKey).Should().BeNull();
        }

        [Fact]
        public void GetPriceEpisodeStartDateForRecord_ZPROG001()
        {
            var priceEpisodeIdentifier = "ThisIsNotADate";

            var recordKey = new RecordKey(null, 1, "ZPROG001", null, 1, 1, 1, 1, null, priceEpisodeIdentifier);

            NewBuilder().GetPriceEpisodeStartDateForRecord(recordKey).Should().BeNull();
        }

        [Fact]
        public void GetPriceEpisodeStartDateForRecord_Length()
        {
            var priceEpisodeIdentifier = "TooShort";

            var recordKey = new RecordKey(null, 1, "ZPROG001", null, 1, 1, 1, 1, null, priceEpisodeIdentifier);

            NewBuilder().GetPriceEpisodeStartDateForRecord(recordKey).Should().BeNull();
        }

        [Fact]
        public void GetPriceEpisodeStartDateForRecord_InvalidDate()
        {
            var priceEpisodeIdentifier = "ThisIsNotADate";

            var recordKey = new RecordKey(null, 1, "ZPROG001", null, 1, 1, 1, 1, null, priceEpisodeIdentifier);

            NewBuilder().GetPriceEpisodeStartDateForRecord(recordKey).Should().BeNull();
        }

        [Fact]
        public void GetLearnerEmploymentStatus()
        {
            var learnerEmploymentStatusEarliest = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 6, 1)).Build();
            var learnerEmploymentStatusAfter = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 10, 1)).Build();
            var learnerEmploymentStatusLatest = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 7, 1)).Build();

            var learner = new LearnerBuilder()
                .With(
                    l => l.LearnerEmploymentStatuses,
                    new List<LearnerEmploymentStatus>()
                    {
                        learnerEmploymentStatusEarliest,
                        learnerEmploymentStatusAfter,
                        learnerEmploymentStatusLatest,
                    })
                .Build();

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.LearnStartDate, new DateTime(2020, 8, 1)).Build();

            NewBuilder().GetLearnerEmploymentStatus(learner, learningDelivery).Should().BeSameAs(learnerEmploymentStatusLatest);
        }

        [Fact]
        public void GetLearnerEmploymentStatus_NullLearner()
        {
            NewBuilder().GetLearnerEmploymentStatus(null, new LearningDeliveryBuilder().Build()).Should().BeNull();
        }

        [Fact]
        public void GetLearnerEmploymentStatus_NullLearnEmploymentStatus()
        {
            var learner = new LearnerBuilder()
                .With(l => l.LearnerEmploymentStatuses, null)
                .Build();

            NewBuilder().GetLearnerEmploymentStatus(learner, new LearningDeliveryBuilder().Build()).Should().BeNull();
        }

        [Fact]
        public void GetLearnerEmploymentStatus_NullLearnerDelivery()
        {
            NewBuilder().GetLearnerEmploymentStatus(new LearnerBuilder().Build(), null).Should().BeNull();
        }

        [Fact]
        public void GetLearnerEmploymentStatus_EmptySet()
        {
            var learner = new LearnerBuilder()
                .With(l => l.LearnerEmploymentStatuses, new List<LearnerEmploymentStatus>())
                .Build();

            NewBuilder().GetLearnerEmploymentStatus(learner, null).Should().BeNull();
        }

        [Fact]
        public void GetLearnerEmploymentStatus_NoneBeforeLearnStartDate()
        {
            var learnerEmploymentStatusOne = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 9, 1)).Build();
            var learnerEmploymentStatusTwo = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 10, 1)).Build();


            var learner = new LearnerBuilder()
                .With(
                    l => l.LearnerEmploymentStatuses,
                    new List<LearnerEmploymentStatus>()
                    {
                        learnerEmploymentStatusOne,
                        learnerEmploymentStatusTwo,
                    })
                .Build();

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.LearnStartDate, new DateTime(2020, 8, 1)).Build();

            NewBuilder().GetLearnerEmploymentStatus(learner, learningDelivery).Should().BeNull();
        }

        [Fact]
        public void GetLearnerEmploymentStatus_MultipleBeforeLearnStartDate()
        {
            var learnerEmploymentStatusOne = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 6, 1)).Build();
            var learnerEmploymentStatusTwo = new LearnerEmploymentStatusBuilder().With(s => s.DateEmpStatApp, new DateTime(2020, 7, 1)).Build();
            
            var learner = new LearnerBuilder()
                .With(
                    l => l.LearnerEmploymentStatuses,
                    new List<LearnerEmploymentStatus>()
                    {
                        learnerEmploymentStatusOne,
                        learnerEmploymentStatusTwo,
                    })
                .Build();

            var learningDelivery = new LearningDeliveryBuilder().With(ld => ld.LearnStartDate, new DateTime(2020, 8, 1)).Build();

            NewBuilder().GetLearnerEmploymentStatus(learner, learningDelivery).Should().BeSameAs(learnerEmploymentStatusTwo);
        }

        private AppsMonthlyModelBuilder NewBuilder()
        {
            return new AppsMonthlyModelBuilder();
        }
    }
}
