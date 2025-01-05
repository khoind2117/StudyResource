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
        public DbSet<Set> Sets { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<GradeSubject> GradeSubjects { get; set; }
        public DbSet<UserComment> UserComments { get; set; }
        public DbSet<Keyword> Keyword { get; set; }
        public DbSet<DocumentKeyword> DocumentKeywords { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Image> Images { get; set; }
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

            // DownloadHistory (Pivot Table: User-Document) N-N Relationship
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

                // Many-to-One relationship with User
                entity.HasOne(d => d.User)
                    .WithMany(u => u.Documents)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
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

            // Set
            modelBuilder.Entity<Set>(entity =>
            {
                entity.ToTable("Set").HasKey(s => s.Id);

                // One-to-Many relationship with Set
                entity.HasMany(s => s.Documents)
                    .WithOne(d => d.Set)
                    .HasForeignKey(s => s.SetId)
                    .OnDelete(DeleteBehavior.Restrict);
            });  

            // Subject
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("Subject").HasKey(s => s.Id);
            });

            // Grade
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grade").HasKey(g => g.Id);
            });

            // GradeSubject (Pivot Table: Grade-Subject) N-N Relationship
            modelBuilder.Entity<GradeSubject>(entity =>
            {
                entity.ToTable("GradeSubject").HasKey(gs => gs.Id);

                // Many-to-One relationship with Grade
                entity.HasOne(gs => gs.Grade)
                    .WithMany(g => g.GradeSubjects)
                    .HasForeignKey(gs => gs.GradeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Many-to-One relationship with Subject
                entity.HasOne(gs => gs.Subject)
                    .WithMany(s => s.GradeSubjects)
                    .HasForeignKey(gs => gs.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One-to-Many relationship with Document
                entity.HasMany(gs => gs.Documents)
                    .WithOne(d => d.GradeSubject)
                    .HasForeignKey(d => d.GradeSubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One-to-Many relationship with Video
                entity.HasMany(gs => gs.Videos)
                    .WithOne(v => v.GradeSubject)
                    .HasForeignKey(v => v.GradeSubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One-to-Many relationship with Image
                entity.HasMany(gs => gs.Images)
                    .WithOne(i => i.GradeSubject)
                    .HasForeignKey(i => i.GradeSubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration for UserComment
            modelBuilder.Entity<UserComment>() 
                .HasOne(uc => uc.Document) 
                .WithMany(d => d.UserComments) 
                .HasForeignKey(uc => uc.DocumentId); 

            modelBuilder.Entity<UserComment>() 
                .HasOne(uc => uc.User) 
                .WithMany(u => u.UserComments) 
                .HasForeignKey(uc => uc.UserId);

            // Keyword
            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.ToTable("Keyword").HasKey(k => k.Id);
            });

            // DocumentKeyword (Pivot Table: Document-Keyword) N-N Relationship
            modelBuilder.Entity<DocumentKeyword>(entity =>
            {
                entity.ToTable("DocumentKeyword").HasKey(dk => dk.Id);

                // Many-to-One relationship with Grade
                entity.HasOne(d => d.Document)
                    .WithMany(dk => dk.DocumentKeywords)
                    .HasForeignKey(dk => dk.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Many-to-One relationship with Subject
                entity.HasOne(k => k.Keyword)
                    .WithMany(dk => dk.DocumentKeywords)
                    .HasForeignKey(dk => dk.KeywordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Video
            modelBuilder.Entity<Video>(entity =>
            {
                entity.ToTable("Video").HasKey(v => v.Id);

                // Many-to-One relationship with User
                entity.HasOne(v => v.User)
                    .WithMany(u => u.Videos)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Image
            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("Image").HasKey(v => v.Id);

                // Many-to-One relationship with User
                entity.HasOne(v => v.User)
                    .WithMany(u => u.Images)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion
        }
    }
}