﻿using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentAECLearningDeliveryInfo
    {
        public string Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public int? AimSequenceNumber { get; set; }

        public string LearnAimRef { get; set; }

        public int PlannedNumOnProgInstalm { get; set; }
    }
}