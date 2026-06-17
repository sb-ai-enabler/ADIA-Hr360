using System;
using System.Collections.Generic;
using Hr360.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hr360.Infrastructure;

public partial class Hr360DbContext : DbContext
{
    public Hr360DbContext(DbContextOptions<Hr360DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditEvent> AuditEvents { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<FeedbackSubmission> FeedbackSubmissions { get; set; }

    public virtual DbSet<ReviewAssignment> ReviewAssignments { get; set; }

    public virtual DbSet<ReviewCycle> ReviewCycles { get; set; }

    public virtual DbSet<ReviewTemplate> ReviewTemplates { get; set; }

    public DbSet<ReviewTemplate> Templates => ReviewTemplates;
    public DbSet<ReviewCycle> Cycles => ReviewCycles;
    public DbSet<ReviewAssignment> Assignments => ReviewAssignments;
    public DbSet<FeedbackSubmission> Submissions => FeedbackSubmissions;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Action).HasMaxLength(120);
            entity.Property(e => e.Actor).HasMaxLength(200);
            entity.Property(e => e.EntityType).HasMaxLength(120);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email, "UX_Employees_Email").IsUnique();

            entity.HasIndex(e => e.EntraObjectId, "UX_Employees_EntraObjectId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Department).HasMaxLength(120);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.EntraObjectId).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Employees_IsActive");
        });

        modelBuilder.Entity<FeedbackSubmission>(entity =>
        {
            entity.HasIndex(e => e.IdempotencyKey, "UX_FeedbackSubmissions_IdempotencyKey")
                .IsUnique()
                .HasFilter("([IdempotencyKey]<>'')");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ClientDraftId).HasMaxLength(120);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(120);

            entity.HasOne(d => d.Assignment).WithMany(p => p.FeedbackSubmissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ReviewAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.CycleId, e.RevieweeId, e.ReviewerId }, "UX_ReviewAssignments_Cycle_Reviewee_Reviewer").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Cycle).WithMany(p => p.ReviewAssignments).HasForeignKey(d => d.CycleId);

            entity.HasOne(d => d.Reviewee).WithMany(p => p.ReviewAssignmentReviewees)
                .HasForeignKey(d => d.RevieweeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewAssignmentReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ReviewCycle>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(160);

            entity.HasOne(d => d.Template).WithMany(p => p.ReviewCycles)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ReviewTemplate>(entity =>
        {
            entity.HasIndex(e => new { e.Name, e.Version }, "UX_ReviewTemplates_Name_Version").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(160);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
