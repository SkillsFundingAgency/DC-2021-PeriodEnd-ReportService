namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Provider.Model
{
    public class PeriodisedValuesFlattened
    {
        public string AttributeName { get; set; }

        public string FundLine { get; set; }

        public decimal?[] Periods { get; set; }
    }
}
