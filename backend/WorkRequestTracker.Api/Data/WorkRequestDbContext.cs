
using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Data;

public class WorkRequestDbContext(
    DbContextOptions<WorkRequestDbContext> options
) : DbContext(options)
{
    public DbSet<WorkRequest> WorkRequests => Set<WorkRequest>();
    public DbSet<WorkRequestNote> WorkRequestNotes => Set<WorkRequestNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var request = modelBuilder.Entity<WorkRequest>();

        request.ToTable("WORK_REQUESTS");
        request.HasKey(x => x.Id);

        request.Property(x => x.Id).HasColumnName("ID");
        request.Property(x => x.Title)
            .HasColumnName("TITLE").HasMaxLength(150).IsRequired();
        request.Property(x => x.ClientName)
            .HasColumnName("CLIENT_NAME").HasMaxLength(150).IsRequired();
        request.Property(x => x.Description)
            .HasColumnName("DESCRIPTION").HasMaxLength(2000);
        request.Property(x => x.Priority)
            .HasColumnName("PRIORITY").HasMaxLength(20).IsRequired();
        request.Property(x => x.Status)
            .HasColumnName("STATUS").HasMaxLength(20).IsRequired();
        request.Property(x => x.DueDate).HasColumnName("DUE_DATE");
        request.Property(x => x.CreatedDate).HasColumnName("CREATED_DATE");
        request.Property(x => x.UpdatedDate).HasColumnName("UPDATED_DATE");

        request.HasIndex(x => x.Status)
            .HasDatabaseName("IX_WR_STATUS_CREATED");
        request.HasIndex(x => x.ClientName)
            .HasDatabaseName("IX_WR_CLIENT_NAME");

        request.HasMany(x => x.Notes)
            .WithOne(x => x.WorkRequest)
            .HasForeignKey(x => x.WorkRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        var note = modelBuilder.Entity<WorkRequestNote>();

        note.ToTable("WORK_REQUEST_NOTES");
        note.HasKey(x => x.Id);

        note.Property(x => x.Id).HasColumnName("ID");
        note.Property(x => x.WorkRequestId)
            .HasColumnName("WORK_REQUEST_ID");
        note.Property(x => x.Text)
            .HasColumnName("NOTE_TEXT")
            .HasMaxLength(1000).IsRequired();
        note.Property(x => x.CreatedDate)
            .HasColumnName("CREATED_DATE");

        note.HasIndex(x => x.WorkRequestId)
            .HasDatabaseName("IX_WR_NOTES_REQUEST");
    }
}
