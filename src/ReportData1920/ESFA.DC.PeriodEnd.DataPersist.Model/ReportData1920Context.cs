using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.PeriodEnd.DataPersist.Model
{
    public partial class ReportData1920Context : DbContext
    {
        public ReportData1920Context()
        {
        }

        public ReportData1920Context(DbContextOptions<ReportData1920Context> options)
            : base(options)
        {
        }

        public virtual DbSet<McaGlaDevelovedOccupancyReportV2> McaGlaDevelovedOccupancyReportV2s { get; set; }

        // Unable to generate entity type for table 'dbo.AppsAdditionalPayments'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.AppsCoInvestmentContribution'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.AppsMonthlyPayment'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.FundingSummaryReport'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\;Database=ESFA.DC.PeriodEnd.DataPerist.Database;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<McaGlaDevelovedOccupancyReportV2>(entity =>
            {
                entity.HasKey(e => new { e.Year, e.Return, e.AcMnth, e.Ukprn, e.LearnerReferenceNumber, e.LearningAimReference, e.AimSeqNumber, e.Id })
                    .HasName("PK_MCAGLA")
                    .ForSqlServerIsClustered(false);

                entity.ToTable("McaGlaDevelovedOccupancyReportV2");

                entity.Property(e => e.Year)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Ukprn).HasColumnName("UKPRN");

                entity.Property(e => e.LearnerReferenceNumber)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.LearningAimReference)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.AchievementElement).HasColumnType("decimal(10, 5)");

                entity.Property(e => e.AchievementPercentage).HasColumnType("decimal(10, 5)");

                entity.Property(e => e.AimValue).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ApplicableAreaFromSourceOfFunding)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.ApplicableFundingRate).HasColumnType("decimal(10, 5)");

                entity.Property(e => e.ApplicableFundingRateFromEsolhours)
                    .HasColumnName("ApplicableFundingRateFromESOLHours")
                    .HasColumnType("decimal(18, 5)");

                entity.Property(e => e.ApplicableProgrammeWeighting)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.AprilAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AprilBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AprilJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AprilLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AprilOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AreaUplift).HasColumnType("decimal(10, 5)");

                entity.Property(e => e.AugustAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AugustBalancingPaymentEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AugustJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AugustLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.AugustOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.CampusIdentifier)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.CappingFactor).HasColumnType("decimal(10, 5)");

                entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

                entity.Property(e => e.DateOfBirth).HasColumnType("date");

                entity.Property(e => e.DateUsedForUpliftsAndOtherLookups).HasColumnType("date");

                entity.Property(e => e.DecemberAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.DecemberBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.DecemberJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.DecemberLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.DecemberOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.DeliveryLocationPostcode)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.DisadvantageUplift).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.EntitlementCategoryLevel2or3)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EsmtypeBenefitStatusIndicator)
                    .HasColumnName("ESMTypeBenefitStatusIndicator")
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.FebruaryAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.FebruaryBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.FebruaryJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.FebruaryLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.FebruaryOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.FundingLineType)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.JanuaryAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JanuaryBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JanuaryJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JanuaryLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JanuaryOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JulyAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JulyBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JulyJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JulyLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JulyOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JuneAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JuneBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JuneJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JuneLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.JuneOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.LdfamtypeCommunityLearningProvisionType)
                    .HasColumnName("LDFAMTypeCommunityLearningProvisionType")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeDama)
                    .HasColumnName("LDFAMTypeDAMA")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeDamb)
                    .HasColumnName("LDFAMTypeDAMB")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeDamc)
                    .HasColumnName("LDFAMTypeDAMC")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeDamd)
                    .HasColumnName("LDFAMTypeDAMD")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeFullOrCoFundingIndicator)
                    .HasColumnName("LDFAMTypeFullOrCoFundingIndicator")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLdma)
                    .HasColumnName("LDFAMTypeLDMA")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLdmb)
                    .HasColumnName("LDFAMTypeLDMB")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLdmc)
                    .HasColumnName("LDFAMTypeLDMC")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLdmd)
                    .HasColumnName("LDFAMTypeLDMD")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLdme)
                    .HasColumnName("LDFAMTypeLDME")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLdmf)
                    .HasColumnName("LDFAMTypeLDMF")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLearningSupportFundingHighestApplicable)
                    .HasColumnName("LDFAMTypeLearningSupportFundingHighestApplicable")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLsfdateAppliesFromEarliest)
                    .HasColumnName("LDFAMTypeLSFDateAppliesFromEarliest")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeLsfdateAppliesToLatest)
                    .HasColumnName("LDFAMTypeLSFDateAppliesToLatest")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeRestartIndicator)
                    .HasColumnName("LDFAMTypeRestartIndicator")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LdfamtypeSourceOfFunding)
                    .HasColumnName("LDFAMTypeSourceOfFunding")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.LearningActualEndDate).HasColumnType("date");

                entity.Property(e => e.LearningAimTitle)
                    .HasMaxLength(254)
                    .IsUnicode(false);

                entity.Property(e => e.LearningPlannedEndDate).HasColumnType("date");

                entity.Property(e => e.LearningStartDate).HasColumnType("date");

                entity.Property(e => e.LearningStartDatePostcode)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.LlddandHealthProblem).HasColumnName("LLDDandHealthProblem");

                entity.Property(e => e.MarchAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MarchBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MarchJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MarchLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MarchOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MayAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MayBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MayJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MayLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.MayOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.NonPublicFundedContribution).HasColumnType("decimal(10, 5)");

                entity.Property(e => e.NotionalNvqlevel)
                    .HasColumnName("NotionalNVQLevel")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.NovemberAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.NovemberBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.NovemberJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.NovemberLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.NovemberOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.OctoberAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.OctoberBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.OctoberJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.OctoberLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.OctoberOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.Officialsensitive)
                    .HasColumnName("OFFICIALSENSITIVE")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.OriginalLearningStartDate).HasColumnType("date");

                entity.Property(e => e.PartnerUkprn).HasColumnName("PartnerUKPRN");

                entity.Property(e => e.PostcodePriorToEnrolment)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.PreMergerUkprn).HasColumnName("PreMergerUKPRN");

                entity.Property(e => e.ProviderSpecifiedDeliveryMonitoringA)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProviderSpecifiedDeliveryMonitoringB)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProviderSpecifiedDeliveryMonitoringC)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProviderSpecifiedDeliveryMonitoringD)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProviderSpecifiedLearnerMonitoringA)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProviderSpecifiedLearnerMonitoringB)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SeptemberAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.SeptemberBalancingPaymentEarnedcash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.SeptemberJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.SeptemberLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.SeptemberOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.Sex)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.SoftwareSupplierAimIdentifier)
                    .HasMaxLength(36)
                    .IsUnicode(false);

                entity.Property(e => e.Tier2SectorSubjectArea).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TotalAimAchievementEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.TotalBalancingPaymentEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.TotalEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.TotalJobOutcomeEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.TotalLearningSupportEarnedCash).HasColumnType("decimal(15, 5)");

                entity.Property(e => e.TotalOnProgrammeEarnedCash).HasColumnType("decimal(15, 5)");
            });
        }
    }
}
