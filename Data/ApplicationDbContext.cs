using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyResource.Models;
using System.Diagnostics.SymbolStore;

namespace StudyResource.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        #region DbSet
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<DownloadHistory> DownloadHistories { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region FluentAPI

            // Favorite (Pivot Table: User - Document) N-N Relationship
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.ToTable("Favorite").HasKey(f => f.Id);

                // Many-to-One relationship with User
                entity.HasOne(f => f.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Many-to-One relationship with Document
                entity.HasOne(f => f.Document)
                    .WithMany(d => d.Favorite)
                    .HasForeignKey(f => f.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DownloadHistory (Pivot Table: User-Document
            modelBuilder.Entity<DownloadHistory>(entity =>
            {
                entity.ToTable("DownloadHistory").HasKey(dh => dh.Id);

                // Many-to-One relationship with User
                entity.HasOne(dh => dh.User)
                    .WithMany(u => u.DownloadHistories)
                    .HasForeignKey(dh => dh.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Many-to-One relationship with Document
                entity.HasOne(dh => dh.Document)
                    .WithMany(d => d.DownloadHistories)
                    .HasForeignKey(dh => dh.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Document
            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Document").HasKey(d => d.Id);

            });

            // DocumentType
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.ToTable("DocumentType").HasKey(dt => dt.Id);

                // One-to-Many relationship with Document
                entity.HasMany(dt => dt.Documents)
                    .WithOne(d => d.DocumentType)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Subject
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("Subject").HasKey(s => s.Id);

                // One-to-Many relationship with Document
                entity.HasMany(s => s.Documents)
                    .WithOne(d => d.Subject)
                    .HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Grade
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grade").HasKey(g => g.Id);

                // One-to-Many relationship with Document
                entity.HasMany(s => s.Documents)
                    .WithOne(g => g.Grade)
                    .HasForeignKey(d => d.GradeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion
        }
    }
}