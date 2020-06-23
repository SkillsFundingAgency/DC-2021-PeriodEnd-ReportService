using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentsClassMapTests
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
                "Provider specified learner monitoring (A)",
                "Provider specified learner monitoring (B)",
                "Learning start date",
                "Funding line type",
                "Type of additional payment",
                "Employer name from apprenticeship service",
                "Employer identifier from ILR",

                "August earnings",
                "August (R01) payments",
                "September earnings",
                "September (R02) payments",
                "October earnings",
                "October (R03) payments",
                "November earnings",
                "November (R04) payments",
                "December earnings",
                "December (R05) payments",
                "January earnings",
                "January (R06) payments",
                "February earnings",
                "February (R07) payments",
                "March earnings",
                "March (R08) payments",
                "April earnings",
                "April (R09) payments",
                "May earnings",
                "May (R10) payments",
                "June earnings",
                "June (R11) payments",
                "July earnings",
                "July (R12) payments",
                "R13 payments",
                "R14 payments",
                "Total earnings",
                "Total payments (year to date)",
                "OFFICIAL - SENSITIVE",
            };

            var input = new List<AppsAdditionalPaymentRecord>()
            {
                new AppsAdditionalPaymentRecord()
            };

            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 8096, true))
                {
                    using (var csvWriter = new CsvWriter(streamWriter))
                    {
                        csvWriter.Configuration.RegisterClassMap<AppsAdditionalPaymentsClassMap>();

                        csvWriter.WriteRecords(input);
                    }
                }

                stream.Position = 0;

                using (var streamReader = new StreamReader(stream))
                {
                    using (var csvreader = new CsvReader(streamReader))
                    {
                        csvreader.Read();
                        csvreader.ReadHeader();
                        var header = csvreader.Context.HeaderRecord;

                        header.Should().ContainInOrder(orderedColumns);

                        header.Should().HaveCount(40);
                    }
                }
            }
        }

    }
}