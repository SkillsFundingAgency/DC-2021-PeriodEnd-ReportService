﻿namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Model
{
    public interface IValue
    {
        int DeliveryPeriod { get; }

        decimal Value { get; }
    }
}
