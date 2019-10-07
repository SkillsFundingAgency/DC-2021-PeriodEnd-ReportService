using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.EAS1920.EF;
using ESFA.DC.EAS1920.EF.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract;
using Microsoft.EntityFrameworkCore;
//using EasSubmission = ESFA.DC.EAS1920.EF.EasSubmission;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public class EasProviderService : AbstractFundModelProviderService, IEasProviderService
    {
        private readonly Func<IEasdbContext> _easContextFactory;

        public EasProviderService(
            ILogger logger,
            Func<IEasdbContext> easContextFactory)
            : base(logger)
        {
            _easContextFactory = easContextFactory;
        }

        public async Task<IList<ProviderEasInfo>> GetProviderEasInfoForFundingSummaryReport(
            int ukprn,
            CancellationToken cancellationToken)
        {
            List<ProviderEasInfo> providerEasSubmissionInfoList = new List<ProviderEasInfo>();

            // TODO: Refactor to use dictionary's for better performance
            try
            {
                // Get all the EAS submissions data for this provider
                var easSubmissionInfos = GetEasSubmissionInfo(ukprn, cancellationToken).GetAwaiter().GetResult();
                var easSubmissionValueInfos = GetEasSubmissionValueInfo(cancellationToken).GetAwaiter().GetResult();
                var easPaymentTypeInfos = GetEasPaymentTypeInfo(cancellationToken).GetAwaiter().GetResult();
                var easFundingLineInfos = GetEasFundingLineInfo(cancellationToken).GetAwaiter().GetResult();
                var easAdjustmentTypeInfos = GetEasAdjustmentTypeInfo(cancellationToken).GetAwaiter().GetResult();

                // Iterate through the submissions to build the model
                foreach (EasSubmissionInfo easSubmissionInfo in easSubmissionInfos)
                {
                    // get the matching EasSubmissionValues for the this easSubmissionInfo
                    var easSubmissionMatchedEasSubmissionValueInfos = easSubmissionValueInfos
                        .Where(x => x.SubmissionId == easSubmissionInfo?.SubmissionId).ToList();

                    // Iterate through the submission values to get the matching payment types in order to get the linking
                    // names for the FundingLine and AdjustmentTypes
                    foreach (EasSubmissionValueInfo easSubmissionValueInfo in easSubmissionMatchedEasSubmissionValueInfos)
                    {
                        // Not sure if there can be multiple payment types for a given eas submission value so might not need to iterate here
                        var easSubmissionValueMatchedPaymentTypeInfos = easPaymentTypeInfos
                            .Where(x => x.PaymentId == easSubmissionValueInfo.PaymentId).ToList();

                        // Iterate though the payment types to get the fund line and adjustment type id's
                        foreach (EasPaymentTypeInfo easPaymentType in easSubmissionValueMatchedPaymentTypeInfos)
                        {
                            string fundLineName = easFundingLineInfos?
                                    .SingleOrDefault(f => f.Id == easPaymentType?.FundingLineId)?.Name ?? string.Empty;

                            string adjustmentTypeName = easAdjustmentTypeInfos?
                                    .SingleOrDefault(a => a.Id == easPaymentType?.AdjustmentTypeId)?
                                    .Name ?? string.Empty;

                            providerEasSubmissionInfoList.Add(
                                BuildProviderEasInfoModel(fundLineName, adjustmentTypeName,
                                    easSubmissionValueInfo.CollectionPeriod, easSubmissionValueInfo.PaymentValue));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get Provider EAS data", ex);
                throw;
            }

            return providerEasSubmissionInfoList;
        }

        public async Task<IList<EasSubmissionInfo>> GetEasSubmissionInfo(int ukprn, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.EasSubmissions
                        .Where(x => x.Ukprn == ukprn.ToString())
                        .Select(s => new EasSubmissionInfo
                        {
                            SubmissionId = s.SubmissionId,
                            Ukprn = s.Ukprn,
                            CollectionPeriod = (byte?)s.CollectionPeriod
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Submission data", ex);
                throw;
            }
        }

        public async Task<IList<EasSubmissionValueInfo>> GetEasSubmissionValueInfo(CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.EasSubmissionValues
                        .Select(v => new EasSubmissionValueInfo
                        {
                            SubmissionId = v.SubmissionId,
                            CollectionPeriod = (byte?)v.CollectionPeriod,
                            PaymentId = v.PaymentId,
                            PaymentValue = v.PaymentValue
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Submission Value data", ex);
                throw;
            }
        }

        public async Task<IList<EasPaymentTypeInfo>> GetEasPaymentTypeInfo(CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.PaymentTypes
                        .Select(p => new EasPaymentTypeInfo
                        {
                            PaymentId = p.PaymentId,
                            PaymentName = p.PaymentName,
                            PaymentTypeDescription = p.PaymentTypeDescription,
                            FundingLineId = p.FundingLineId,
                            AdjustmentTypeId = p.AdjustmentTypeId,
                            Fm36 = p.Fm36
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Payment Type data", ex);
                throw;
            }
        }

        public async Task<IList<EasAdjustmentTypeInfo>> GetEasAdjustmentTypeInfo(CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.AdjustmentTypes
                        .Select(a => new EasAdjustmentTypeInfo
                        {
                            Id = a.Id,
                            Name = a.Name
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Adjustment Type data", ex);
                throw;
            }
        }

        public async Task<IList<EasFundingLineInfo>> GetEasFundingLineInfo(CancellationToken cancellationToken)
        {
            try
            {
                using (var context = _easContextFactory())
                {
                    return await context.FundingLines
                        .Select(a => new EasFundingLineInfo
                        {
                            Id = a.Id,
                            Name = a.Name
                        })
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS Funding Line data", ex);
                throw;
            }
        }

        private ProviderEasInfo BuildProviderEasInfoModel(string fundLineName, string adjustmentTypeName, byte? collectionPeriod, decimal? paymentValue)
        {
            // create an instance of the final providerEasSubmissionInfo model
            ProviderEasInfo providerEasInfo = new ProviderEasInfo();

            if (collectionPeriod != null)
            {
                // assign the fundline and adjustment type
                providerEasInfo.FundLine = fundLineName;
                providerEasInfo.AdjustmentType = adjustmentTypeName;

                // default the period values to zero
                providerEasInfo.Period1 = 0m;
                providerEasInfo.Period2 = 0m;
                providerEasInfo.Period3 = 0m;
                providerEasInfo.Period4 = 0m;
                providerEasInfo.Period5 = 0m;
                providerEasInfo.Period6 = 0m;
                providerEasInfo.Period7 = 0m;
                providerEasInfo.Period8 = 0m;
                providerEasInfo.Period9 = 0m;
                providerEasInfo.Period10 = 0m;
                providerEasInfo.Period11 = 0m;
                providerEasInfo.Period12 = 0m;

                // assign the payment value to the appropriate collection period
                switch (collectionPeriod)
                {
                    case 1:
                        providerEasInfo.Period1 = collectionPeriod;
                        break;

                    case 2:
                        providerEasInfo.Period2 = collectionPeriod;
                        break;

                    case 3:
                        providerEasInfo.Period3 = collectionPeriod;
                        break;

                    case 4:
                        providerEasInfo.Period4 = collectionPeriod;
                        break;

                    case 5:
                        providerEasInfo.Period5 = collectionPeriod;
                        break;

                    case 6:
                        providerEasInfo.Period6 = collectionPeriod;
                        break;

                    case 7:
                        providerEasInfo.Period7 = collectionPeriod;
                        break;

                    case 8:
                        providerEasInfo.Period8 = collectionPeriod;
                        break;

                    case 9:
                        providerEasInfo.Period9 = collectionPeriod;
                        break;

                    case 10:
                        providerEasInfo.Period10 = collectionPeriod;
                        break;

                    case 11:
                        providerEasInfo.Period11 = collectionPeriod;
                        break;

                    case 12:
                        providerEasInfo.Period12 = collectionPeriod;
                        break;

                    default:
                        break;
                }
            }

            return providerEasInfo;
        }
    }
}
