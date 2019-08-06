using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class DASPaymentsProviderService : IDASPaymentsProviderService
    {
        private readonly ILogger _logger;
        private readonly Func<IDASPaymentsContext> _dasPaymentsContextFactory;

        public DASPaymentsProviderService(ILogger logger, Func<IDASPaymentsContext> dasPaymentsContextFactory)
        {
            _logger = logger;
            _dasPaymentsContextFactory = dasPaymentsContextFactory;
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
                    Payments = new List<AppsMonthlyPaymentDASPaymentInfo>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                // get the base payments data for report (ACT 1 payments for 1920 academic year)
                List<Payment> paymentsList = null;
                using (var context = _dasPaymentsContextFactory())
                {
                    //paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn && x.FundingSource == FundingSource).ToListAsync(cancellationToken);

                    paymentsList = await context.Payments.Where(x => x.Ukprn == ukPrn
                                                                //&& x.AcademicYear == 1920  //TODO: uncomment when 1920 packagages are available
                                                                && x.AcademicYear == 1819
                                                                && x.ContractType == 1)
                                                               .ToListAsync(cancellationToken);
                }

                foreach (var payment in paymentsList)
                {
                    var paymentInfo = new AppsMonthlyPaymentDASPaymentInfo
                    {
                        UkPrn = (int)payment.Ukprn,
                        LearnerReferenceNumber = payment.LearnerReferenceNumber,
                        LearnerUln = payment.LearnerUln,
                        LearningAimReference = payment.LearningAimReference,
                        LearningAimProgrammeType = payment.LearningAimProgrammeType,
                        LearningAimStandardCode = payment.LearningAimStandardCode,
                        LearningAimFrameworkCode = payment.LearningAimFrameworkCode,
                        LearningAimPathwayCode = payment.LearningAimPathwayCode,
                        LearningAimFundingLineType = payment.LearningAimFundingLineType,
                        PriceEpisodeIdentifier = payment.PriceEpisodeIdentifier,
                        FundingSource = payment.FundingSource,
                        TransactionType = payment.TransactionType,
                        AcademicYear = payment.AcademicYear,
                        CollectionPeriod = payment.CollectionPeriod,
                        ContractType = payment.ContractType,
                        DeliveryPeriod = payment.DeliveryPeriod,
                        LearningStartDate = payment.LearningStartDate,
                        Amount = payment.Amount
                    };

                    appsMonthlyPaymentDasInfo.Payments.Add(paymentInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get DAS Payments", ex);
            }

            return appsMonthlyPaymentDasInfo;
        }
    }
}
