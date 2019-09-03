using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.ReferenceData.FCS.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class FCSProviderService : IFCSProviderService
    {
        private readonly ILogger _logger;
        private readonly Func<IFcsContext> _fcsContextFunc;

        public FCSProviderService(ILogger logger, Func<IFcsContext> fcsContext)
        {
            _logger = logger;
            _fcsContextFunc = fcsContext;
        }

        public async Task<List<DataExtractFcsInfo>> GetFCSForDataExtractReport(IEnumerable<string> OrganisationIds, CancellationToken cancellationToken)
        {
            using (IFcsContext fcsContext = _fcsContextFunc())
            {
                return await fcsContext.Contractors
                    .Include(x => x.Contracts)
                    .Where(x => OrganisationIds.Contains(x.OrganisationIdentifier, StringComparer.OrdinalIgnoreCase))
                    .GroupBy(x => new { x.OrganisationIdentifier, x.Ukprn })
                    .Select(x => new DataExtractFcsInfo
                    {
                        OrganisationIdentifier = x.Key.OrganisationIdentifier,
                        UkPrn = x.Key.Ukprn
                    }).ToListAsync(cancellationToken);
            }
        }

        public async Task<AppsMonthlyPaymentFcsInfo> GetFcsInfoForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            AppsMonthlyPaymentFcsInfo appsMonthlyPaymentFcsInfo = null;

            try
            {
                appsMonthlyPaymentFcsInfo = new AppsMonthlyPaymentFcsInfo()
                {
                    UkPrn = ukPrn,
                    Contracts = new List<AppsMonthlyPaymentContractInfo>()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using (var fcsContext = _fcsContextFunc())
                {
                    // Get a list of fcs contracts by Ukprn (need to link to the Contractor table for the Ukprn)
                    var fcsContracts = await fcsContext.Contracts
                        .Include(x => x.Contractor)
                        .Include(y => y.ContractAllocations)
                        .Where(x => x.Contractor.Ukprn == ukPrn)
                        .ToListAsync(cancellationToken);

                    // copy the fields we need from the EF model to our report model
                    foreach (var fcsContract in fcsContracts)
                    {
                        var appsMonthlyPaymentContractInfo = new AppsMonthlyPaymentContractInfo()
                        {
                            ContractNumber = fcsContract?.ContractNumber ?? string.Empty,
                            ContractVersionNumber = fcsContract?.ContractVersionNumber.ToString() ?? string.Empty,
                            StartDate = fcsContract?.StartDate.ToString() ?? string.Empty,
                            EndDate = fcsContract?.EndDate.ToString() ?? string.Empty,
                            ContractAllocations = fcsContract?.ContractAllocations.Select(x => new AppsMonthlyPaymentContractAllocation
                            {
                                ContractAllocationNumber = x?.ContractAllocationNumber ?? string.Empty,
                                Period = x?.Period ?? string.Empty,
                                PeriodTypeCode = x?.PeriodTypeCode ?? string.Empty,
                                FundingStreamCode = x?.FundingStreamCode ?? string.Empty,
                                FundingStreamPeriodCode = x?.FundingStreamPeriodCode ?? string.Empty,
                                StartDate = x?.StartDate.ToString() ?? string.Empty,
                                EndDate = x?.EndDate.ToString() ?? string.Empty
                            }).ToList() ?? new List<AppsMonthlyPaymentContractAllocation>(),
                            Provider = new AppsMonthlyPaymentContractorInfo()
                            {
                                UkPrn = fcsContract?.Contractor?.Ukprn.ToString() ?? string.Empty,
                                OrganisationIdentifier = fcsContract?.Contractor?.OrganisationIdentifier ?? string.Empty,
                                LegalName = fcsContract?.Contractor?.LegalName ?? string.Empty
                            }
                        };

                        appsMonthlyPaymentFcsInfo.Contracts.Add(appsMonthlyPaymentContractInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get FCS Contracts", ex);
            }

            return appsMonthlyPaymentFcsInfo;
        }
    }
}
