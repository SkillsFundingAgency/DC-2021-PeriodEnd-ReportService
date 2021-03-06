﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentsModelBuilder : IAppsAdditionalPaymentsModelBuilder
    {
        private readonly IPaymentLineFormatter _paymentFundingLineFormatter;
        private readonly IEarningsAndPaymentsBuilder _earningsAndPaymentsBuilder;
        private const string NotApplicable = "Not applicable. For more information refer to the funding reports guidance.";


        public AppsAdditionalPaymentsModelBuilder(IPaymentLineFormatter paymentFundingLineFormatter, IEarningsAndPaymentsBuilder earningsAndPaymentsBuilder)
        {
            _paymentFundingLineFormatter = paymentFundingLineFormatter;
            _earningsAndPaymentsBuilder = earningsAndPaymentsBuilder;
        }

        public IEnumerable<AppsAdditionalPaymentReportModel> Build(
            ICollection<Payment> payments,
            ICollection<Learner> learners,
            ICollection<AecLearningDelivery> learningDeliveries,
            ICollection<ApprenticeshipPriceEpisodePeriodisedValues> periodisedValues)
        {
            // Format the Funding Line Type before grouping by it (Update old entries to new line type
            foreach (var payment in payments)
            {
                payment.ReportingAimFundingLineType =
                    _paymentFundingLineFormatter.GetUpdatedFindingLineType(payment.ReportingAimFundingLineType);
            }

            var learnerLookup = learners.ToDictionary(l => l.LearnRefNumber, l => l, StringComparer.OrdinalIgnoreCase);
            var periodisedValuesLookup = periodisedValues.GroupBy(pv => pv.LearnRefNumber).ToDictionary(pvg => pvg.Key);

            List<PaymentAndLearningDelivery> matchedPaymentsAndLearningDeliveries = MatchPaymentAndLearningDeliveries(payments, learningDeliveries);

            return matchedPaymentsAndLearningDeliveries
                .GroupBy(pld => new RecordKey(pld.Payment.LearnerReferenceNumber,
                    pld.Payment.LearnerUln,
                    pld.Payment.LearningStartDate,
                    pld.Payment.ReportingAimFundingLineType,
                    _paymentFundingLineFormatter.GetAdditionalPaymentType(pld.Payment.TransactionType),
                    _paymentFundingLineFormatter.GetApprenticeshipLegalEntityName(pld.Payment),
                    _paymentFundingLineFormatter.GetEmployerId(pld.LearningDelivery, pld.Payment)))
                .Select(paymentLine =>
                {
                    var learner = learnerLookup.GetValueOrDefault(paymentLine.Key.LearnerReferenceNumber);
                    var periodisedValuesForLearner = periodisedValuesLookup.GetValueOrDefault(paymentLine.Key.LearnerReferenceNumber);

                    return new AppsAdditionalPaymentReportModel
                    {
                        RecordKey = paymentLine.Key,
                        FamilyName =  learner?.LearnRefNumber != null ? learner.FamilyName : NotApplicable,
                        GivenNames = learner?.LearnRefNumber != null ? learner.GivenNames : NotApplicable,
                        ProviderSpecifiedLearnerMonitoringA =  learner?.ProvSpecLearnMonA,
                        ProviderSpecifiedLearnerMonitoringB =  learner?.ProvSpecLearnMonB,
                        EarningsAndPayments = _earningsAndPaymentsBuilder.Build(paymentLine.AsEnumerable(), periodisedValuesForLearner.AsEnumerable())
                    } ;
                });
        }

        private List<PaymentAndLearningDelivery> MatchPaymentAndLearningDeliveries(ICollection<Payment> payments, ICollection<AecLearningDelivery> learningDeliveries)
        {
            var learningDeliveriesLookup = learningDeliveries.GroupBy(ld => ld.LearnRefNumber).ToDictionary(ldg => ldg.Key);

            return payments.Select(p =>
            {
                var matchedLearningDelivery = GetLearningDelivery(learningDeliveriesLookup, p);

                return new PaymentAndLearningDelivery { Payment = p, LearningDelivery = matchedLearningDelivery };
            }).ToList();
        }

        private static AecLearningDelivery GetLearningDelivery(
            Dictionary<string, IGrouping<string, AecLearningDelivery>> learningDeliveriesLookup, Payment payment)
        {
            var learningDelivery = learningDeliveriesLookup.GetValueOrDefault(payment.LearnerReferenceNumber)?.FirstOrDefault(
                ld => ld.LearnAimRef.CaseInsensitiveEquals(payment.LearningAimReference)
                      && ld.LearnStartDate == payment.LearningStartDate
                      && payment.LearningAimProgrammeType == (ld.ProgType ?? 0)
                      && payment.LearningAimStandardCode == (ld.StdCode ?? 0)
                      && payment.LearningAimFrameworkCode == (ld.FworkCode ?? 0)
                      && payment.LearningAimPathwayCode == (ld.PwayCode ?? 0));

            return learningDelivery;
        }
    }
}
