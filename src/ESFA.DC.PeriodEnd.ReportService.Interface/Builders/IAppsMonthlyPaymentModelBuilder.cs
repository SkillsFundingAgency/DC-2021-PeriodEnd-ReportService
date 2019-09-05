﻿using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders
{
    public interface IAppsMonthlyPaymentModelBuilder
    {
        IReadOnlyList<AppsMonthlyPaymentModel> BuildModel(
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo,
            AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList);
    }
}