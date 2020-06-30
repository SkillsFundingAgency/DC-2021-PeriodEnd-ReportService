using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment
{
    public class AppsCoInvestmentClassMapTests
    {
        [Fact]
        public void HeaderTest()
        {
            var orderedColumns = new string[]
            {
                "Learner reference number",
                "Unique learner number",
                "Family name",
                "Given names",
                "Learning start date",
                "Programme type",
                "Standard code",
                "Framework code",
                "Apprenticeship pathway",
                "Software supplier aim identifier",
                "Learning delivery funding and monitoring type - apprenticeship contract type",
                "Employer identifier (ERN) at start of learning",
                "Employer name from apprenticeship service",
                "Employer co-investment percentage",
                "Applicable programme start date",
                "Total employer contribution collected (PMR) in previous funding years",
                "Total co-investment (below band upper limit) due from employer in previous funding years",
                "Total employer contribution collected (PMR) in this funding year",
                "Total co-investment (below band upper limit) due from employer in this funding year",
                "Percentage of co-investment collected (for all funding years)",
                "LDM 356 or 361?",
                "Completion earnings in this funding year",
                "Completion payments in this funding year (including employer co-investment)",
                "Co-investment (below band upper limit) due from employer for August (R01)",
                "Co-investment (below band upper limit) due from employer for September (R02)",
                "Co-investment (below band upper limit) due from employer for October (R03)",
                "Co-investment (below band upper limit) due from employer for November (R04)",
                "Co-investment (below band upper limit) due from employer for December (R05)",
                "Co-investment (below band upper limit) due from employer for January (R06)",
                "Co-investment (below band upper limit) due from employer for February (R07)",
                "Co-investment (below band upper limit) due from employer for March (R08)",
                "Co-investment (below band upper limit) due from employer for April (R09)",
                "Co-investment (below band upper limit) due from employer for May (R10)",
                "Co-investment (below band upper limit) due from employer for June (R11)",
                "Co-investment (below band upper limit) due from employer for July (R12)",
                "Co-investment (below band upper limit) due from employer for R13",
                "Co-investment (below band upper limit) due from employer for R14",
                "OFFICIAL - SENSITIVE",
            };

            var input = new List<AppsCoInvestmentRecord>()
            {
                new AppsCoInvestmentRecord()
            };

            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 8096, true))
                {
                    using (var csvWriter = new CsvWriter(streamWriter))
                    {
                        csvWriter.Configuration.RegisterClassMap<AppsCoInvestmentClassMap>();

                        csvWriter.WriteRecords(input);
                    }
                }

                stream.Position = 0;

                using (var streamReader = new StreamReader(stream))
                {
                    using (var csvReader = new CsvReader(streamReader))
                    {
                        csvReader.Read();
                        csvReader.ReadHeader();
                        var header = csvReader.Context.HeaderRecord;

                        header.Should().ContainInOrder(orderedColumns);

                        header.Should().HaveCount(38);
                    }
                }
            }
        }
    }
}
