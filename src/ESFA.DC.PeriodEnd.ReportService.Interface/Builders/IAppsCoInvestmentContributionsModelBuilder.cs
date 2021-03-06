﻿using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface IAppsCoInvestmentContributionsModelBuilder
    {
        IEnumerable<AppsCoInvestmentContributionsModel> BuildModel(
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            AppsCoInvestmentRulebaseInfo appsCoInvestmentRulebaseInfo,
            AppsCoInvestmentPaymentsInfo appsCoInvestmentPaymentsInfo,
            List<AppsCoInvestmentRecordKey> paymensAppCoInvestmentRecordKeys,
            List<AppsCoInvestmentRecordKey> ilrAppCoInvestmentRecordKeys,
            IDictionary<long, string> apprenticeshipIdLegalEntityNameDictionary,
            long jobId,
            int upkrn);
    }
}