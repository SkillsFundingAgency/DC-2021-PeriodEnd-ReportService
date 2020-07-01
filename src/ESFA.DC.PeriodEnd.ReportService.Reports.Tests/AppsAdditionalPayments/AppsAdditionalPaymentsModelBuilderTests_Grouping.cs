using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentsModelBuilderTests_Grouping
    {
        [Fact]
        public void GroupingByPaymentType()
        {
            var payments = new List<Payment>
            {
                new Payment {LearnerReferenceNumber = "ABC", TransactionType =  4, LearningStartDate = new DateTime(2020, 01, 01), LearningAimProgrammeType = 1, LearningAimStandardCode = 2, LearningAimFrameworkCode = 3, LearningAimPathwayCode = 10},
                new Payment {LearnerReferenceNumber = "ABC", TransactionType =  5, LearningStartDate = new DateTime(2020, 01, 01), LearningAimProgrammeType = 1, LearningAimStandardCode = 2, LearningAimFrameworkCode = 3, LearningAimPathwayCode = 11}, 
                new Payment {LearnerReferenceNumber = "ABC", TransactionType =  6, LearningStartDate = new DateTime(2020, 01, 01), LearningAimProgrammeType = 1, LearningAimStandardCode = 2, LearningAimFrameworkCode = 3, LearningAimPathwayCode = 12},
                new Payment {LearnerReferenceNumber = "ABC", TransactionType =  7, LearningStartDate = new DateTime(2020, 01, 01), LearningAimProgrammeType = 1, LearningAimStandardCode = 2, LearningAimFrameworkCode = 3, LearningAimPathwayCode = 13},
                new Payment {LearnerReferenceNumber = "ABC", TransactionType = 16, LearningStartDate = new DateTime(2020, 01, 01), LearningAimProgrammeType = 1, LearningAimStandardCode = 2, LearningAimFrameworkCode = 3, LearningAimPathwayCode = 14},
            };

            var learners = new List<Learner>
            {
                new Learner{ LearnRefNumber = "ABC" }
            };


            var LearningDeliveries = new List<AecLearningDelivery>
            {
                new AecLearningDelivery{ LearnRefNumber = "ABC", LearnStartDate = new DateTime(2020, 01, 01), ProgType = 1, StdCode = 2, FworkCode = 3, PwayCode = 10, AimSequenceNumber = 1},
                new AecLearningDelivery{ LearnRefNumber = "ABC", LearnStartDate = new DateTime(2020, 01, 01), ProgType = 1, StdCode = 2, FworkCode = 3, PwayCode = 11, AimSequenceNumber = 2},
                new AecLearningDelivery{ LearnRefNumber = "ABC", LearnStartDate = new DateTime(2020, 01, 01), ProgType = 1, StdCode = 2, FworkCode = 3, PwayCode = 12, AimSequenceNumber = 3},
                new AecLearningDelivery{ LearnRefNumber = "ABC", LearnStartDate = new DateTime(2020, 01, 01), ProgType = 1, StdCode = 2, FworkCode = 3, PwayCode = 13, AimSequenceNumber = 4},
                new AecLearningDelivery{ LearnRefNumber = "ABC", LearnStartDate = new DateTime(2020, 01, 01), ProgType = 1, StdCode = 2, FworkCode = 3, PwayCode = 14, AimSequenceNumber = 5}

            };

            var periodisedValues = new List<ApprenticeshipPriceEpisodePeriodisedValues>();


            var paymentLineFormatter = new PaymentLineFormatter() as IPaymentLineFormatter;
            var earningsAndPaymentsBuilder = new EarningsAndPaymentsBuilder() as IEarningsAndPaymentsBuilder;
            var appsAdditionalPaymentsModelBuilder = new AppsAdditionalPaymentsModelBuilder(paymentLineFormatter, earningsAndPaymentsBuilder);

            var results = appsAdditionalPaymentsModelBuilder.Build(payments, learners, LearningDeliveries, periodisedValues);

            results.Count().Should().Be(3);
        }
    }
}