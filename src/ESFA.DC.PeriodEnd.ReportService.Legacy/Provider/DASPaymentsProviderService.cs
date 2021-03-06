﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Constants;
using ESFA.DC.PeriodEnd.ReportService.Legacy.Provider.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Provider
{
    public class DASPaymentsProviderService : AbstractFundModelProviderService, IDASPaymentsProviderService
    {
        private const int AppsCoInvestmentFundingType = 3;

        private readonly int[] _appsAdditionalPaymentsTransactionTypes = {
            Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive,
            Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive,
            Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive,
            Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive,
            Constants.DASPayments.TransactionType.Apprenticeship };

        private readonly int[] _appsCoInvestmentTransactionTypes =
        {
            Constants.DASPayments.TransactionType.Learning_On_Programme,
            Constants.DASPayments.TransactionType.Completion,
            Constants.DASPayments.TransactionType.Balancing,
        };

        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(
            ILogger logger,
            Func<IDASPaymentsContext> dasPaymentsContextFactory)
            : base(logger)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
        }

        public async Task<List<DASPaymentInfo>> GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _dasPaymentsContextFactory())
            {
                return await context
                    .Payments
                    .Where(p =>
                        p.Ukprn == ukprn
                        && p.AcademicYear == Generics.AcademicYear
                        && p.FundingSource == Constants.DASPayments.FundingSource.Fully_Funded_SFA
                        && _appsAdditionalPaymentsTransactionTypes.Contains(p.TransactionType))
                    .Select(payment => new DASPaymentInfo()
                        {
                            Ukprn = payment.Ukprn,
                            FundingSource = payment.FundingSource,
                            TransactionType = payment.TransactionType,
                            AcademicYear = payment.AcademicYear,
                            CollectionPeriod = payment.CollectionPeriod,
                            ContractType = payment.ContractType,
                            //DeliveryPeriod = payment.DeliveryPeriod,
                            LearnerReferenceNumber = payment.LearnerReferenceNumber,
                            LearnerUln = payment.LearnerUln,
                            LearningAimFrameworkCode = payment.LearningAimFrameworkCode,
                            LearningAimPathwayCode = payment.LearningAimPathwayCode,
                            LearningAimProgrammeType = payment.LearningAimProgrammeType,
                            LearningAimReference = payment.LearningAimReference,
                            LearningAimStandardCode = payment.LearningAimStandardCode,
                            LearningStartDate = payment.LearningStartDate,
                            Amount = payment.Amount,
                            LearningAimFundingLineType = payment.LearningAimFundingLineType,
                            ApprenticeshipId = payment.ApprenticeshipId
                        })
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<LearnerLevelViewHBCPInfo> GetHBCPInfoAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            LearnerLevelViewHBCPInfo hbcpInfo = null;

            using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
            {
                hbcpInfo = new LearnerLevelViewHBCPInfo()
                {
                    UkPrn = ukPrn,
                    HBCPModels = new List<LearnerLevelViewHBCPModel>()
                };

                var hbcpRecords = await dasPaymentsContext.RequiredPaymentEvents
                    .Where(x => x.Ukprn == ukPrn)
                    .Select(x => new LearnerLevelViewHBCPModel
                    {
                        UkPrn = x.Ukprn,
                        LearnerReferenceNumber = x.LearnerReferenceNumber,
                        LearnerUln = x.LearnerUln,
                        CollectionPeriod = x.CollectionPeriod,
                        DeliveryPeriod = x.DeliveryPeriod,
                        LearningAimReference = x.LearningAimReference,
                        NonPaymentReason = x.NonPaymentReason,
                    })
                    .Distinct()
                    .ToListAsync(cancellationToken);

                hbcpInfo.HBCPModels.AddRange(hbcpRecords);
            }

            return hbcpInfo;
        }

        public async Task<LearnerLevelViewDASDataLockInfo> GetDASDataLockInfoAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            LearnerLevelViewDASDataLockInfo dataLockInfo = null;
            try
            {
                using (IDASPaymentsContext dasPaymentsContext = _dasPaymentsContextFactory())
                {
                    dataLockInfo = new LearnerLevelViewDASDataLockInfo()
                    {
                        UkPrn = ukPrn,
                        DASDataLocks = new List<LearnerLevelViewDASDataLockModel>()
                    };

                    if (dasPaymentsContext.DataMatchReport.Any(x => x.UkPrn == ukPrn))
                    {
                        var dataLockValidationErrors = await dasPaymentsContext.DataMatchReport
                            .Where(x => x.UkPrn == ukPrn)
                            .Select(x => new LearnerLevelViewDASDataLockModel
                            {
                                UkPrn = x.UkPrn,
                                LearnerReferenceNumber = x.LearnerReferenceNumber,
                                LearnerUln = x.LearnerUln,
                                DataLockFailureId = x.DataLockFailureId,
                                LearningAimSequenceNumber = x.LearningAimSequenceNumber,
                                CollectionPeriod = x.CollectionPeriod,
                                DeliveryPeriod = x.DeliveryPeriod
                            })
                            .Distinct()
                            .ToListAsync(cancellationToken);

                        dataLockInfo.DASDataLocks.AddRange(dataLockValidationErrors);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get DASDataLockInfo data", ex);
                throw;
            }

            return dataLockInfo;
        }

        public async Task<AppsMonthlyPaymentDASInfo> GetPaymentsInfoForAppsMonthlyPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo = null;

            try
            {
                appsMonthlyPaymentDasInfo = new AppsMonthlyPaymentDASInfo
                {
                    UkPrn = ukPrn,
                    Payments = new List<AppsMonthlyPaymentDasPaymentModel>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using (var context = _dasPaymentsContextFactory())
                {
                    appsMonthlyPaymentDasInfo.Payments = await context.Payments
                        .Where(x => x.Ukprn == ukPrn && x.AcademicYear == Generics.AcademicYear)
                        .Select(payment => new AppsMonthlyPaymentDasPaymentModel
                        {
                            Ukprn = (int?)payment.Ukprn,
                            LearnerReferenceNumber = payment.LearnerReferenceNumber,
                            LearnerUln = payment.LearnerUln,
                            LearningAimReference = payment.LearningAimReference,
                            LearningStartDate = payment.LearningStartDate,
                            LearningAimProgrammeType = payment.LearningAimProgrammeType,
                            LearningAimStandardCode = payment.LearningAimStandardCode,
                            LearningAimFrameworkCode = payment.LearningAimFrameworkCode,
                            LearningAimPathwayCode = payment.LearningAimPathwayCode,
                            LearningAimFundingLineType = payment.LearningAimFundingLineType,
                            ReportingAimFundingLineType = payment.ReportingAimFundingLineType,
                            PriceEpisodeIdentifier = payment.PriceEpisodeIdentifier,
                            FundingSource = payment.FundingSource,
                            TransactionType = payment.TransactionType,
                            AcademicYear = payment.AcademicYear,
                            CollectionPeriod = payment.CollectionPeriod,
                            ContractType = payment.ContractType,
                            DeliveryPeriod = payment.DeliveryPeriod,
                            EarningEventId = payment.EarningEventId,
                            Amount = payment.Amount,
                            NonPaymentReason = payment.NonPaymentReason,
                            ApprenticeshipId = payment.ApprenticeshipId
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get Rulebase data", ex);
                throw;
            }

            return appsMonthlyPaymentDasInfo;
        }

        public async Task<AppsMonthlyPaymentDasEarningsInfo> GetEarningsInfoForAppsMonthlyPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentDasEarningsInfo appsMonthlyPaymentDasEarningsInfo = null;

            try
            {
                appsMonthlyPaymentDasEarningsInfo = new AppsMonthlyPaymentDasEarningsInfo
                {
                    UkPrn = ukPrn,
                    Earnings = new List<AppsMonthlyPaymentDasEarningEventModel>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                // List<Payment> paymentsList;
                using (var context = _dasPaymentsContextFactory())
                {
                    appsMonthlyPaymentDasEarningsInfo.Earnings = await context.EarningEvents
                        .Where(x => x.Ukprn == ukPrn)
                        .Select(earning => new AppsMonthlyPaymentDasEarningEventModel
                        {
                            Id = earning.Id,
                            EventId = earning.EventId,
                            Ukprn = (int?)earning.Ukprn,
                            ContractType = earning.ContractType,
                            CollectionPeriod = earning.CollectionPeriod,
                            AcademicYear = earning.AcademicYear,
                            LearnerReferenceNumber = earning.LearnerReferenceNumber,
                            LearnerUln = earning.LearnerUln,
                            LearningAimReference = earning.LearningAimReference,
                            LearningAimProgrammeType = earning.LearningAimProgrammeType,
                            LearningAimStandardCode = earning.LearningAimStandardCode,
                            LearningAimFrameworkCode = earning.LearningAimFrameworkCode,
                            LearningAimPathwayCode = earning.LearningAimPathwayCode,
                            LearningStartDate = earning.LearningStartDate,
                            AgreementId = earning.AgreementId,
                            IlrSubmissionDateTime = earning.IlrSubmissionDateTime,
                            JobId = earning.JobId,
                            EventTime = earning.EventTime,
                            CreationDate = earning.CreationDate,
                            LearningAimSequenceNumber = (byte?)earning.LearningAimSequenceNumber
                        }).ToListAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get Earning Event data", e);
            }

            return appsMonthlyPaymentDasEarningsInfo;
        }

        public async Task<List<AppsCoInvestmentRecordKey>> GetUniqueCombinationsOfKeyFromPaymentsAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (IDASPaymentsContext context = _dasPaymentsContextFactory())
            {
                return await context.Payments
                    .Where(p => p.Ukprn == ukprn
                                && p.AcademicYear <= Generics.AcademicYear)
                    .GroupBy(p =>
                    new
                    {
                        p.LearnerReferenceNumber,
                        p.LearningStartDate,
                        p.LearningAimProgrammeType,
                        p.LearningAimStandardCode,
                        p.LearningAimFrameworkCode,
                        p.LearningAimPathwayCode,
                    })
                    .Select(
                        g =>
                        new AppsCoInvestmentRecordKey(
                            g.Key.LearnerReferenceNumber,
                            g.Key.LearningStartDate,
                            g.Key.LearningAimProgrammeType,
                            g.Key.LearningAimStandardCode,
                            g.Key.LearningAimFrameworkCode,
                            g.Key.LearningAimPathwayCode))
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<AppsCoInvestmentPaymentsInfo> GetPaymentsInfoForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsCoInvestmentPaymentsInfo = new AppsCoInvestmentPaymentsInfo
            {
                UkPrn = ukPrn,
                Payments = new List<PaymentInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();
            using (IDASPaymentsContext context = _dasPaymentsContextFactory())
            {
                appsCoInvestmentPaymentsInfo.Payments =
                    await context.Payments
                    .Where(p =>
                        p.Ukprn == ukPrn
                        && p.AcademicYear <= Generics.AcademicYear
                        && (p.FundingSource == AppsCoInvestmentFundingType
                        || _appsCoInvestmentTransactionTypes.Contains(p.TransactionType)))
                    .Select(payment =>
                        new PaymentInfo()
                        {
                            FundingSource = payment.FundingSource,
                            TransactionType = payment.TransactionType,
                            AcademicYear = payment.AcademicYear,
                            CollectionPeriod = payment.CollectionPeriod,
                            ContractType = payment.ContractType,
                            DeliveryPeriod = payment.DeliveryPeriod,
                            LearnerReferenceNumber = payment.LearnerReferenceNumber,
                            LearnerUln = payment.LearnerUln,
                            LearningAimFrameworkCode = payment.LearningAimFrameworkCode,
                            LearningAimPathwayCode = payment.LearningAimPathwayCode,
                            LearningAimProgrammeType = payment.LearningAimProgrammeType,
                            LearningAimReference = payment.LearningAimReference,
                            LearningAimStandardCode = payment.LearningAimStandardCode,
                            LearningStartDate = payment.LearningStartDate,
                            UkPrn = payment.Ukprn,
                            Amount = payment.Amount,
                            PriceEpisodeIdentifier = payment.PriceEpisodeIdentifier,
                            SfaContributionPercentage = payment.SfaContributionPercentage,
                            ApprenticeshipId = payment.ApprenticeshipId,
                        }).ToListAsync(cancellationToken);
            }

            return appsCoInvestmentPaymentsInfo;
        }

        public async Task<IDictionary<long, string>> GetLegalEntityNameApprenticeshipIdDictionaryAsync(IEnumerable<long?> apprenticeshipIds, CancellationToken cancellationToken)
        {
            var uniqueApprenticeshipIds = apprenticeshipIds.Where(id => id.HasValue).Distinct().OrderBy(a => a).ToList();

            List<Tuple<long, string>> apprenticeshipIdLegalEntityNameCollection = new List<Tuple<long, string>>();

            var count = uniqueApprenticeshipIds.Count;
            var pageSize = 1000;

            using (IDASPaymentsContext context = _dasPaymentsContextFactory())
            {
                for (var i = 0; i < count; i += pageSize)
                {
                    var page = await context
                        .Apprenticeships
                        .Where(a => uniqueApprenticeshipIds.Skip(i).Take(pageSize).Contains(a.Id))
                        .Select(a => new Tuple<long, string>(a.Id, a.LegalEntityName))
                        .ToListAsync(cancellationToken);

                    apprenticeshipIdLegalEntityNameCollection.AddRange(page);
                }

                return apprenticeshipIdLegalEntityNameCollection.ToDictionary(a => a.Item1, a => a.Item2);
            }
        }
    }
}
