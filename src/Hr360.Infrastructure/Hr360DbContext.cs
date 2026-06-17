using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure;

public sealed class Hr360DbContext(DbContextOptions<Hr360DbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ReviewTemplate> Templates => Set<ReviewTemplate>();
    public DbSet<ReviewCycle> Cycles => Set<ReviewCycle>();
    public DbSet<ReviewAssignment> Assignments => Set<ReviewAssignment>();
    public DbSet<FeedbackSubmission> Submissions => Set<FeedbackSubmission>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasIndex(e => e.EntraObjectId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Department).HasMaxLength(120);
        });

        modelBuilder.Entity<ReviewTemplate>(entity =>
        {
            entity.ToTable("ReviewTemplates");
            entity.Property(t => t.Name).HasMaxLength(160);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.DefinitionJson).HasColumnType("nvarchar(max)");
            entity.HasIndex(t => new { t.Name, t.Version }).IsUnique();
        });

        modelBuilder.Entity<ReviewCycle>(entity =>
        {
            entity.ToTable("ReviewCycles");
            entity.Property(c => c.Name).HasMaxLength(160);
            entity.Property(c => c.TemplateSnapshotJson).HasColumnType("nvarchar(max)");
            entity.HasMany(c => c.Assignments)
                .WithOne(a => a.Cycle)
                .HasForeignKey(a => a.CycleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewAssignment>(entity =>
        {
            entity.ToTable("ReviewAssignments");
            entity.HasIndex(a => new { a.CycleId, a.RevieweeId, a.ReviewerId }).IsUnique();
            entity.Property(a => a.RowVersion).IsRowVersion();
            entity.HasOne(a => a.Reviewee)
                .WithMany()
                .HasForeignKey(a => a.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Reviewer)
                .WithMany()
                .HasForeignKey(a => a.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeedbackSubmission>(entity =>
        {
            entity.ToTable("FeedbackSubmissions");
            entity.Property(s => s.AnswersJson).HasColumnType("nvarchar(max)");
            entity.Property(s => s.IdempotencyKey).HasMaxLength(120);
            entity.Property(s => s.ClientDraftId).HasMaxLength(120);
            entity.HasIndex(s => s.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] <> ''");
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("AuditEvents");
            entity.Property(a => a.Actor).HasMaxLength(200);
            entity.Property(a => a.Action).HasMaxLength(120);
            entity.Property(a => a.EntityType).HasMaxLength(120);
            entity.Property(a => a.MetadataJson).HasColumnType("nvarchar(max)");
        });
    }
}
