using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class LearningDeliveryFamsBuilderTests
    {
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

        private LearningDeliveryFamsBuilder NewBuilder()
        {
            return new LearningDeliveryFamsBuilder();
        }
    }
}
