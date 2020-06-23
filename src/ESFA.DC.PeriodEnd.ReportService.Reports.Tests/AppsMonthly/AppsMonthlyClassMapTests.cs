using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly;
using ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsMonthly
{
    public class AppsMonthlyClassMapTests
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
                "Campus identifier",
                "Provider specified learner monitoring (A)",
                "Provider specified learner monitoring (B)",
                "Aim sequence number",
                "Learning aim reference",
                "Learning aim title",
                "Original learning start date",
                "Learning start date",
                "Learning planned end date",
                "Completion status",
                "Learning actual end date",
                "Achievement date",
                "Outcome",
                "Programme type",
                "Standard code",
                "Framework code",
                "Apprenticeship pathway",
                "Aim type",
                "Software supplier aim identifier",
                "Learning delivery funding and monitoring type - learning delivery monitoring (A)",
                "Learning delivery funding and monitoring type - learning delivery monitoring (B)",
                "Learning delivery funding and monitoring type - learning delivery monitoring (C)",
                "Learning delivery funding and monitoring type - learning delivery monitoring (D)",
                "Learning delivery funding and monitoring type - learning delivery monitoring (E)",
                "Learning delivery funding and monitoring type - learning delivery monitoring (F)",
                "Provider specified delivery monitoring (A)",
                "Provider specified delivery monitoring (B)",
                "Provider specified delivery monitoring (C)",
                "Provider specified delivery monitoring (D)",
                "End point assessment organisation",
                "Planned number of on programme instalments for aim",
                "Sub contracted or partnership UKPRN",
                "Price episode start date",
                "Price episode actual end date",
                "Contract no",
                "Funding line type",
                "Learning delivery funding and monitoring type - apprenticeship contract type",
                "Employer identifier on employment status date",
                "Employment status",
                "Employment status date",
                "August (R01) levy payments",
                "August (R01) co-investment payments",
                "August (R01) co-investment (below band upper limit) due from employer",
                "August (R01) employer additional payments",
                "August (R01) provider additional payments",
                "August (R01) apprentice additional payments",
                "August (R01) English and maths payments",
                "August (R01) payments for learning support, disadvantage and framework uplifts",
                "August (R01) total payments",
                "September (R02) levy payments",
                "September (R02) co-investment payments",
                "September (R02) co-investment (below band upper limit) due from employer",
                "September (R02) employer additional payments",
                "September (R02) provider additional payments",
                "September (R02) apprentice additional payments",
                "September (R02) English and maths payments",
                "September (R02) payments for learning support, disadvantage and framework uplifts",
                "September (R02) total payments",
                "October (R03) levy payments",
                "October (R03) co-investment payments",
                "October (R03) co-investment (below band upper limit) due from employer",
                "October (R03) employer additional payments",
                "October (R03) provider additional payments",
                "October (R03) apprentice additional payments",
                "October (R03) English and maths payments",
                "October (R03) payments for learning support, disadvantage and framework uplifts",
                "October (R03) total payments",
                "November (R04) levy payments",
                "November (R04) co-investment payments",
                "November (R04) co-investment (below band upper limit) due from employer",
                "November (R04) employer additional payments",
                "November (R04) provider additional payments",
                "November (R04) apprentice additional payments",
                "November (R04) English and maths payments",
                "November (R04) payments for learning support, disadvantage and framework uplifts",
                "November (R04) total payments",
                "December (R05) levy payments",
                "December (R05) co-investment payments",
                "December (R05) co-investment (below band upper limit) due from employer",
                "December (R05) employer additional payments",
                "December (R05) provider additional payments",
                "December (R05) apprentice additional payments",
                "December (R05) English and maths payments",
                "December (R05) payments for learning support, disadvantage and framework uplifts",
                "December (R05) total payments",
                "January (R06) levy payments",
                "January (R06) co-investment payments",
                "January (R06) co-investment (below band upper limit) due from employer",
                "January (R06) employer additional payments",
                "January (R06) provider additional payments",
                "January (R06) apprentice additional payments",
                "January (R06) English and maths payments",
                "January (R06) payments for learning support, disadvantage and framework uplifts",
                "January (R06) total payments",
                "February (R07) levy payments",
                "February (R07) co-investment payments",
                "February (R07) co-investment (below band upper limit) due from employer",
                "February (R07) employer additional payments",
                "February (R07) provider additional payments",
                "February (R07) apprentice additional payments",
                "February (R07) English and maths payments",
                "February (R07) payments for learning support, disadvantage and framework uplifts",
                "February (R07) total payments",
                "March (R08) levy payments",
                "March (R08) co-investment payments",
                "March (R08) co-investment (below band upper limit) due from employer",
                "March (R08) employer additional payments",
                "March (R08) provider additional payments",
                "March (R08) apprentice additional payments",
                "March (R08) English and maths payments",
                "March (R08) payments for learning support, disadvantage and framework uplifts",
                "March (R08) total payments",
                "April (R09) levy payments",
                "April (R09) co-investment payments",
                "April (R09) co-investment (below band upper limit) due from employer",
                "April (R09) employer additional payments",
                "April (R09) provider additional payments",
                "April (R09) apprentice additional payments",
                "April (R09) English and maths payments",
                "April (R09) payments for learning support, disadvantage and framework uplifts",
                "April (R09) total payments",
                "May (R10) levy payments",
                "May (R10) co-investment payments",
                "May (R10) co-investment (below band upper limit) due from employer",
                "May (R10) employer additional payments",
                "May (R10) provider additional payments",
                "May (R10) apprentice additional payments",
                "May (R10) English and maths payments",
                "May (R10) payments for learning support, disadvantage and framework uplifts",
                "May (R10) total payments",
                "June (R11) levy payments",
                "June (R11) co-investment payments",
                "June (R11) co-investment (below band upper limit) due from employer",
                "June (R11) employer additional payments",
                "June (R11) provider additional payments",
                "June (R11) apprentice additional payments",
                "June (R11) English and maths payments",
                "June (R11) payments for learning support, disadvantage and framework uplifts",
                "June (R11) total payments",
                "July (R12) levy payments",
                "July (R12) co-investment payments",
                "July (R12) co-investment (below band upper limit) due from employer",
                "July (R12) employer additional payments",
                "July (R12) provider additional payments",
                "July (R12) apprentice additional payments",
                "July (R12) English and maths payments",
                "July (R12) payments for learning support, disadvantage and framework uplifts",
                "July (R12) total payments",
                "R13 levy payments",
                "R13 co-investment payments",
                "R13 co-investment (below band upper limit) due from employer",
                "R13 employer additional payments",
                "R13 provider additional payments",
                "R13 apprentice additional payments",
                "R13 English and maths payments",
                "R13 payments for learning support, disadvantage and framework uplifts",
                "R13 total payments",
                "R14 levy payments",
                "R14 co-investment payments",
                "R14 co-investment (below band upper limit) due from employer",
                "R14 employer additional payments",
                "R14 provider additional payments",
                "R14 apprentice additional payments",
                "R14 English and maths payments",
                "R14 payments for learning support, disadvantage and framework uplifts",
                "R14 total payments",
                "Total levy payments",
                "Total co-investment payments",
                "Total co-investment (below band upper limit) due from employer",
                "Total employer additional payments",
                "Total provider additional payments",
                "Total apprentice additional payments",
                "Total English and maths payments",
                "Total payments for learning support, disadvantage and framework uplifts",
                "Total payments",
                "OFFICIAL - SENSITIVE",
            };

            var input = new List<AppsMonthlyRecord>()
            {
                new AppsMonthlyRecord()
            };

            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 8096, true))
                {
                    using (var csvWriter = new CsvWriter(streamWriter))
                    {
                        csvWriter.Configuration.RegisterClassMap<AppsMonthlyClassMap>();

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

                        header.Should().HaveCount(180);
                    }
                }
            }
        }
    }
}
