using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IStringUtilitiesService
    {
        string JoinWithMaxLength(IEnumerable<string> list);

        DateTime? GetIlrFileDate(string ilrFilename);

        int TryGetInt(string value, int def);

        string GetDateTimeAsString(DateTime? dateTime, string def, DateTime? replace = null);

        string[] GetArrayEntries(IEnumerable<ILearningDeliveryFAM> availableValues, int size);
    }
}
