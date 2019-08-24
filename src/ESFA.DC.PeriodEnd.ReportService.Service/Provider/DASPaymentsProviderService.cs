using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private const int FundingSource = 3;
        private int[] AppsAdditionalPaymentsTransactionTypes = { 4, 5, 6, 7, 16 };
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
        }

        public async Task<AppsAdditionalPaymentDasPaymentsInfo> GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsAdditionalPaymentDasPaymentsInfo = new AppsAdditionalPaymentDasPaymentsInfo
            {
                UkPrn = ukPrn,
                Payments = new List<DASPaymentInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<Payment> paymentsList;
            List<Apprenticeship> apprenticeships;
            using (var context = _dasPaymentsContextFactory())
            {
                paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn &&
                                                                x.FundingSource == FundingSource &&
                                                                 AppsAdditionalPaymentsTransactionTypes.Contains(x.TransactionType)).ToListAsync(cancellationToken);

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
                    EmployerName = apprenticeships?.SingleOrDefault(a => a.Id == payment.ApprenticeshipId)?.LegalEntityName ?? string.Empty
                };

                appsAdditionalPaymentDasPaymentsInfo.Payments.Add(paymentInfo);
            }

            return appsAdditionalPaymentDasPaymentsInfo;
        }

        public async Task<AppsMonthlyPaymentDASInfo> GetPaymentsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentDasInfo = new AppsMonthlyPaymentDASInfo
            {
                UkPrn = ukPrn,
                Payments = new List<AppsMonthlyPaymentDasPayments2Payment>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            using (var context = _dasPaymentsContextFactory())
            {
                appsMonthlyPaymentDasInfo.Payments = await context.Payments
                    .Where(x => x.Ukprn == ukPrn && x.FundingSource == FundingSource)
                    .Select(payment => new AppsMonthlyPaymentDasPayments2Payment
                    {
                        // Convert the database null values to a default value so that we don't have to keep checking for null later
                        // and to stop exceptions where we're not able to check for null e.g. in LINQ statements
                        // Also give 'not null' columns a default value as the table definition may change at a later date causing our code to break
                        Ukprn = (payment != null && payment.Ukprn != null) ? payment.Ukprn.ToString() : string.Empty,
                        LearnerReferenceNumber = (payment != null && payment.LearnerReferenceNumber != null) ? payment.LearnerReferenceNumber : string.Empty,
                        LearnerUln = (payment != null && payment.LearnerUln != null) ? payment.LearnerUln.ToString() : string.Empty,
                        LearningAimReference = (payment != null && payment.LearningAimReference != null) ? payment.LearningAimReference : string.Empty,
                        LearningStartDate = (payment != null && payment.LearningStartDate != null) ? payment.LearningStartDate.ToString() : string.Empty,
                        LearningAimProgrammeType = (payment != null && payment.LearningAimProgrammeType != null) ? payment.LearningAimProgrammeType.ToString() : "25", // for FM36 ProgType should be 25
                        LearningAimStandardCode = (payment != null && payment.LearningAimStandardCode != null) ? payment.LearningAimStandardCode.ToString() : string.Empty,
                        LearningAimFrameworkCode = (payment != null && payment.LearningAimFrameworkCode != null) ? payment.LearningAimFrameworkCode.ToString() : string.Empty,
                        LearningAimPathwayCode = (payment != null && payment.LearningAimPathwayCode != null) ? payment.LearningAimPathwayCode.ToString() : string.Empty,
                        LearningAimFundingLineType = (payment != null && payment.LearningAimFundingLineType != null) ? payment.LearningAimFundingLineType : string.Empty,
                        ReportingAimFundingLineType = (payment != null && payment.ReportingAimFundingLineType != null) ? payment.ReportingAimFundingLineType : string.Empty,
                        PriceEpisodeIdentifier = payment.PriceEpisodeIdentifier,
                        FundingSource = payment.FundingSource,
                        TransactionType = payment.TransactionType,
                        AcademicYear = payment.AcademicYear,
                        CollectionPeriod = payment.CollectionPeriod,
                        ContractType = (payment != null && payment.ContractType != null) ? payment.ContractType.ToString() : string.Empty,
                        DeliveryPeriod = (payment != null && payment.DeliveryPeriod != null) ? payment.DeliveryPeriod.ToString() : string.Empty,
                        EarningEventId = payment.EarningEventId,
                        Amount = (payment != null && payment.Amount != null) ? payment.Amount : 0m
                    })
                    .ToListAsync(cancellationToken);

                return appsMonthlyPaymentDasInfo;
            }
        }

        public async Task<AppsMonthlyPaymentDasEarningsInfo> GetEarningsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentDasEarningsInfo = new AppsMonthlyPaymentDasEarningsInfo
            {
                UkPrn = ukPrn,
                Earnings = new List<AppsMonthlyPaymentDasEarningEventInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            // List<Payment> paymentsList;
            using (var context = _dasPaymentsContextFactory())
            {
                appsMonthlyPaymentDasEarningsInfo.Earnings = await context.EarningEvents
                    .Where(x => x.Ukprn == ukPrn)
                    .Select(earning => new AppsMonthlyPaymentDasEarningEventInfo
                    {
                        Id = earning.Id,
                        EventId = earning.EventId,
                        Ukprn = earning.Ukprn,
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
                        LearningAimSequenceNumber = earning.LearningAimSequenceNumber
                    }).ToListAsync(cancellationToken);

                return appsMonthlyPaymentDasEarningsInfo;
            }
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
