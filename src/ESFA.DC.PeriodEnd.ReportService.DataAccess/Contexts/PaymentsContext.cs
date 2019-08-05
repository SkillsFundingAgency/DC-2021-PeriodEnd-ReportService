using ESFA.DC.DASPayments.EF;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.ReportService.DataAccess.Contexts
{
    public class PaymentsContext : DASPaymentsContext
    {
        public PaymentsContext(DbContextOptions<DASPaymentsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PaymentMetricsEntity> PaymentMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaymentMetricsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).HasColumnName("Transaction Type");
                entity.Property(e => e.EarningsYTD).HasColumnName("Earnings YTD");
                entity.Property(e => e.EarningsACT1).HasColumnName("Earnings ACT1");
                entity.Property(e => e.EarningsACT2).HasColumnName("Earnings ACT2");
                entity.Property(e => e.NegativeEarnings).HasColumnName("Negative Earnings");
                entity.Property(e => e.NegativeEarningsACT1).HasColumnName("Negative Earnings ACT1");
                entity.Property(e => e.NegativeEarningsACT2).HasColumnName("Negative Earnings ACT2");
                entity.Property(e => e.PaymentsYTD).HasColumnName("Payments YTD");
                entity.Property(e => e.PaymentsACT1).HasColumnName("Payments ACT1");
                entity.Property(e => e.PaymentsACT2).HasColumnName("Payments ACT2");
                entity.Property(e => e.DataLockErrors).HasColumnName("Data Lock Errors");
                entity.Property(e => e.HeldBackCompletion).HasColumnName("Held Back Completion");
                entity.Property(e => e.HBCPACT1).HasColumnName("HBCP ACT1");
                entity.Property(e => e.HBCPACT2).HasColumnName("HBCP ACT2");
            });
        }
    }
}