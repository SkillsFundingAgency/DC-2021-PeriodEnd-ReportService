using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class ProviderMonitoringsBuilderTests
    {
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

        private ProviderMonitoringsBuilder NewBuilder()
        {
            return new ProviderMonitoringsBuilder();
        }
    }
}
