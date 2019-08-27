using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.ReferenceData.FCS.Model;
using ESFA.DC.ReferenceData.FCS.Model.Interface;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider
{
    public sealed class FcsProviderService : IFcsProviderService
    {
        private readonly ILogger _logger;
        private readonly Func<IFcsContext> _fcsContext;

        public FcsProviderService(ILogger logger, Func<IFcsContext> fcsContext)
        {
            _logger = logger;
            _fcsContext = fcsContext;
        }

        public async Task<AppsMonthlyPaymentFcsInfo> GetFcsInfoForAppsMonthlyPaymentReportAsync(
            int ukPrn,
            CancellationToken cancellationToken)
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

                using (var fcsContext = _fcsContext())
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

        public string GetFcsContractAllocationNumber(
            int ukPrn,
            string fundingStreamPeriodCode)
        {
            string contractAllocationNumber = string.Empty;

            try
            {
                using (var fcsContext = _fcsContext())
                {
                    var contract = fcsContext.Contracts
                        .Include(x => x.Contractor)
                        .Include(x => x.ContractAllocations)
                        .Where(x => x.Contractor.Ukprn == ukPrn &&
                                    x.ContractAllocations.Any(y =>
                                        y.FundingStreamPeriodCode == fundingStreamPeriodCode))
                        .FirstOrDefault();

                    if (contract != null && contract.ContractAllocations != null)
                    {
                        contractAllocationNumber =
                            contract.ContractAllocations.FirstOrDefault().ContractAllocationNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get FCS Contracts", ex);
            }

            return contractAllocationNumber;
        }

        //appsMonthlyPaymentFcsInfo.UkPrn = contract.Contractor.Ukprn ?? throw new InvalidDataException(string.Format("Exception in {0}: {1} cannot be null", nameof(this.CreateAppsMonthlyPaymentFcsInfoFromEFModel), nameof(contract.Contractor.Ukprn)));
    }
}
