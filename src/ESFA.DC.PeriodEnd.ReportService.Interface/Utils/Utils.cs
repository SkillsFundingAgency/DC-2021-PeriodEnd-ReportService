namespace ESFA.DC.PeriodEnd.ReportService.Interface.Utils
{
    public static class Utils
    {
        /// <summary>
        /// Returns the corresponding FundingStreapPeriod code for the given FundingLineType.
        /// </summary>
        /// <returns>string.</returns>
        public static string GetFundingStreamPeriodForFundingLineType(string fundingLineType)
        {
            string FundingStreamPeriod = string.Empty;

            if (!string.IsNullOrEmpty(fundingLineType))
            {
                // BR3 table map for ReportingAimFundingLineType/FundingStreamPeriod
                switch (fundingLineType.ToUpper().Replace(" ", string.Empty))
                {
                    case "16-18APPRENTICESHIP(FROMMAY2017)LEVYCONTRACT":
                        FundingStreamPeriod = "LEVY1799";
                        break;

                    case "16-18APPRENTICESHIP(EMPLOYERONAPPSERVICE)LEVYFUNDING":
                        FundingStreamPeriod = "LEVY1799";
                        break;

                    case "19+APPRENTICESHIP(FROMMAY2017)LEVYCONTRACT":
                        FundingStreamPeriod = "LEVY1799";
                        break;

                    case "19+APPRENTICESHIP(EMPLOYERONAPPSERVICE)LEVYFUNDING":
                        FundingStreamPeriod = "LEVY1799";
                        break;

                    case "16-18APPRENTICESHIP(FROMMAY2017)NON-LEVYCONTRACT":
                        FundingStreamPeriod = "APPS1920";
                        break;

                    case "16-18APPRENTICESHIP(FROMMAY2017)NON-LEVYCONTRACT(NON-PROCURED)":
                        FundingStreamPeriod = "APPS1920";
                        break;

                    case "16-18APPRENTICESHIPNON-LEVYCONTRACT(PROCURED)":
                        FundingStreamPeriod = "16-18NLAP2018";
                        break;

                    case "16-18APPRENTICESHIP(EMPLOYERONAPPSERVICE)NON-LEVYFUNDING":
                        FundingStreamPeriod = "NONLEVY2019";
                        break;

                    case "19+APPRENTICESHIP(FROMMAY2017)NON-LEVYCONTRACT":
                        FundingStreamPeriod = "APPS1920";
                        break;

                    case "19+APPRENTICESHIP(FROMMAY2017)NON-LEVYCONTRACT(NON-PROCURED)":
                        FundingStreamPeriod = "APPS1920";
                        break;

                    case "19+APPRENTICESHIPNON-LEVYCONTRACT(PROCURED)":
                        FundingStreamPeriod = "ANLAP2018";
                        break;

                    case "19+APPRENTICESHIP(EMPLOYERONAPPSERVICE)NON-LEVYFUNDING":
                        FundingStreamPeriod = "NONLEVY2019";
                        break;
                }
            }

            return FundingStreamPeriod;
        }
    }
}
