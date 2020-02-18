dotnet.exe ef dbcontext scaffold "Server=.\;Database=ESFA.DC.PeriodEnd.DataPerist.Database;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -c ReportData1920Context --schema dbo --force --startup-project . --project ..\ESFA.DC.PeriodEnd.DataPersist.Model --verbose

pause
