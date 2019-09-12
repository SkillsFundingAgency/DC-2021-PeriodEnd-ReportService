using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private int[] AppsAdditionalPaymentsTransactionTypes = {
            Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive,
            Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive,
            Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive,
            Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive,
            Constants.DASPayments.TransactionType.Apprenticeship };

        private const int _fundingSource = 3;
        private int[] AppsAdditionalPaymentsTransactionTypes = { 4, 5, 6, 7, 16 };
        private int[] TransactionTypes = { 1, 2, 3 };
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
                                                            x.FundingSource == Constants.DASPayments.FundingSource.Fully_Funded_SFA &&
                                                            AppsAdditionalPaymentsTransactionTypes.Contains(x.TransactionType))
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
                Payments = new List<AppsMonthlyPaymentDASPaymentInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<Payment> paymentsList;
            using (var context = _dasPaymentsContextFactory())
            {
                paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn && x.FundingSource == Constants.DASPayments.FundingSource.Co_Invested_Employer).ToListAsync(cancellationToken);
            }

            foreach (var payment in paymentsList)
            {
                var paymentInfo = new AppsMonthlyPaymentDASPaymentInfo
                {
                    LearnerReferenceNumber = payment.LearnerReferenceNumber,
                    LearnerUln = payment.LearnerUln,
                    LearningAimReference = payment.LearningAimReference,
                    LearningAimProgrammeType = payment.LearningAimProgrammeType,
                    LearningAimStandardCode = payment.LearningAimStandardCode,
                    LearningAimFrameworkCode = payment.LearningAimFrameworkCode,
                    LearningAimPathwayCode = payment.LearningAimPathwayCode,
                    Amount = payment.Amount,
                    LearningAimFundingLineType = payment.LearningAimFundingLineType,
                    PriceEpisodeIdentifier = payment.PriceEpisodeIdentifier,
                    FundingSource = payment.FundingSource,
                    TransactionType = payment.TransactionType,
                    AcademicYear = payment.AcademicYear,
                    CollectionPeriod = payment.CollectionPeriod,
                    ContractType = payment.ContractType,
                    DeliveryPeriod = payment.DeliveryPeriod,
                    LearningStartDate = payment.LearningStartDate
                };

                appsMonthlyPaymentDasInfo.Payments.Add(paymentInfo);
            }

            return appsMonthlyPaymentDasInfo;
        }

        public async Task<AppsCoInvestmentPaymentsInfo> GetPaymentsInfoForAppsCoInvestmentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsCoInvestmentPaymentsInfo = new AppsCoInvestmentPaymentsInfo
            {
                UkPrn = ukPrn,
                Payments = new List<PaymentInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();
            List<Payment> paymentsList;
            List<Apprenticeship> apprenticeships;
            using (IDASPaymentsContext context = _dasPaymentsContextFactory())
            {
                paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn &&
                                                                x.FundingSource == _fundingSource &&
                                                                TransactionTypes.Contains(x.TransactionType)).ToListAsync(cancellationToken);
                apprenticeships = await context.Apprenticeships.Join(
                    paymentsList,
                    a => a.Id,
                    p => p.ApprenticeshipId,
                    (a, p) => a).ToListAsync(cancellationToken);
            }

            foreach (var payment in paymentsList)
            {
                var paymentInfo = new PaymentInfo
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
                    EmployerName = apprenticeships?.SingleOrDefault(a => a.Id == payment.ApprenticeshipId)?.LegalEntityName ?? string.Empty
                };

                appsCoInvestmentPaymentsInfo.Payments.Add(paymentInfo);
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
