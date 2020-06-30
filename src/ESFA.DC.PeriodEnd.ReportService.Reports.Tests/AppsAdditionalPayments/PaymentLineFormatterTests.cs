using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsAdditionalPayments
{
    public class PaymentLineFormatterTests
    {
        [Fact]
        public void ReplaceOldWithNew()
        {
            var payments = new List<Payment>
            {
                new Payment { LearningAimFundingLineType = "16-18 Apprenticeship (Employer on App Service)" },
                new Payment { LearningAimFundingLineType = "16-18 Apprenticeship (Employer on App Service) Levy funding" },
                new Payment { LearningAimFundingLineType = "16-18 Apprenticeship (Employer on App Service) Non-Levy funding" },
                new Payment { LearningAimFundingLineType = "16-18 Apprenticeship (From May 2017) Levy Contract" },
                new Payment { LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship1618 },
                new Payment { LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship1618NonProcured },
                new Payment { LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy" },
                new Payment { LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy Contract (procured)" },
                new Payment { LearningAimFundingLineType = "19+ Apprenticeship (Employer on App Service)" },
                new Payment { LearningAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Levy funding" },
                new Payment { LearningAimFundingLineType = "19+ Apprenticeship (Employer on App Service) Non-Levy funding" },
                new Payment { LearningAimFundingLineType = "19+ Apprenticeship (From May 2017) Levy Contract" },
                new Payment { LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship19Plus },
                new Payment { LearningAimFundingLineType = FundLineConstants.NonLevyApprenticeship19PlusNonProcured },
                new Payment { LearningAimFundingLineType = "19+ Apprenticeship Non-Levy Contract (procured)" },
            };

            var formatter = new PaymentLineFormatter() as IPaymentLineFormatter;

            // Format the Funding Line Type before grouping by it (Update old entries to new line type
            foreach (var payment in payments)
            {
                payment.LearningAimFundingLineType =
                    formatter.GetUpdatedFindingLineType(payment.LearningAimFundingLineType);
            }
            var groupedPayments = payments.GroupBy(p => p.LearningAimFundingLineType).ToList();

            payments.Count().Should().Be(15);
            groupedPayments.Count().Should().Be(13);

            groupedPayments
                .Single(gp => gp.Key == FundLineConstants.NonLevyApprenticeship1618NonProcured).Count()
                .Should().Be(2);
            groupedPayments
                .Single(gp => gp.Key == FundLineConstants.NonLevyApprenticeship19PlusNonProcured)
                .Count().Should().Be(2);

            groupedPayments.Any(gp => gp.Key == FundLineConstants.NonLevyApprenticeship1618).Should()
                .BeFalse();
            groupedPayments.Any(gp => gp.Key == FundLineConstants.NonLevyApprenticeship19Plus).Should()
                .BeFalse();

        }

        [Fact]
        public void GetValidPaymentTypes()
        {
            var formatter = new PaymentLineFormatter() as IPaymentLineFormatter;

            formatter.GetAdditionalPaymentType(4).Should().Be("Employer");
            formatter.GetAdditionalPaymentType(5).Should().Be("Provider");
            formatter.GetAdditionalPaymentType(6).Should().Be("Employer");
            formatter.GetAdditionalPaymentType(7).Should().Be("Provider");
            formatter.GetAdditionalPaymentType(16).Should().Be("Apprentice");
        }

        [Fact]
        public void InvalidPaymentTypeRaisesException()
        {
            var formatter = new PaymentLineFormatter() as IPaymentLineFormatter;
            Action act = () => formatter.GetAdditionalPaymentType(1);
            act.Should().Throw<ApplicationException>().Where(e => e.Message.Equals("Unexpected TransactionType [1]"));
        }

        [Theory]
        [InlineData(1, 4, "AppEntityName")]
        [InlineData(1, 6, "AppEntityName")]
        [InlineData(1, 1, null)]
        [InlineData(2, 4, null)]
        public void GetApprenticeshipLegalEntityName(byte contractType, byte transactionType, string expected)
        {
            var formatter = new PaymentLineFormatter() as IPaymentLineFormatter;
            var payment = new Payment
            {
                ContractType = contractType, TransactionType = transactionType,
                ApprenticeshipLegalEntityName = "AppEntityName"
            };

            var result = formatter.GetApprenticeshipLegalEntityName(payment);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 1, 2, null)]
        [InlineData(4, 1, 2, "1")]
        [InlineData(6, 1, 2, "2")]
        [InlineData(4, null, 2, "Not Available")]
        public void GetEmployerId(byte transactionType, int? learnDelEmpIdFirst, int ? learnDelEmpIdSecond, string expected)
        {
            var formatter = new PaymentLineFormatter() as IPaymentLineFormatter;
            var learningDeliver = new AecLearningDelivery
            {
                LearnDelEmpIdFirstAdditionalPaymentThreshold = learnDelEmpIdFirst,
                LearnDelEmpIdSecondAdditionalPaymentThreshold = learnDelEmpIdSecond
            }
            ;
            var payment = new Payment { TransactionType = transactionType };

            var result = formatter.GetEmployerId(learningDeliver, payment);

            result.Should().Be(expected);
        }
    }
}