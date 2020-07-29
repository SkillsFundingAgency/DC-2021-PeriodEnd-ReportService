﻿using ESFA.DC.PeriodEnd.ReportService.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ActCount.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.ActCount
{
    public class ActCountClassMap : AbstractClassMap<ActCountModel>
    {
        public ActCountClassMap()
        {
            MapIndex(m => m.Ukprn).Name("UKPRN");
            MapIndex(m => m.LearnersAct1).Name("ActCountOne");
            MapIndex(m => m.LearnersAct2).Name("ActCountTwo");
        }
    }
}
