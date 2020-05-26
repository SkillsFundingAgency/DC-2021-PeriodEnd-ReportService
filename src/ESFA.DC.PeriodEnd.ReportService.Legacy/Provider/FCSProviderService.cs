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

namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Provider
{
    public sealed class FCSProviderService : IFCSProviderService
    {
        private readonly ILogger _logger;
        private readonly Func<IFcsContext> _fcsContextFunc;

        private readonly DateTime _academicYearStartDate = new DateTime(2019, 08, 01);
        private readonly DateTime _academicYearEndDate = new DateTime(2020, 07, 31);

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
                    // Get a list of fcs contracts by Ukprn
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
                            ContractNumber = fcsContract?.ContractNumber,
                            ContractVersionNumber = fcsContract?.ContractVersionNumber.ToString(),
                            StartDate = fcsContract?.StartDate,
                            EndDate = fcsContract?.EndDate,
                            ContractAllocations = fcsContract?.ContractAllocations.Select(x => new AppsMonthlyPaymentContractAllocationInfo
                            {
                                ContractAllocationNumber = x?.ContractAllocationNumber,
                                Period = x?.Period,
                                PeriodTypeCode = x?.PeriodTypeCode,
                                FundingStreamCode = x?.FundingStreamCode,
                                FundingStreamPeriodCode = x?.FundingStreamPeriodCode,
                                StartDate = x?.StartDate,
                                EndDate = x?.EndDate
                            }).ToList() ?? new List<AppsMonthlyPaymentContractAllocationInfo>(),
                            Provider = new AppsMonthlyPaymentContractorInfo()
                            {
                                UkPrn = fcsContract?.Contractor?.Ukprn,
                                OrganisationIdentifier = fcsContract?.Contractor?.OrganisationIdentifier,
                                LegalName = fcsContract?.Contractor?.LegalName
                            }
                        };

                        appsMonthlyPaymentFcsInfo.Contracts.Add(appsMonthlyPaymentContractInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get FCS Contracts", ex);
                throw;
            }

            return appsMonthlyPaymentFcsInfo;
        }

        public async Task<IDictionary<string, string>> GetContractAllocationNumberFSPCodeLookupAsync(int ukprn, CancellationToken cancellationToken)
        {
            using (var context = _fcsContextFunc())
            {
                var allocations = await context
                    .ContractAllocations
                    .Where(ca => ca.DeliveryUkprn == ukprn
                                 && ca.StartDate <= _academicYearEndDate
                                 && (!ca.EndDate.HasValue || ca.EndDate >= _academicYearStartDate))
                    .GroupBy(ca => ca.FundingStreamPeriodCode)
                    .ToListAsync(cancellationToken);

                var lookupDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var allocation in allocations)
                {
                    lookupDictionary.Add(allocation.Key, string.Join(";", allocation.OrderByDescending(a => a.Id).Select(a => a.ContractAllocationNumber)));
                }

                return lookupDictionary;
            }
        }
    }
}
