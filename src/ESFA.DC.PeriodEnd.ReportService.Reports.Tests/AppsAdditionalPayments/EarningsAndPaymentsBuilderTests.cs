using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Components.DictionaryAdapter;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsAdditionalPayments
{
    public class EarningsAndPaymentsBuilderTests
    {
        [Theory]
        [InlineData(4, new string[] {"PriceEpisodeFirstEmp1618Pay", "PriceEpisodeSecondEmp1618Pay"})]
        [InlineData(5, new string[] {"PriceEpisodeFirstProv1618Pay", "PriceEpisodeSecondProv1618Pay"})]
        [InlineData(6, new string[] {"PriceEpisodeFirstEmp1618Pay", "PriceEpisodeSecondEmp1618Pay"})]
        [InlineData(7, new string[] {"PriceEpisodeFirstProv1618Pay", "PriceEpisodeSecondProv1618Pay"})]
        [InlineData(16, new string[] {"PriceEpisodeLearnerAdditionalPayment"})]
        public void GetAttributesForTransactionType(byte transType, string[] expected)
        {
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;

            var result = earningsAndPaymentsBuilder.GetAttributesForTransactionType(transType);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetAttributesForTransactionTypeThrowsException()
        {
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;
            Action act = () => earningsAndPaymentsBuilder.GetAttributesForTransactionType(1);
            act.Should().Throw<ApplicationException>().Where(e => e.Message.Equals("Unexpected TransactionType [1]"));

        }

        [Fact]
        public void GetEarningsForPeriod_EmptyArraysDontCrash()
        {
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;
            var periodisedValuesForPayment = new List<ApprenticeshipPriceEpisodePeriodisedValues> { };
            var attributeTypes = new string[] { };

            var result =
                earningsAndPaymentsBuilder.GetEarningsForPeriod(periodisedValuesForPayment, attributeTypes, 1);

            result.Should().Be(0);
        }

        [Fact]
        public void GetEarningsForPeriod_OnlyMatchesCorrectAttributesAndPeriod()
        {
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;
            var periodisedValuesForPayment = new List<ApprenticeshipPriceEpisodePeriodisedValues>
            {
                new ApprenticeshipPriceEpisodePeriodisedValues {AttributeName = "ATTRIB1", Period_1 = 1},
                new ApprenticeshipPriceEpisodePeriodisedValues {AttributeName = "ATTRIB2", Period_1 = 2},
                new ApprenticeshipPriceEpisodePeriodisedValues {AttributeName = "ATTRIB4", Period_1 = 4},
                new ApprenticeshipPriceEpisodePeriodisedValues {AttributeName = "ATTRIB2", Period_2 = 8},
            };
            var attributeTypes = new string[] {"attrib1", "attrib2"};

            var result =
                earningsAndPaymentsBuilder.GetEarningsForPeriod(periodisedValuesForPayment, attributeTypes, 1);

            result.Should().Be(3);
        }

        [Fact]
        public void GetEarningsForPeriodInvalidPeriodThrowsException()
        {
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;
            var periodisedValuesForPayment = new List<ApprenticeshipPriceEpisodePeriodisedValues> { };
            var attributeTypes = new string[] { };
            Action act = () =>
                earningsAndPaymentsBuilder.GetEarningsForPeriod(periodisedValuesForPayment, attributeTypes, 16);
            act.Should().Throw<ApplicationException>().Where(e => e.Message.Equals("Unexpected Period [16]"));
        }

        [Fact]
        public void BuildAssignsAcrossPeriods()
        {
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;
            var paymentAndLearningDeliveries = new List<PaymentAndLearningDelivery>();
            var periodisedValuesForPayment = new List<ApprenticeshipPriceEpisodePeriodisedValues>();

            var periodisedValuesType = typeof(ApprenticeshipPriceEpisodePeriodisedValues);

            for (int i = 1; i <= 14; i++)
            {
                paymentAndLearningDeliveries.Add(new PaymentAndLearningDelivery
                {
                    Payment = new Payment{ Amount = (decimal)Math.Pow(2, i), CollectionPeriod = (byte)i, TransactionType = 4 },
                    LearningDelivery = new LearningDelivery{ AimSequenceNumber = i }
                });
            }

            for (int i = 1; i <= 12; i++)
            {
                var periodisedValues = new ApprenticeshipPriceEpisodePeriodisedValues { AimSeqNumber = i, AttributeName = "PriceEpisodeFirstEmp1618Pay" };

                PropertyInfo pvInstance = periodisedValuesType.GetProperty($"Period_{i}");
                pvInstance.SetValue(periodisedValues, (decimal)Math.Pow(2, i) + 1000);

                periodisedValuesForPayment.Add(periodisedValues);
            }

            var result = earningsAndPaymentsBuilder.Build(paymentAndLearningDeliveries, periodisedValuesForPayment);

            result.TotalPaymentsYearToDate.Should().Be((decimal)Math.Pow(2, 15) - 2);
            result.TotalEarnings.Should().Be((decimal)Math.Pow(2, 13) - 2 + 12000);
        }
    }
}