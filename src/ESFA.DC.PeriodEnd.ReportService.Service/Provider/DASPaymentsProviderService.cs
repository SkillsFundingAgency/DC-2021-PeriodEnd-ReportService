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
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class DASPaymentsProviderService : AbstractFundModelProviderService, IDASPaymentsProviderService
    {
        private const int FundingSource = 3;
        private readonly int[] _appsAdditionalPaymentsTransactionTypes = { 4, 5, 6, 7, 16 };
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

            List<Payment> paymentsList;
            List<Apprenticeship> apprenticeships;
            var context = _dasPaymentsContextFactory();

            if (context != null)
            {
                using (context)
                {
                    paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn &&
                                                                     x.FundingSource == FundingSource &&
                                                                     _appsAdditionalPaymentsTransactionTypes.Contains(
                                                                         x.TransactionType))
                        .ToListAsync(cancellationToken);

                    apprenticeships = await context.Apprenticeships.Join(
                        paymentsList,
                        a => a.Id,
                        p => p.ApprenticeshipId,
                        (a, p) => a).ToListAsync(cancellationToken);
                }

                foreach (var payment in paymentsList)
                {
                    var paymentInfo = new DASPaymentInfo
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
                        Amount = payment.Amount,
                        LearningAimFundingLineType = payment.LearningAimFundingLineType,
                        TypeOfAdditionalPayment = GetTypeOfAdditionalPayment(payment.TransactionType),
                        EmployerName =
                            apprenticeships?.SingleOrDefault(a => a.Id == payment.ApprenticeshipId)?.LegalEntityName ??
                            string.Empty
                    };

                    appsAdditionalPaymentDasPaymentsInfo.Payments.Add(paymentInfo);
                }
            }

            return appsAdditionalPaymentDasPaymentsInfo;
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

                var context = _dasPaymentsContextFactory();
                if (context != null)
                {
                    using (context)
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
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get Rulebase data", ex);
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

                var context = _dasPaymentsContextFactory();
                if (context != null)
                {
                    using (context)
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
                                LearningAimFundingLineType = earning.LearningAimFundingLineType,
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
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get Earning Event data", e);
            }

            return appsMonthlyPaymentDasEarningsInfo;
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
