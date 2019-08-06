using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Utils
{
    public static class Utils
    {
        /// <summary>
        /// Returns the corresponding FundingStreapPeriod code for the given FundingLineType
        /// </summary>
        /// <param name="fundingLineType"></param>
        /// <returns>FltFspMap</returns>
        public static FundingLineTypeToFundingStreamPeriodMap GetFundingStreamPeriodForFundingLineType(string fundingLineType)
        {
            FundingLineTypeToFundingStreamPeriodMap fltFspMap = new FundingLineTypeToFundingStreamPeriodMap();

            fltFspMap.FundingLineType = fundingLineType;
            fltFspMap.FundingStreamPeriod = string.Empty;

            // BR3 table map for ReportingAimFundingLineType/FundingStreamPeriod
            switch (fundingLineType.ToUpper())
            {
                case "16 - 18 APPRENTICESHIP(FROM MAY 2017) LEVY CONTRACT":
                    fltFspMap.FundingLineType = "LEVY1799";
                    break;

                case "16 - 18 APPRENTICESHIP(EMPLOYER ON APP SERVICE) LEVY FUNDING":
                    fltFspMap.FundingLineType = "LEVY1799";
                    break;

                case "19 + APPRENTICESHIP(FROM MAY 2017) LEVY CONTRACT":
                    fltFspMap.FundingLineType = "LEVY1799";
                    break;

                case "19 + APPRENTICESHIP(EMPLOYER ON APP SERVICE) LEVY FUNDING":
                    fltFspMap.FundingLineType = "LEVY1799";
                    break;

                case "16 - 18 APPRENTICESHIP(FROM MAY 2017) NON - LEVY CONTRACT":
                    fltFspMap.FundingLineType = "APPS1920";
                    break;

                case "16 - 18 APPRENTICESHIP(FROM MAY 2017) NON - LEVY CONTRACT(NON - PROCURED)":
                    fltFspMap.FundingLineType = "APPS1920";
                    break;

                case "16 - 18 APPRENTICESHIP NON-LEVY CONTRACT(PROCURED)":
                    fltFspMap.FundingLineType = "16 - 18NLAP2018";
                    break;

                case "16 - 18 APPRENTICESHIP(EMPLOYER ON APP SERVICE) NON - LEVY FUNDING":
                    fltFspMap.FundingLineType = "NONLEVY2019";
                    break;

                case "19 + APPRENTICESHIP(FROM MAY 2017) NON - LEVY CONTRACT":
                    fltFspMap.FundingLineType = "APPS1920";
                    break;

                case "19 + APPRENTICESHIP(FROM MAY 2017) NON - LEVY CONTRACT(NON - PROCURED)":
                    fltFspMap.FundingLineType = "APPS1920";
                    break;

                case "19 + APPRENTICESHIP NON - LEVY CONTRACT(PROCURED)":
                    fltFspMap.FundingLineType = "ANLAP2018";
                    break;

                case "19 + APPRENTICESHIP(EMPLOYER ON APP SERVICE) NON - LEVY FUNDING":
                    fltFspMap.FundingLineType = "NONLEVY2019";
                    break;
            }

            return fltFspMap;
        }
    }
}
