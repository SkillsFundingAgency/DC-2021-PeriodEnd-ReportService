using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private const int FundingSource = 3;
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
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
                paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn && x.FundingSource == FundingSource).ToListAsync(cancellationToken);
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
    }
}
