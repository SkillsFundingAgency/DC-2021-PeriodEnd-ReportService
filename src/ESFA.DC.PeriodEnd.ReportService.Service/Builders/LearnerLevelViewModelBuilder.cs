using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class LearnerLevelViewModelBuilder : ILearnerLevelViewModelBuilder
    {
        private readonly HashSet<byte?> _fundingSourceLevyPayments = new HashSet<byte?>() { 1, 5 };
        private readonly HashSet<byte?> _fundingSourceCoInvestmentPayments = new HashSet<byte?>() { 2 };
        private readonly HashSet<byte?> _fundingSourceCoInvestmentDueFromEmployer = new HashSet<byte?>() { 3 };

        private readonly HashSet<byte?> _transactionTypesLevyPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesCoInvestmentPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesCoInvestmentDueFromEmployer = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesEmployerAdditionalPayments = new HashSet<byte?>() { 4, 6 };
        private readonly HashSet<byte?> _transactionTypesProviderAdditionalPayments = new HashSet<byte?>() { 5, 7 };
        private readonly HashSet<byte?> _transactionTypesApprenticeshipAdditionalPayments = new HashSet<byte?>() { 16 };
        private readonly HashSet<byte?> _transactionTypesEnglishAndMathsPayments = new HashSet<byte?>() { 13, 14 };
        private readonly HashSet<byte?> _transactionTypesLearningSupportPayments = new HashSet<byte?>() { 8, 9, 10, 11, 12, 15 };

        private readonly ILogger _logger;
        private readonly ILLVPaymentRecordKeyEqualityComparer _lLVPaymentRecordKeyEqualityComparer;
        private readonly ILLVPaymentRecordLRefOnlyKeyEqualityComparer _lLVPaymentRecordLRefOnlyKeyEqualityComparer;
        private int _appsReturnPeriod;

        public LearnerLevelViewModelBuilder(
            ILogger logger,
            ILLVPaymentRecordKeyEqualityComparer lLVPaymentRecordKeyEqualityComparer,
            ILLVPaymentRecordLRefOnlyKeyEqualityComparer lLVPaymentRecordLRefOnlyKeyEqualityComparer)
        {
            _logger = logger;
            _lLVPaymentRecordKeyEqualityComparer = lLVPaymentRecordKeyEqualityComparer;
            _lLVPaymentRecordLRefOnlyKeyEqualityComparer = lLVPaymentRecordLRefOnlyKeyEqualityComparer;
        }

        public IReadOnlyList<LearnerLevelViewModel> BuildLearnerLevelViewModelList(
            int ukprn,
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            LearnerLevelViewDASDataLockInfo learnerLevelViewDASDataLockInfo,
            LearnerLevelViewHBCPInfo learnerLevelHBCPInfo,
            LearnerLevelViewFM36Info learnerLevelDAsInfo,
            IDictionary<LearnerLevelViewPaymentsKey, List<AppsMonthlyPaymentDasPaymentModel>> paymentsDictionary,
            IDictionary<string, List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> aECPriceEpisodeDictionary,
            IDictionary<string, List<AECLearningDeliveryPeriodisedValuesInfo>> aECLearningDeliveryDictionary,
            IDictionary<long, string> employerNameDictionary,
            int returnPeriod)
        {
            // cache the passed in data for use in the private 'Get' methods
            _appsReturnPeriod = returnPeriod;

            // this variable is the final report and is the return value of this method.
            List<LearnerLevelViewModel> learnerLevelViewModelList = null;

            try
            {
                // Union the keys from the datasets being used to source the report
                var unionedKeys = UnionKeys(paymentsDictionary.Keys, aECPriceEpisodeDictionary.Keys, aECLearningDeliveryDictionary.Keys);

                // Populate the learner list using the ILR query first
                learnerLevelViewModelList = unionedKeys
                .OrderBy(o => o.LearnerReferenceNumber)
                    .ThenBy(o => o.PaymentFundingLineType)
                .Select(record =>
                {
                    var reportRecord = new LearnerLevelViewModel()
                    {
                        Ukprn = ukprn,
                        PaymentLearnerReferenceNumber = record.LearnerReferenceNumber,
                        PaymentFundingLineType = record.PaymentFundingLineType
                    };

                    if (paymentsDictionary.TryGetValue(new LearnerLevelViewPaymentsKey(reportRecord.PaymentLearnerReferenceNumber, reportRecord.PaymentFundingLineType), out List<AppsMonthlyPaymentDasPaymentModel> paymentValues))
                    {
                        // Assign the amounts
                        reportRecord.PlannedPaymentsToYouToDate = paymentValues.Where(p => PeriodESFAPlannedPaymentsFSTypePredicateToPeriod(p, _appsReturnPeriod) ||
                                                    PeriodESFAPlannedPaymentsTTTypePredicateToPeriod(p, _appsReturnPeriod) ||
                                                    PeriodESFAPlannedPaymentsNonZPTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        reportRecord.ESFAPlannedPaymentsThisPeriod = paymentValues.Where(p => PeriodESFAPlannedPaymentsFSTypePredicate(p, _appsReturnPeriod) ||
                                                    PeriodESFAPlannedPaymentsTTTypePredicate(p, _appsReturnPeriod) ||
                                                    PeriodESFAPlannedPaymentsNonZPTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        reportRecord.CoInvestmentPaymentsToCollectThisPeriod = paymentValues.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        // NOTE: Additional earigns calc required for this field
                        reportRecord.CoInvestmentOutstandingFromEmplToDate = paymentValues.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        // Pull across the ULN if it's not already there (we can pick the first record as the set is matched on learner ref so all ULNs in it will be the same)
                        reportRecord.PaymentUniqueLearnerNumber = paymentValues.FirstOrDefault()?.LearnerUln;

                        // Extract company name
                        string employerName;
                        if ((employerNameDictionary != null) && employerNameDictionary.TryGetValue(paymentValues.FirstOrDefault().ApprenticeshipId ?? 0, out employerName))
                        {
                            reportRecord.EmployerName = employerName;
                        }
                        else
                        {
                            reportRecord.EmployerName = string.Empty;
                        }
                    }

                    // Extract ILR info
                    var ilrRecord = appsMonthlyPaymentIlrInfo.Learners.FirstOrDefault(p => p.Ukprn == ukprn && p.LearnRefNumber == reportRecord.PaymentLearnerReferenceNumber);
                    if (ilrRecord != null)
                    {
                        reportRecord.FamilyName = ilrRecord.FamilyName;
                        reportRecord.GivenNames = ilrRecord.GivenNames;
                        reportRecord.PaymentUniqueLearnerNumber = ilrRecord.UniqueLearnerNumber;
                        if ((ilrRecord.LearnerEmploymentStatus != null) && (ilrRecord.LearnerEmploymentStatus.Count > 0))
                        {
                            reportRecord.LearnerEmploymentStatusEmployerId = ilrRecord.LearnerEmploymentStatus.Where(les => les?.Ukprn == ukprn &&
                                                                                                                        les.LearnRefNumber.CaseInsensitiveEquals(reportRecord.PaymentLearnerReferenceNumber) &&
                                                                                                                        les?.EmpStat == 10)
                                                                                                                .OrderByDescending(les => les?.DateEmpStatApp)
                                                                                                                .FirstOrDefault()?.EmpdId;
                        }
                    }

                    if (appsCoInvestmentIlrInfo != null)
                    {
                        var ilrInfo = appsCoInvestmentIlrInfo.Learners?
                                        .FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(reportRecord.PaymentLearnerReferenceNumber));

                        if (ilrInfo != null)
                        {
                            var learningDeliveries = ilrInfo.LearningDeliveries.Where(p => p.LearnRefNumber == reportRecord.PaymentLearnerReferenceNumber && p.UKPRN == ukprn);
                            if ((learningDeliveries != null) && (learningDeliveries.Count() > 0))
                            {
                                foreach (var learningDelivery in learningDeliveries)
                                {
                                    IReadOnlyCollection<AppFinRecordInfo> currentYearData = learningDelivery.AppFinRecords.Where(x => x.AFinDate >= Generics.BeginningOfYear && x.AFinDate <= Generics.EndOfYear && x.AFinType.CaseInsensitiveEquals("PMR")).ToList();
                                    reportRecord.TotalCoInvestmentCollectedToDate =
                                        currentYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                                        currentYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);
                                }
                            }
                        }
                    }

                    // Work out total earnings
                    aECLearningDeliveryDictionary.TryGetValue(reportRecord.PaymentLearnerReferenceNumber, out List<AECLearningDeliveryPeriodisedValuesInfo> ldLearner);
                    aECPriceEpisodeDictionary.TryGetValue(reportRecord.PaymentLearnerReferenceNumber, out List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> peLearner);

                    reportRecord.TotalEarningsToDate = CalculatePriceEpisodeEarningsToPeriod(ldLearner, peLearner, true, _appsReturnPeriod, reportRecord) +
                                                       CalculateLearningDeliveryEarningsToPeriod(ldLearner, true, _appsReturnPeriod, reportRecord);

                    reportRecord.TotalEarningsForPeriod = CalculatePriceEpisodeEarningsToPeriod(ldLearner, peLearner, false, _appsReturnPeriod, reportRecord) +
                                                          CalculateLearningDeliveryEarningsToPeriod(ldLearner, false, _appsReturnPeriod, reportRecord);

                    // Get any missing funding line types from earnings
                    if (string.IsNullOrEmpty(reportRecord.PaymentFundingLineType) && learnerLevelDAsInfo != null && learnerLevelDAsInfo.AECPriceEpisodeFLTsInfo != null)
                    {
                        reportRecord.PaymentFundingLineType = learnerLevelDAsInfo.AECPriceEpisodeFLTsInfo.FirstOrDefault(p => p.LearnerReferenceNumber == reportRecord.PaymentLearnerReferenceNumber)?.PaymentFundingLineType;
                    }

                    // Default any null valued records
                    reportRecord.ESFAPlannedPaymentsThisPeriod = reportRecord.ESFAPlannedPaymentsThisPeriod == null ? 0 : reportRecord.ESFAPlannedPaymentsThisPeriod;
                    reportRecord.PlannedPaymentsToYouToDate = reportRecord.PlannedPaymentsToYouToDate == null ? 0 : reportRecord.PlannedPaymentsToYouToDate;
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate == null ? 0 : reportRecord.CoInvestmentOutstandingFromEmplToDate;
                    reportRecord.CoInvestmentPaymentsToCollectThisPeriod = reportRecord.CoInvestmentPaymentsToCollectThisPeriod == null ? 0 : reportRecord.CoInvestmentPaymentsToCollectThisPeriod;
                    reportRecord.TotalCoInvestmentCollectedToDate = reportRecord.TotalCoInvestmentCollectedToDate == null ? 0 : reportRecord.TotalCoInvestmentCollectedToDate;

                    // Work out calculated fields
                    // Issues amount - how much the gap is between what the provider earnt and the payments the ESFA/Employer were planning to give them
                    // Following BR2 - if payments are => earnings, issues amount should be zero
                    if ((reportRecord.ESFAPlannedPaymentsThisPeriod + reportRecord.CoInvestmentPaymentsToCollectThisPeriod) >= reportRecord.TotalEarningsForPeriod)
                    {
                        reportRecord.IssuesAmount = 0;
                    }
                    else
                    {
                        reportRecord.IssuesAmount = (reportRecord.TotalEarningsForPeriod
                                                    - reportRecord.ESFAPlannedPaymentsThisPeriod
                                                    - reportRecord.CoInvestmentPaymentsToCollectThisPeriod) * -1;
                    }

                    // Work out what is remaining from employer by subtracting what they a have paid so far from their calculated payments.
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate - reportRecord.TotalCoInvestmentCollectedToDate;

                    // Issues for non-payment - worked out in order of priority.
                    // Work out issues (Other) (NOTE: Do this first as lowest priority)
                    if (reportRecord.TotalEarningsForPeriod > reportRecord.CoInvestmentPaymentsToCollectThisPeriod +
                                                              reportRecord.ESFAPlannedPaymentsThisPeriod)
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Other;
                    }

                    // Work out issues (HBCP)
                    if ((learnerLevelHBCPInfo != null) &&
                        learnerLevelHBCPInfo.HBCPModels.Any(p => p.UkPrn == ukprn &&
                                                            p.LearnerReferenceNumber == reportRecord.PaymentLearnerReferenceNumber &&
                                                            p.NonPaymentReason == 0 &&
                                                            p.DeliveryPeriod == _appsReturnPeriod))
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_CompletionHoldbackPayment;
                    }

                    // Work out reason for issues (Clawback)
                    if (reportRecord.ESFAPlannedPaymentsThisPeriod < 0)
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Clawback;
                    }

                    // NOTE: As per WI93411, a level of rounding is required before the comparisions can be performed
                    if ((reportRecord.TotalEarningsForPeriod > reportRecord.ESFAPlannedPaymentsThisPeriod +
                                                               reportRecord.CoInvestmentPaymentsToCollectThisPeriod)
                        && (decimal.Round(reportRecord.TotalEarningsToDate ?? 0, 2) == decimal.Round((reportRecord.PlannedPaymentsToYouToDate ?? 0) +
                                                                (reportRecord.TotalCoInvestmentCollectedToDate ?? 0) +
                                                                (reportRecord.CoInvestmentOutstandingFromEmplToDate ?? 0), 2)))
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Clawback;
                    }

                    // If the reason for issue is datalock then we need to set the rule description
                    if ((learnerLevelViewDASDataLockInfo != null) && (learnerLevelViewDASDataLockInfo.DASDataLocks != null))
                    {
                        var datalock = learnerLevelViewDASDataLockInfo.DASDataLocks
                                                .FirstOrDefault(x => x.UkPrn == reportRecord.Ukprn &&
                                                        x.LearnerReferenceNumber == reportRecord.PaymentLearnerReferenceNumber &&
                                                        x.DeliveryPeriod == _appsReturnPeriod);

                        // Check to see if any records returned
                        if (datalock != null)
                        {
                            // Extract data lock info
                            int datalock_rule_id = datalock.DataLockFailureId;

                            // calculate the rule description
                            string datalockValue = Generics.DLockErrorRuleNamePrefix + datalock_rule_id.ToString("00");
                            reportRecord.ReasonForIssues = datalockValue;
                            reportRecord.RuleDescription = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId.CaseInsensitiveEquals(datalockValue))?.ErrorMessage;
                        }
                    }

                    // Default any null calculated records
                    reportRecord.IssuesAmount = reportRecord.IssuesAmount == null ? 0 : reportRecord.IssuesAmount;
                    reportRecord.ReasonForIssues = reportRecord.ReasonForIssues == null ? string.Empty : reportRecord.ReasonForIssues;
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate == null ? 0 : reportRecord.CoInvestmentOutstandingFromEmplToDate;

                    return reportRecord;
                }).ToList();

                // Remove the zeroed results
                learnerLevelViewModelList.RemoveAll(p => p.TotalEarningsToDate == 0 && p.PlannedPaymentsToYouToDate == 0 && p.TotalCoInvestmentCollectedToDate == 0
                                                                       && p.CoInvestmentOutstandingFromEmplToDate == 0 && p.TotalEarningsForPeriod == 0 && p.ESFAPlannedPaymentsThisPeriod == 0
                                                                       && p.CoInvestmentPaymentsToCollectThisPeriod == 0 && p.IssuesAmount == 0);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Build Learner Level View", ex);
                throw ex;
            }

            return learnerLevelViewModelList;
        }

        public IEnumerable<LearnerLevelViewPaymentsKey> UnionKeys(IEnumerable<LearnerLevelViewPaymentsKey> paymentRecords, IEnumerable<string> priceEpisodeRecords, IEnumerable<string> learningDeliveryRecords)
        {
            var filteredPaymentRecordsHashSet = new HashSet<LearnerLevelViewPaymentsKey>(paymentRecords.Select(r => new LearnerLevelViewPaymentsKey(r.LearnerReferenceNumber, r.PaymentFundingLineType)), _lLVPaymentRecordKeyEqualityComparer);
            var filteredPriceEpisodeRecordHashset = new HashSet<LearnerLevelViewPaymentsKey>(priceEpisodeRecords.Select(r => new LearnerLevelViewPaymentsKey(r.ToString(), string.Empty)), _lLVPaymentRecordLRefOnlyKeyEqualityComparer);
            var filteredLearningDeliveryRecordHashset = new HashSet<LearnerLevelViewPaymentsKey>(learningDeliveryRecords.Select(r => new LearnerLevelViewPaymentsKey(r.ToString(), string.Empty)), _lLVPaymentRecordLRefOnlyKeyEqualityComparer);

            filteredPaymentRecordsHashSet.UnionWith(filteredPriceEpisodeRecordHashset);
            filteredPaymentRecordsHashSet.UnionWith(filteredLearningDeliveryRecordHashset);

            return filteredPaymentRecordsHashSet.ToList();
        }

        /// <summary>
        /// Returns true if the payment is an ESFA planned payment for a range of periods.
        /// For funding source 1, 2, 5 and transaction type 1, 2, 3.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodESFAPlannedPaymentsFSTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalESFAPlannedPaymentFSTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is an ESFA planned payment for a range of periods.
        /// For funding source 1, 2, 5 and transaction type 1, 2, 3.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodESFAPlannedPaymentsFSTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalESFAPlannedPaymentFSTypePredicate(payment);

            return result;
        }

        private bool TotalESFAPlannedPaymentFSTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   Generics.eSFAFundingSources.Contains(payment.FundingSource) &&
                   Generics.eSFAFSTransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is an ESFA planned payment for a range of periods.
        /// For transaction type 4 -> 16. (With the exception of maths and english types - 13 and 14)
        /// /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodESFAPlannedPaymentsTTTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalESFAPlannedPaymentsTTTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is an ESFA planned payment for a range of periods.
        /// For transaction type 4 -> 16. (With the exception of maths and english types - 13 and 14)
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodESFAPlannedPaymentsTTTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalESFAPlannedPaymentsTTTypePredicate(payment);

            return result;
        }

        private bool TotalESFAPlannedPaymentsTTTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   Generics.eSFATransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is an ESFA planned payment for a range of periods.
        /// For transaction type 4 -> 16.
        /// /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodESFAPlannedPaymentsNonZPTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalESFAPlannedPaymentsNonZPTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is an ESFA planned payment for a range of periods.
        /// For transaction type 4 -> 16.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodESFAPlannedPaymentsNonZPTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalESFAPlannedPaymentsNonZPTypePredicate(payment);

            return result;
        }

        private bool TotalESFAPlannedPaymentsNonZPTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                   !payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   Generics.eSFANONZPTransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a CoInvestment payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment payment, otherwise false.</returns>
        private bool PeriodCoInvestmentPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalCoInvestmentPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalCoInvestmentPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _fundingSourceCoInvestmentPayments.Contains(payment.FundingSource) &&
                          _transactionTypesCoInvestmentPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Due From Employer Payment Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payments is a CoInvestment payment due from the Employer.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment due from the employer payment, otherwise false.</returns>
        private bool PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payments is a CoInvestment payment due from the Employer for a range of periods.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment due from the employer payment, otherwise false.</returns>
        private bool PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(payment);

            return result;
        }

        private bool TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   Generics.coInvestmentundingSources.Contains(payment.FundingSource) &&
                   Generics.coInvestmentTransactionTypes.Contains(payment.TransactionType);

            return result;
        }

        private decimal? CalculatePriceEpisodeEarningsToPeriod(
                        List<AECLearningDeliveryPeriodisedValuesInfo> ldLearnerRecords,
                        List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> pelearnerRecords,
                        bool yearToDate,
                        int period,
                        LearnerLevelViewModel learnerLevelViewModel)
        {
            // Exit if there are no records to calc
            if (pelearnerRecords == null)
            {
                return 0;
            }

            var peAttributeGroup = new List<string>()
            {
                Generics.Fm36PriceEpisodeCompletionPaymentAttributeName,
                Generics.Fm36PriceEpisodeOnProgPaymentAttributeName,
                Generics.Fm3PriceEpisodeBalancePaymentAttributeName,
                Generics.Fm36PriceEpisodeLSFCashAttributeName
            };

            decimal? sum = 0;

            // Build a list of records we don't need to calculate for
            var mathEngRecords = ldLearnerRecords.Where(p => p.LearnDelMathEng == true).Select(record => new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
            {
                LearnRefNumber = record.LearnRefNumber,
                AimSeqNumber = record.AimSeqNumber
            });

            var peRecords = pelearnerRecords?.Where(pe => peAttributeGroup.Contains(pe.AttributeName)).Except(mathEngRecords, new AECApprenticeshipPriceEpisodePeriodisedValuesInfoComparer());

            foreach (var pe in peRecords)
            {
                // Add the PE value to the sum
                // NOTE: "i" is an index value created in the linq query
                if ((yearToDate == true) && (pe.Periods != null))
                {
                    sum = sum + pe.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i <= period - 1).Sum(o => o.pv);
                }
                else
                {
                    sum = sum + pe.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i == period - 1).Sum(o => o.pv);
                }
            }

            return sum;
        }

        private decimal? CalculateLearningDeliveryEarningsToPeriod(
                    List<AECLearningDeliveryPeriodisedValuesInfo> ldLearnerRecords,
                    bool yearToDate,
                    int period,
                    LearnerLevelViewModel learnerLevelViewModel)
        {
            // Exit if there are no records to calc
            if (ldLearnerRecords == null)
            {
                return 0;
            }

            var ldAttributeGroup = new List<string>()
            {
                Generics.Fm36MathEngOnProgPaymentAttributeName,
                Generics.Fm36LearnSuppFundCashAttributeName,
                Generics.Fm36MathEngBalPayment,
                Generics.Fm36DisadvFirstPayment,
                Generics.Fm36DisadvSecondPayment,
                Generics.Fm36LearnDelFirstEmp1618Pay,
                Generics.Fm36LearnDelSecondEmp1618Pay,
                Generics.Fm36LearnDelFirstProv1618Pay,
                Generics.Fm36LearnDelSecondProv1618Pay,
                Generics.Fm36LearnDelLearnAddPayment,
                Generics.Fm36LDApplic1618FrameworkUpliftBalancingPayment,
                Generics.Fm36LDApplic1618FrameworkUpliftCompletionPayment,
                Generics.Fm36LDApplic1618FrameworkUpliftOnProgPayment
            };

            decimal? sum = 0;
            var ldRecords = ldLearnerRecords?.Where(pe => ldAttributeGroup.Contains(pe.AttributeName));

            foreach (var ld in ldRecords)
            {
                // Check to see if this is a request for "LearnSuppFundCash" - if it is check to see if the record has LearnDelMathEng flag set.
                // If it does, we are OK to use this value in the sum (otherwise the corresponding PE value will be used)
                if ((ld.AttributeName == Generics.Fm36LearnSuppFundCashAttributeName) && (ld.LearnDelMathEng != true))
                {
                    // Don't calc anything
                }
                else
                {
                    // Make sure this is the right attrib to be summed
                    // NOTE: "i" is an index value created in the linq query
                    if ((yearToDate == true) && (ld.Periods != null))
                    {
                        sum = sum + ld.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i <= period - 1).Sum(o => o.pv);
                    }
                    else
                    {
                        sum = sum + ld.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i == period - 1).Sum(o => o.pv);
                    }
                }
            }

            return sum;
        }
    }
}
