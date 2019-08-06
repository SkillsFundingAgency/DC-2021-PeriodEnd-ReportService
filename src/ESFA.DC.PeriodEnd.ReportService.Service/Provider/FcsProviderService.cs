﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Provider;
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
                                        .Where(x => x.Contractor.Ukprn == ukPrn && x.ContractAllocations.Any(y => y.FundingStreamPeriodCode == fundingStreamPeriodCode))
                                        .FirstOrDefault();

                    if (contract != null && contract.ContractAllocations != null)
                    {
                        contractAllocationNumber = contract.ContractAllocations.FirstOrDefault().ContractAllocationNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get FCS Contracts", ex);
            }

            return contractAllocationNumber;
        }
    }
}
