﻿using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract
{
    public abstract class AbstractFundModelProviderService
    {
        protected readonly ILogger _logger;

        protected AbstractFundModelProviderService(ILogger logger)
        {
            _logger = logger;
        }
    }
}
