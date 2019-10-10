using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.FundingSummaryReport
{
    public class FundingSummaryReportData
    {
        private Dictionary<string, Dictionary<string, decimal?[][]>> Fm25LearnerPeriodisedValues;
        private Dictionary<string, Dictionary<string, decimal?[][]>> Fm35LearningDeliveryPeriodisedValues;
        private Dictionary<string, Dictionary<string, decimal?[][]>> Fm36PaymentsPeriodisedValues;

        private Dictionary<string, Dictionary<string, decimal?[][]>> EasFm36PeriodisedValues;
        private Dictionary<string, Dictionary<string, decimal?[][]>> EasExceptFm36PeriodisedValues;
    }
}
