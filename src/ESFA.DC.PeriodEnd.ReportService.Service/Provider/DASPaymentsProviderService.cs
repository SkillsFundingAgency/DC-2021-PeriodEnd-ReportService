using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
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

        public async Task<AppsAdditionalPaymentDasPaymentsInfo> GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(
            int ukPrn, CancellationToken cancellationToken)
        {
            var appsAdditionalPaymentDasPaymentsInfo = new AppsAdditionalPaymentDasPaymentsInfo
            {
                UkPrn = ukPrn,
                Payments = new List<DASPaymentInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            using (var context = _dasPaymentsContextFactory())
            {
                var paymentsList =
                    await (from payment in context.Payments
                           join apprenticeships in context.Apprenticeships on payment.ApprenticeshipId equals apprenticeships.Id
                           into payment_apprenticeship_join
                           from payment_apprenticeship in payment_apprenticeship_join.DefaultIfEmpty()
                           where payment.Ukprn == ukPrn &&
                                 payment.FundingSource == Constants.DASPayments.FundingSource.Fully_Funded_SFA &&
                                 _appsAdditionalPaymentsTransactionTypes.Contains(payment.TransactionType)
                           select new DASPaymentInfo()
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
                               Amount = payment.Amount,
                               LearningAimFundingLineType = MapOldFundingLineTypes(payment.LearningAimFundingLineType),
                               TypeOfAdditionalPayment = GetTypeOfAdditionalPayment(payment.TransactionType),
                               EmployerName = GetAppServiceEmployerName(payment, payment_apprenticeship.LegalEntityName)
                           }).ToListAsync(cancellationToken);

                appsAdditionalPaymentDasPaymentsInfo.Payments.AddRange(paymentsList);
            }

            return appsAdditionalPaymentDasPaymentsInfo;
        }

        public string GetAppServiceEmployerName(Payment payment, string legalEntityName)
        {
            string name = string.Empty;

            if (payment.ContractType == 1 && (payment.TransactionType == 4 || payment.TransactionType == 6))
            {
                name = legalEntityName ?? string.Empty;
            }

            return name;
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
                        .Where(x => x.Ukprn == ukPrn && x.AcademicYear == 1920)
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
                            Amount = payment.Amount
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
                            LearningAimFundingLineType = MapOldFundingLineTypes(earning.LearningAimFundingLineType),
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

        public string MapOldFundingLineTypes(string fundingLineType)
        {
            string newFundingLineType = string.Empty;

            switch (fundingLineType.ToUpper())
            {
                case @"16 - 18 APPRENTICESHIP(FROM MAY 2017) NON - LEVY CONTRACT":
                    newFundingLineType = @"16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)";
                    break;

                case @"19+ APPRENTICESHIP (FROM MAY 2017) NON-LEVY CONTRACT":
                    newFundingLineType = @"19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)";
                    break;

                default:
                    newFundingLineType = fundingLineType;
                    break;
            }

            return newFundingLineType;
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
                var paymentsList =
                    await (from payment in context.Payments
                           join apprenticeships in context.Apprenticeships on payment.ApprenticeshipId equals apprenticeships.Id into groupjoin
                           from subapps in groupjoin.DefaultIfEmpty()
                           where payment.Ukprn == ukPrn &&
                                 payment.FundingSource == AppsCoInvestmentFundingType &&
                                 _appsCoInvestmentTransactionTypes.Contains(payment.TransactionType)
                           select new PaymentInfo()
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
                               EmployerName = subapps.LegalEntityName ?? string.Empty
                           }).ToListAsync(cancellationToken);

                appsCoInvestmentPaymentsInfo.Payments.AddRange(paymentsList);
            }

            return appsCoInvestmentPaymentsInfo;
        }

        private string GetTypeOfAdditionalPayment(byte transactionType)
        {
            switch (transactionType)
            {
                case 4:
                case 6:
                    return "Employer";
                case 5:
                case 7:
                    return "Provider";
                case 16:
                    return "Apprentice";
                default:
                    return string.Empty;
            }
        }
    }
}
