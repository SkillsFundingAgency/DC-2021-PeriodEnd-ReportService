﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.UYPSummaryView.Model.Comparer;
using ESFA.DC.Logging.Interfaces;
using System.Diagnostics;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.UYPSummaryView
{
    public class UYPSummaryViewModelBuilder : IUYPSummaryViewModelBuilder
    {
        private readonly HashSet<byte?> _fundingSourceCoInvestmentPayments = new HashSet<byte?>() { 2 };

        private readonly HashSet<byte?> _transactionTypesCoInvestmentPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly ILogger _logger;

        private static readonly DataLockComparer _dataLockComparer = new DataLockComparer();
        private static readonly PriceEpisodeEarningComparer _peEarningComparer = new PriceEpisodeEarningComparer();

        private readonly HashSet<string> _peAttributeGroup = new HashSet<string>()
        {
            AttributeConstants.Fm36PriceEpisodeCompletionPaymentAttributeName,
            AttributeConstants.Fm36PriceEpisodeOnProgPaymentAttributeName,
            AttributeConstants.Fm3PriceEpisodeBalancePaymentAttributeName,
            AttributeConstants.Fm36PriceEpisodeLSFCashAttributeName
        };

        private readonly HashSet<string> _ldAttributeGroup = new HashSet<string>()
        {
            AttributeConstants.Fm36MathEngOnProgPayment,
            AttributeConstants.Fm36LearnSuppFundCash,
            AttributeConstants.Fm36MathEngBalPayment,
            AttributeConstants.Fm36DisadvFirstPayment,
            AttributeConstants.Fm36DisadvSecondPayment,
            AttributeConstants.Fm36LearnDelFirstEmp1618Pay,
            AttributeConstants.Fm36LearnDelSecondEmp1618Pay,
            AttributeConstants.Fm36LearnDelFirstProv1618Pay,
            AttributeConstants.Fm36LearnDelSecondProv1618Pay,
            AttributeConstants.Fm36LearnDelLearnAddPayment,
            AttributeConstants.Fm36LDApplic1618FrameworkUpliftBalancingPayment,
            AttributeConstants.Fm36LDApplic1618FrameworkUpliftCompletionPayment,
            AttributeConstants.Fm36LDApplic1618FrameworkUpliftOnProgPayment
        };

        public UYPSummaryViewModelBuilder(
            ILogger logger)
        {
            _logger = logger;
        }

        public ICollection<LearnerLevelViewModel> Build(
            ICollection<Payment> payments,
            ICollection<Learner> learners,
            ICollection<LearningDeliveryEarning> ldEarnings,
            ICollection<PriceEpisodeEarning> peEarnings,
            ICollection<CoInvestmentInfo> coInvestmentInfo,
            ICollection<DataLock> datalocks,
            ICollection<HBCPInfo> hbcpInfo,
            IDictionary<long, string> legalEntityNameDictionary,
            int returnPeriod,
            int ukprn)
        {
            // this variable is the final report and is the return value of this method.
            List<LearnerLevelViewModel> learnerLevelViewModelList = null;

            try
            {
                // Lookup for employer IDs to employer name
                IDictionary<int, string> employerNamesToIDs = new Dictionary<int, string>();

                // Build dictionaries to allow quicker processing of the larger datasets
                var paymentsDictionary = payments.GroupBy(e => e.LearnerReferenceNumber, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(e => e.Key, e => e.ToList(), StringComparer.OrdinalIgnoreCase);
                
                var ilrLearnersDict = learners.ToDictionary(t => t.LearnRefNumber, StringComparer.OrdinalIgnoreCase);

                var newLdEarningsDictionary = ldEarnings.GroupBy(e => e.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(e => e.Key, e => e.ToList(), StringComparer.OrdinalIgnoreCase);

                var newPeEarningsDictionary = peEarnings.GroupBy(e => e.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(e => e.Key, e => e.ToList(), StringComparer.OrdinalIgnoreCase);

                var dataLockHashset = datalocks.ToImmutableHashSet(_dataLockComparer);

                // Union the keys from the datasets being used to source the report
                var unionedKeys = UnionKeys(payments, ldEarnings, peEarnings);

                // Populate the learner list using the ILR query first
                learnerLevelViewModelList = unionedKeys
                .Select(record =>
                {
                    var reportRecord = new LearnerLevelViewModel()
                    {
                        Ukprn = ukprn,
                        PaymentLearnerReferenceNumber = record
                    };

                    // Extract ILR info
                    if (ilrLearnersDict.TryGetValue(record, out var ilrRecord))
                    {
                        reportRecord.FamilyName = ilrRecord.FamilyName;
                        reportRecord.GivenNames = ilrRecord.GivenNames;
                        reportRecord.PaymentUniqueLearnerNumbers = ilrRecord.ULN.ToString();
                        if ((ilrRecord.LearnerEmploymentStatuses != null) && (ilrRecord.LearnerEmploymentStatuses.Count > 0))
                        {
                            reportRecord.LearnerEmploymentStatusEmployerId = ilrRecord.LearnerEmploymentStatuses.Where(les => les.LearnRefNumber.CaseInsensitiveEquals(reportRecord.PaymentLearnerReferenceNumber) &&
                                                                                                                        les?.EmpStat == 10)
                                                                                                                .OrderByDescending(les => les?.DateEmpStatApp)
                                                                                                                .FirstOrDefault()?.EmpId;
                        }
                    }

                    if (paymentsDictionary.TryGetValue(reportRecord.PaymentLearnerReferenceNumber, out var paymentValues))
                    {
                        // Assign the amounts
                        reportRecord.PlannedPaymentsToYouToDate = paymentValues.Where(p => p.CollectionPeriod <= returnPeriod &&
                            (TotalESFAPlannedPaymentFSTypePredicate(p) ||
                             TotalESFAPlannedPaymentsTTTypePredicate(p) ||
                             TotalESFAPlannedPaymentsNonZPTypePredicate(p))).Sum(c => c.Amount);

                        reportRecord.ESFAPlannedPaymentsThisPeriod = paymentValues.Where(p => p.CollectionPeriod == returnPeriod &&
                            (TotalESFAPlannedPaymentFSTypePredicate(p) ||
                             TotalESFAPlannedPaymentsTTTypePredicate(p) ||
                             TotalESFAPlannedPaymentsNonZPTypePredicate(p))).Sum(c => c.Amount);

                        reportRecord.CoInvestmentPaymentsToCollectThisPeriod = paymentValues.Where(p => p.CollectionPeriod == returnPeriod &&
                            TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(p)).Sum(c => c.Amount);

                        // NOTE: Additional earnings calc required for this field
                        reportRecord.CoInvestmentOutstandingFromEmplToDate = paymentValues.Where(p => p.CollectionPeriod <= returnPeriod &&
                            TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(p)).Sum(c => c.Amount);

                        // Add any further ULNs found
                        var otherULNs = paymentValues.Select(x=>x.LearnerUln.ToString()).Distinct().ToList();
                        var existingULNS = reportRecord.PaymentUniqueLearnerNumbers.Split(';');
                        foreach (string ULN in otherULNs)
                        {
                            if (!existingULNS.Contains(ULN))
                            {
                                reportRecord.PaymentUniqueLearnerNumbers = (reportRecord.PaymentUniqueLearnerNumbers == null ? ULN: reportRecord.PaymentUniqueLearnerNumbers + ";" + ULN);
                            }
                        }

                        // Extract company name
                        if ((legalEntityNameDictionary != null) && legalEntityNameDictionary.TryGetValue(paymentValues.First().ApprenticeshipId, out var employerName))
                        {
                            reportRecord.EmployerName = employerName;
                            if ((reportRecord.LearnerEmploymentStatusEmployerId != null) && !employerNamesToIDs.ContainsKey(reportRecord.LearnerEmploymentStatusEmployerId ?? 0))
                            {
                                employerNamesToIDs.Add(new KeyValuePair<int, string>(reportRecord.LearnerEmploymentStatusEmployerId ?? 0, reportRecord.EmployerName));
                            }
                        }
                        else
                        {
                            reportRecord.EmployerName = string.Empty;
                        }
                    }

                    // Extract coInvestmentInfo - NOTE: is possible to have duplicate LearnerRef numbers in this list
                    if (coInvestmentInfo != null)
                    {
                        var learningDeliveries = coInvestmentInfo.Where(p => p.LearnRefNumber == reportRecord.PaymentLearnerReferenceNumber).ToList();

                        if (learningDeliveries.Any())
                        {
                            reportRecord.TotalCoInvestmentCollectedToDate =
                                learningDeliveries.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                                learningDeliveries.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);
                        }
                    }

                    // Work out total earnings
                    var ldLearner = newLdEarningsDictionary.GetValueOrDefault(reportRecord.PaymentLearnerReferenceNumber, Enumerable.Empty<LearningDeliveryEarning>().ToList());
                    var peLearner = newPeEarningsDictionary.GetValueOrDefault(reportRecord.PaymentLearnerReferenceNumber, Enumerable.Empty<PriceEpisodeEarning>().ToList());

                    reportRecord.TotalEarningsToDate = CalculatePriceEpisodeEarningsToPeriod(ldLearner, peLearner, true, returnPeriod, reportRecord) +
                                                        CalculateLearningDeliveryEarningsToPeriod(ldLearner, true, returnPeriod, reportRecord);

                    reportRecord.TotalEarningsForPeriod = CalculatePriceEpisodeEarningsToPeriod(ldLearner, peLearner, false, returnPeriod, reportRecord) +
                                                            CalculateLearningDeliveryEarningsToPeriod(ldLearner, false, returnPeriod, reportRecord);

                    // Default any null valued records
                    reportRecord.ESFAPlannedPaymentsThisPeriod = reportRecord.ESFAPlannedPaymentsThisPeriod ?? 0;
                    reportRecord.PlannedPaymentsToYouToDate = reportRecord.PlannedPaymentsToYouToDate ?? 0;
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate ?? 0;
                    reportRecord.CoInvestmentPaymentsToCollectThisPeriod = reportRecord.CoInvestmentPaymentsToCollectThisPeriod ?? 0;
                    reportRecord.TotalCoInvestmentCollectedToDate = reportRecord.TotalCoInvestmentCollectedToDate ?? 0;

                    // Work out calculated fields
                    // Issues amount - how much the gap is between what the provider earnt and the payments the ESFA/Employer were planning to give them
                    // Following BR2 - if payments are => earnings, issues amount should be zero
                    if ((reportRecord.ESFAPlannedPaymentsThisPeriod + reportRecord.CoInvestmentPaymentsToCollectThisPeriod) >= reportRecord.TotalEarningsForPeriod)
                    {
                        reportRecord.IssuesAmount = 0;
                    }
                    else
                    {
                        reportRecord.IssuesAmount = (decimal.Round((reportRecord.TotalEarningsForPeriod ?? 0)
                                                     - (reportRecord.CoInvestmentPaymentsToCollectThisPeriod ?? 0)
                                                     - (reportRecord.ESFAPlannedPaymentsThisPeriod ?? 0), LearnerLevelViewConstants.LLVRoundingAccuracy)) * -1;
                    }

                    // Work out what is remaining from employer by subtracting what they a have paid so far from their calculated payments.
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate - reportRecord.TotalCoInvestmentCollectedToDate;

                    // Issues for non-payment - worked out in order of priority.
                    // Work out issues (Other) (NOTE: Do this first as lowest priority)
                    // Include rounding in calculation
                    if (reportRecord.IssuesAmount != 0)
                    {
                        reportRecord.ReasonForIssues = LearnerLevelViewConstants.ReasonForIssues_Other;
                    }

                    // Work out issues (HBCP)
                    if ((hbcpInfo != null) &&
                        hbcpInfo.Any(p => p.LearnerReferenceNumber == reportRecord.PaymentLearnerReferenceNumber &&
                                                            p.NonPaymentReason != null &&
                                                            p.CollectionPeriod == returnPeriod))
                    {
                        reportRecord.ReasonForIssues = LearnerLevelViewConstants.ReasonForIssues_CompletionHoldbackPayment;
                    }

                    // Work out reason for issues (Clawback)
                    if (reportRecord.ESFAPlannedPaymentsThisPeriod < 0)
                    {
                        reportRecord.ReasonForIssues = LearnerLevelViewConstants.ReasonForIssues_Clawback;
                    }

                    // NOTE: As per WI93411, a level of rounding is required before the comparisons can be performed
                    if ((reportRecord.IssuesAmount != 0)
                         && (decimal.Round((reportRecord.TotalEarningsToDate ?? 0) 
                                           - (reportRecord.PlannedPaymentsToYouToDate ?? 0)
                                           - (reportRecord.TotalCoInvestmentCollectedToDate ?? 0)
                                           - (reportRecord.CoInvestmentOutstandingFromEmplToDate ?? 0), LearnerLevelViewConstants.LLVRoundingAccuracy) == 0))
                    {
                        reportRecord.ReasonForIssues = LearnerLevelViewConstants.ReasonForIssues_Clawback;
                    }

                    // If the reason for issue is datalock then we need to set the rule description
                    if ((datalocks != null) && (datalocks.Any()))
                    {
                        var datalock = dataLockHashset.FirstOrDefault(x => x.LearnerReferenceNumber == reportRecord.PaymentLearnerReferenceNumber &&
                                                                           x.CollectionPeriod == returnPeriod);

                        // Check to see if any records returned
                        if (datalock != null)
                        {
                            // Extract data lock info
                            int datalockRuleId = datalock.DataLockFailureId;

                            // calculate the rule description
                            string datalockValue = LearnerLevelViewConstants.DLockErrorRuleNamePrefix + datalockRuleId.ToString("00");
                            reportRecord.ReasonForIssues = datalockValue;
                            reportRecord.RuleDescription = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId.CaseInsensitiveEquals(datalockValue))?.ErrorMessage;
                        }
                    }

                    // Default any null calculated records
                    reportRecord.IssuesAmount = reportRecord.IssuesAmount ?? 0;
                    reportRecord.ReasonForIssues = reportRecord.ReasonForIssues ?? string.Empty;
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate ?? 0;

                    return reportRecord;

                }).OrderBy(o => o.PaymentLearnerReferenceNumber)
                    .ThenBy(o => o.PaymentFundingLineType).ToList();

                // Remove the zeroed results - checked documentation and performance of this should be similar to a .Where
                learnerLevelViewModelList.RemoveAll(p => p.TotalEarningsToDate == 0 && p.PlannedPaymentsToYouToDate == 0 && p.TotalCoInvestmentCollectedToDate == 0
                                                                       && p.CoInvestmentOutstandingFromEmplToDate == 0 && p.TotalEarningsForPeriod == 0 && p.ESFAPlannedPaymentsThisPeriod == 0
                                                                       && p.CoInvestmentPaymentsToCollectThisPeriod == 0 && p.IssuesAmount == 0);

                // Set the missing employer names
                foreach (var llvr in learnerLevelViewModelList.Where(w => String.IsNullOrEmpty(w.EmployerName)))
                {
                    if (employerNamesToIDs.TryGetValue(llvr.LearnerEmploymentStatusEmployerId ?? 0, out var employerName))
                    {
                        llvr.EmployerName = employerName;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Build Learner Level View", ex);
                throw ex;
            }

            return learnerLevelViewModelList;
        }

        private IEnumerable<string> UnionKeys(
                        ICollection<Payment> payments,
                        ICollection<LearningDeliveryEarning> ldEarnings,
                        ICollection<PriceEpisodeEarning> peEarnings)
        {
            var filteredPaymentRecordsHashSet = new HashSet<string>(payments.Select(r => r.LearnerReferenceNumber));
            var filteredPriceEpisodeRecordHashset = new HashSet<string>(peEarnings.Select(r => r.LearnRefNumber));
            var filteredLearningDeliveryRecordHashset = new HashSet<string>(ldEarnings.Select(r => r.LearnRefNumber));

            filteredPaymentRecordsHashSet.UnionWith(filteredPriceEpisodeRecordHashset);
            filteredPaymentRecordsHashSet.UnionWith(filteredLearningDeliveryRecordHashset);

            return filteredPaymentRecordsHashSet.ToList();
        }

        private bool TotalESFAPlannedPaymentFSTypePredicate(Payment payment)
        {
            bool result = payment.LearningAimReference.CaseInsensitiveEquals(LearnAimRefConstants.ZPROG001) &&
                   LearnerLevelViewConstants.eSFAFundingSources.Contains(payment.FundingSource) &&
                   LearnerLevelViewConstants.eSFAFSTransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        private bool TotalESFAPlannedPaymentsTTTypePredicate(Payment payment)
        {
            bool result = payment.LearningAimReference.CaseInsensitiveEquals(LearnAimRefConstants.ZPROG001) &&
                   LearnerLevelViewConstants.eSFATransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        private bool TotalESFAPlannedPaymentsNonZPTypePredicate(Payment payment)
        {
            bool result = !payment.LearningAimReference.CaseInsensitiveEquals(LearnAimRefConstants.ZPROG001) &&
                   LearnerLevelViewConstants.eSFANONZPTransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Due From Employer Payment Type Predicates
        //------------------------------------------------------------------------------------------------------

        private bool TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(Payment payment)
        {
            bool result = payment.LearningAimReference.CaseInsensitiveEquals(LearnAimRefConstants.ZPROG001) &&
                   LearnerLevelViewConstants.coInvestmentundingSources.Contains(payment.FundingSource) &&
                   LearnerLevelViewConstants.coInvestmentTransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        private decimal? CalculatePriceEpisodeEarningsToPeriod(
                        List<LearningDeliveryEarning> ldLearnerRecords,
                        List<PriceEpisodeEarning> pelearnerRecords,
                        bool yearToDate,
                        int period,
                        LearnerLevelViewModel learnerLevelViewModel)
        {
            // Exit if there are no records to calc
            if (pelearnerRecords == null)
            {
                return 0;
            }

            decimal? sum = 0;

            // Build a list of records we don't need to calculate for
            var mathEngRecords = ldLearnerRecords.Where(p => p.LearnDelMathEng == true).Select(record => new PriceEpisodeEarning()
            {
                LearnRefNumber = record.LearnRefNumber
            });

            var peRecords = pelearnerRecords?.Where(pe => _peAttributeGroup.Contains(pe.AttributeName)).Except(mathEngRecords, _peEarningComparer);

            foreach (var pe in peRecords)
            {
                sum = sum + PEPeriodSum(pe, period, yearToDate);
            }

            return sum;
        }

        private decimal PEPeriodSum(PriceEpisodeEarning pe, int period, bool yearToDate)
        {
            switch(period)
            {
                case 1: return pe.Period_1??0;
                case 2: return (yearToDate ? pe.Period_1 : 0) + pe.Period_2 ?? 0;
                case 3: return (yearToDate ? pe.Period_1 + pe.Period_2 : 0) + pe.Period_3 ?? 0;
                case 4: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 : 0) + pe.Period_4 ?? 0;
                case 5: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 : 0) + pe.Period_5 ?? 0;
                case 6: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 : 0) + pe.Period_6 ?? 0;
                case 7: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 + pe.Period_6 : 0) + pe.Period_7 ?? 0;
                case 8: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 + pe.Period_6 + pe.Period_7 : 0) + pe.Period_8 ?? 0;
                case 9: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 + pe.Period_6 + pe.Period_7 + pe.Period_8 : 0) + pe.Period_9 ?? 0;
                case 10: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 + pe.Period_6 + pe.Period_7 + pe.Period_8 + pe.Period_9 : 0) + pe.Period_10 ?? 0;
                case 11: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 + pe.Period_6 + pe.Period_7 + pe.Period_8 + pe.Period_9 + pe.Period_10 : 0) + pe.Period_11 ?? 0;
                case 12: return (yearToDate ? pe.Period_1 + pe.Period_2 + pe.Period_3 + pe.Period_4 + pe.Period_5 + pe.Period_6 + pe.Period_7 + pe.Period_8 + pe.Period_9 + pe.Period_10 + pe.Period_11 : 0) + pe.Period_12 ?? 0;

                default: return 0;
            }
        }

        private decimal? CalculateLearningDeliveryEarningsToPeriod(
                    List<LearningDeliveryEarning> ldLearnerRecords,
                    bool yearToDate,
                    int period,
                    LearnerLevelViewModel learnerLevelViewModel)
        {
            // Exit if there are no records to calc
            if (ldLearnerRecords == null)
            {
                return 0;
            }

            decimal? sum = 0;
            var ldRecords = ldLearnerRecords?.Where(pe => _ldAttributeGroup.Contains(pe.AttributeName));

            foreach (var ld in ldRecords)
            {
                // Check to see if this is a request for "LearnSuppFundCash" - if it is check to see if the record has LearnDelMathEng flag set.
                // If it does, we are OK to use this value in the sum (otherwise the corresponding PE value will be used)
                if ((ld.AttributeName == AttributeConstants.Fm36LearnSuppFundCash) && (ld.LearnDelMathEng != true))
                {
                    // Don't calc anything
                }
                else
                {
                    sum = sum + LDPeriodSum(ld, period, yearToDate);
                }
            }

            return sum;
        }

        private decimal LDPeriodSum(LearningDeliveryEarning ld, int period, bool yearToDate)
        {
            switch (period)
            {
                case 1: return ld.Period_1 ?? 0;
                case 2: return (yearToDate ? ld.Period_1 : 0) + ld.Period_2 ?? 0;
                case 3: return (yearToDate ? ld.Period_1 + ld.Period_2 : 0) + ld.Period_3 ?? 0;
                case 4: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 : 0) + ld.Period_4 ?? 0;
                case 5: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 : 0) + ld.Period_5 ?? 0;
                case 6: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 : 0) + ld.Period_6 ?? 0;
                case 7: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 + ld.Period_6 : 0) + ld.Period_7 ?? 0;
                case 8: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 + ld.Period_6 + ld.Period_7 : 0) + ld.Period_8 ?? 0;
                case 9: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 + ld.Period_6 + ld.Period_7 + ld.Period_8 : 0) + ld.Period_9 ?? 0;
                case 10: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 + ld.Period_6 + ld.Period_7 + ld.Period_8 + ld.Period_9 : 0) + ld.Period_10 ?? 0;
                case 11: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 + ld.Period_6 + ld.Period_7 + ld.Period_8 + ld.Period_9 + ld.Period_10 : 0) + ld.Period_11 ?? 0;
                case 12: return (yearToDate ? ld.Period_1 + ld.Period_2 + ld.Period_3 + ld.Period_4 + ld.Period_5 + ld.Period_6 + ld.Period_7 + ld.Period_8 + ld.Period_9 + ld.Period_10 + ld.Period_11 : 0) + ld.Period_12 ?? 0;

                default: return 0;
            }
        }
    }
}
