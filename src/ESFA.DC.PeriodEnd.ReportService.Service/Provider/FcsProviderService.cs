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
                    // Get a list of fcs contracts by UKPRN (need to link to the Contractor table for the UKPRN)
                    var fcsContracts = await fcsContext.Contracts
                        .Include(x => x.Contractor)
                        .Where(x => x.Contractor.Ukprn == ukPrn)
                        .ToListAsync(cancellationToken);

                    // copy the fields we need from the EF model to our report model
                    foreach (var fcsContract in fcsContracts)
                    {
                        var appsMonthlyPaymentContractInfo = new AppsMonthlyPaymentContractInfo()
                        {
                            ContractNumber = fcsContract.ContractNumber,
                            ContractVersionNumber = fcsContract.ContractVersionNumber,
                            StartDate = fcsContract.StartDate,
                            EndDate = fcsContract.EndDate,
                            ContractAllocations = fcsContract.ContractAllocations.Select(x =>
                                new AppsMonthlyPaymentContractAllocation()
                                {
                                    ContractAllocationNumber = x.ContractAllocationNumber,
                                    Period = x.Period,
                                    PeriodTypeCode = x.PeriodTypeCode,
                                    FundingStreamCode = x.FundingStreamCode,
                                    FundingStreamPeriodCode = x.FundingStreamPeriodCode,
                                    StartDate = x.StartDate,
                                    EndDate = x.EndDate
                                }).ToList(),
                            Provider = new AppsMonthlyPaymentContractorInfo()
                            {
                                UkPrn = fcsContract.Contractor.Ukprn,
                                OrganisationIdentifier = fcsContract.Contractor.OrganisationIdentifier,
                                LegalName = fcsContract.Contractor.LegalName
                            }
                        };
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
