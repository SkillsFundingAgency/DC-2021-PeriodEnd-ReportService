﻿using System;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsMonthly.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly.Builders
{
    public class EarningBuilder : AbstractBuilder<Earning>
    {
        public static Guid EventId { get; } = Guid.Parse("12345678-1234-1234-1234-123456789012");
        public const int AimSequenceNumber = 1;

        public EarningBuilder()
        {
            modelObject = new Earning()
            {
                EventId = EventId,
                AimSequenceNumber = AimSequenceNumber,
            };
        }
    }
}
