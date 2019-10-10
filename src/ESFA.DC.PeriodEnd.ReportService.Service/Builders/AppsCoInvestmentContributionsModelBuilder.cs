using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsCoInvestmentContributionsModelBuilder : IAppsCoInvestmentContributionsModelBuilder
    {
        private const int _fundingSource = 3;
        private readonly HashSet<int> _transactionTypes = new HashSet<int>()
        {
            Constants.DASPayments.TransactionType.Learning_On_Programme,
            Constants.DASPayments.TransactionType.Completion,
            Constants.DASPayments.TransactionType.Balancing,
        };

        private readonly string _priceEpisodeCompletionPayment = "PriceEpisodeCompletionPayment";
        private readonly ILogger _logger;

        public AppsCoInvestmentContributionsModelBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo,
            List<AppsCoInvestmentRecordKey> paymentsAppsCoInvestmentUniqueKeys,
            List<AppsCoInvestmentRecordKey> ilrAppsCoInvestmentUniqueKeys,
            long jobId)
        {
            string errorMessage = string.Empty;

            if (appsCoInvestmentIlrInfo == null || appsCoInvestmentIlrInfo.Learners == null)
            {
                errorMessage = "Error: BuildModel() - AppsCoInvestmentILRInfo is null, no data has been retrieved from the ILR1920 data store.";
                _logger.LogInfo(errorMessage, jobIdOverride: jobId);

                throw new Exception(errorMessage);
            }

            if (appsCoInvestmentRulebaseInfo == null)
            {
                errorMessage = "Error: BuildModel() - AppsCoInvestmentRulebaseInfo is null, no data has been retrieved from the ILR1920 data store.";
                _logger.LogInfo(errorMessage, jobIdOverride: jobId);

                throw new Exception(errorMessage);
            }

            if (appsCoInvestmentPaymentsInfo == null)
            {
                errorMessage = "Error: BuildModel() - appsCoInvestmentPaymentsInfo is null, no data has been retrieved from the Payments data store.";
                _logger.LogInfo(errorMessage, jobIdOverride: jobId);

                throw new Exception(errorMessage);
            }

            List<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels = new List<AppsCoInvestmentContributionsModel>();

            var learnRefNumbers = GetRelevantLearners(appsCoInvestmentIlrInfo, appsCoInvestmentPaymentsInfo);

            var uniqueKeys = paymentsAppsCoInvestmentUniqueKeys.Union(ilrAppsCoInvestmentUniqueKeys);

            var filteredRecordKeys = FilterRelevantLearnersFromPaymentsRecordKeys(learnRefNumbers, uniqueKeys);

            var filterReportRows = filteredRecordKeys.Where(r => FilterReportRows(appsCoInvestmentPaymentsInfo, appsCoInvestmentRulebaseInfo, appsCoInvestmentIlrInfo, r));

            return filteredRecordKeys
                .Select(record =>
                {
                    var paymentRecords = GetPaymentInfosForRecord(appsCoInvestmentPaymentsInfo, record).ToList();
                    var learner = GetLearnerForRecord(appsCoInvestmentIlrInfo, record);
                    var learningDeliveries = GetLearningDeliveriesForRecord(learner, record).ToList();
                    var filteredPaymentRecords = FundingSourceAndTransactionTypeFilter(paymentRecords).ToList();

                    var model = new AppsCoInvestmentContributionsModel();

                    model.LearnRefNumber = record.LearnerReferenceNumber;
                    model.UniqueLearnerNumber = GetUniqueOrEmpty(paymentRecords, p => p.LearnerUln);
                    model.LearningStartDate = record.LearningStartDate;
                    model.ProgType = record.LearningAimProgrammeType;
                    model.StandardCode = record.LearningAimStandardCode;
                    model.FrameworkCode = record.LearningAimFrameworkCode;
                    model.ApprenticeshipPathway = record.LearningAimPathwayCode;
                    model.SoftwareSupplierAimIdentifier = learningDeliveries.FirstOrDefault(ld => ld.LearnAimRef.CaseInsensitiveEquals("ZPROG001"))?.SWSupAimId;
                    model.LearningDeliveryFAMTypeApprenticeshipContractType = GetUniqueOrEmpty(paymentRecords, p => p.ContractType);
                    model.EmployerIdentifierAtStartOfLearning = learner?.LearnerEmploymentStatus.Where(w => w.DateEmpStatApp <= record.LearningStartDate).OrderByDescending(o => o.DateEmpStatApp).FirstOrDefault()?.EmpId;
                    model.EmployerNameFromApprenticeshipService = GetEarliestPaymentInfo(paymentRecords)?.LegalEntityName;
                    model.EmployerCoInvestmentPercentage = (1 - filteredPaymentRecords
                                                                .GroupBy(g => new { g.DeliveryPeriod, g.AcademicYear })
                                                                .Select(s => new { AggAmount = s.Sum(a => a.Amount), SFaContrib = s.Min(m => m.SfaContributionPercentage) })
                                                                .Where(w => w.AggAmount != 0)
                                                                .Select(s => s.SFaContrib)
                                                                .DefaultIfEmpty()
                                                                .Min()) * 100;
                    //model.ApplicableProgrammeStartDate

                    return model;
                });
        }

        public IEnumerable<PaymentInfo> FundingSourceAndTransactionTypeFilter(IEnumerable<PaymentInfo> paymentInfos)
        {
            return paymentInfos.Where(p => p.FundingSource == _fundingSource && _transactionTypes.Contains(p.TransactionType));
        }

        public PaymentInfo GetEarliestPaymentInfo(IEnumerable<PaymentInfo> paymentInfos)
        {
            return paymentInfos?
                .OrderBy(p => p.AcademicYear)
                .ThenBy(p => p.DeliveryPeriod)
                .ThenBy(p => p.CollectionPeriod)
                .FirstOrDefault();
        }

        public IEnumerable<PaymentInfo> GetPaymentInfosForRecord(AppsCoInvestmentPaymentsInfo paymentsInfo, AppsCoInvestmentRecordKey record)
        {
            return paymentsInfo
                .Payments?
                .Where(p =>
                    p.LearnerReferenceNumber.CaseInsensitiveEquals(record.LearnerReferenceNumber)
                    && p.LearningStartDate == record.LearningStartDate
                    && p.LearningAimProgrammeType == record.LearningAimProgrammeType
                    && p.LearningAimStandardCode == record.LearningAimStandardCode
                    && p.LearningAimFrameworkCode == record.LearningAimFrameworkCode
                    && p.LearningAimPathwayCode == record.LearningAimPathwayCode
                    && p.LearningAimReference.CaseInsensitiveEquals(record.LearningAimReference))
                ?? Enumerable.Empty<PaymentInfo>();
        }

        public IEnumerable<LearningDeliveryInfo> GetLearningDeliveriesForRecord(LearnerInfo learner, AppsCoInvestmentRecordKey record)
        {
            return learner?
                .LearningDeliveries
                .Where(ld => IlrLearningDeliveryRecordMatch(ld, record))
                ?? Enumerable.Empty<LearningDeliveryInfo>();
        }

        public LearnerInfo GetLearnerForRecord(AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentRecordKey record)
        {
            return ilrInfo
                .Learners?
                .FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(record.LearnerReferenceNumber)
                    && (l.LearningDeliveries?.Any(ld => IlrLearningDeliveryRecordMatch(ld, record)) ?? false));
        }

        public bool IlrLearningDeliveryRecordMatch(LearningDeliveryInfo learningDelivery, AppsCoInvestmentRecordKey record)
        {
            return learningDelivery.LearnStartDate == record.LearningStartDate
                    && learningDelivery.ProgType == record.LearningAimProgrammeType
                    && learningDelivery.StdCode == record.LearningAimStandardCode
                    && learningDelivery.FworkCode == record.LearningAimFrameworkCode
                    && learningDelivery.PwayCode == record.LearningAimPathwayCode
                    && learningDelivery.LearnAimRef.CaseInsensitiveEquals(record.LearningAimReference);
        }

        //public IEnumerable<AppFinRecordInfo> GetAppFinForRecord(AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentRecordKey record)
        //{
        //    return ilrInfo
        //        .Learners
        //        .FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(record.LearnerReferenceNumber))?
        //        .LearningDeliveries
        //        .Where(ld =>
        //            ld.LearnStartDate == record.LearningStartDate
        //            && ld.ProgType == record.LearningAimProgrammeType
        //            && ld.StdCode == record.LearningAimStandardCode
        //            && ld.FworkCode == record.LearningAimFrameworkCode
        //            && ld.PwayCode == record.LearningAimPathwayCode).Select(fin)
        //        //?? Enumerable.Empty<AppFinRecordInfo>();
        //}

        public T? GetUniqueOrEmpty<TIn, T>(IEnumerable<TIn> input, Func<TIn, T> selector)
            where T : struct
        {
            var distinct = input.Select(selector).Distinct().ToList();

            if (distinct.Count > 1 || distinct.Count == 0)
            {
                return null;
            }

            return distinct.FirstOrDefault();
        }

        public decimal CalculateCoInvestmentDueForPeriod(bool flag, IEnumerable<PaymentInfo> paymentInfoList, int collectionPeriod)
        {
            return flag
                ? paymentInfoList.Where(x => x.CollectionPeriod == collectionPeriod).Sum(x => x.Amount)
                : 0;
        }

        // BR1
        public IEnumerable<string> GetRelevantLearners(AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentPaymentsInfo paymentsInfo)
        {
            var ilrLearnRefNumbers = ilrInfo
                .Learners?
                .Where(l =>
                    l.LearningDeliveries?
                        .Any(ld => ld.FundModel == 36
                        && (ld.AppFinRecords?.Any(afr => afr.AFinType == "PMR")
                        ?? false))
                    ?? false)
                .Select(l => l.LearnRefNumber).ToList()
                ?? Enumerable.Empty<string>();

            var paymentLearnRefNumbers = paymentsInfo
                .Payments
                .Where(p => p.FundingSource == _fundingSource)
                .Select(p => p.LearnerReferenceNumber).ToList();

            return ilrLearnRefNumbers.Union(paymentLearnRefNumbers);
        }

        public IEnumerable<AppsCoInvestmentRecordKey> FilterRelevantLearnersFromPaymentsRecordKeys(IEnumerable<string> learnRefNumbers, IEnumerable<AppsCoInvestmentRecordKey> recordKeys)
        {
            var learnRefNumbersHashSet = new HashSet<string>(learnRefNumbers);

            return recordKeys.Where(r => learnRefNumbersHashSet.Contains(r.LearnerReferenceNumber));
        }

        // BR2
        public bool FilterReportRows(AppsCoInvestmentPaymentsInfo paymentInfo, AppsCoInvestmentRulebaseInfo rulebaseInfo, AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentRecordKey recordKey)
        {
            return
                EmployerCoInvestmentPaymentFilter(paymentInfo, recordKey.LearnerReferenceNumber)
                || CompletionPaymentFilter(paymentInfo, recordKey.LearnerReferenceNumber)
                || PMRAppFinRecordFilter(ilrInfo, recordKey.LearnerReferenceNumber)
                || NonZeroCompletionEarningsFilter(rulebaseInfo, recordKey.LearnerReferenceNumber);
        }

        public bool CompletionPaymentFilter(AppsCoInvestmentPaymentsInfo paymentsInfo, string learnRefNumber)
        {
            return paymentsInfo.Payments?.Any(p => p.TransactionType == 3 && p.LearnerReferenceNumber.CaseInsensitiveEquals(learnRefNumber)) ?? false;
        }

        public bool EmployerCoInvestmentPaymentFilter(AppsCoInvestmentPaymentsInfo paymentsInfo, string learnRefNumber)
        {
            return paymentsInfo.Payments?.Any(p => p.FundingSource == 3 && p.LearnerReferenceNumber.CaseInsensitiveEquals(learnRefNumber)) ?? false;
        }

        public bool NonZeroCompletionEarningsFilter(AppsCoInvestmentRulebaseInfo rulebaseInfo, string learnRefNumber)
        {
            return rulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues?
                .Any(
                    p =>
                        p.AttributeName == "PriceEpisodeCompletionPayment"
                        && (p.Periods?.Any(v => v.HasValue && v != 0) ?? false))
                ?? false;
        }

        public bool PMRAppFinRecordFilter(AppsCoInvestmentILRInfo ilrInfo, string learnRefNumber)
        {
            return ilrInfo
                .Learners?.Any(
                l =>
                    l.LearnRefNumber.CaseInsensitiveEquals(learnRefNumber)
                    && (l.LearningDeliveries?.Any(ld =>
                        ld.AppFinRecords?.Any(afr => afr.AFinType.CaseInsensitiveEquals(Generics.PMR))
                        ?? false)
                    ?? false))
                ?? false;
        }
    }
}