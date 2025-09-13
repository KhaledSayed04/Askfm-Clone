using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Askfm_Clone.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<CoinsTransaction> CoinsTransactions { get; set; }
        public DbSet<RefreshTokenInfo> RefreshTokensInfo { get; set; }
        public DbSet<QuestionRecipient> QuestionRecipients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Entity Configuration ---
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Name).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Coins).HasDefaultValue(0);

                // A user can send many questions.
                entity.HasMany(u => u.QuestionsSent)
                      .WithOne(q => q.Sender)
                      .HasForeignKey(q => q.SenderId)
                      .OnDelete(DeleteBehavior.Restrict); // A user's questions remain if their account is deleted.

                // A user can receive many questions via the QuestionRecipient table.
                entity.HasMany(u => u.QuestionsReceived)
                      .WithOne(qr => qr.Receptor)
                      .HasForeignKey(qr => qr.ReceptorId)
                      .OnDelete(DeleteBehavior.Restrict); // Keep received questions if user is deleted.

                // Other user relationships...
                entity.HasMany(u => u.Answers).WithOne(a => a.Creator).HasForeignKey(a => a.CreatorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.Comments).WithOne(c => c.Creator).HasForeignKey(c => c.CreatorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.Likes).WithOne(l => l.User).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(u => u.CoinsTransactions).WithOne(ct => ct.Receiver).HasForeignKey(ct => ct.ReceiverId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // --- Question Entity Configuration ---
            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Questions");
                entity.Property(q => q.Content).IsRequired();
                entity.Property(q => q.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // --- QuestionRecipient (Join Table) Configuration ---
            modelBuilder.Entity<QuestionRecipient>(entity =>
            {
                entity.ToTable("QuestionRecipients");
                entity.HasKey(qr => new { qr.QuestionId, qr.ReceptorId }); // Composite primary key
                entity.HasIndex(qr => qr.ReceptorId);
            });

            // --- Answer Entity Configuration ---
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.ToTable("Answers");
                entity.Property(a => a.Content).IsRequired();
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(a => a.QuestionRecipient)
                      .WithOne(qr => qr.Answer)
                      .HasForeignKey<Answer>(a => new { a.QuestionId, a.ReceptorId })
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Answer>()
                        .HasIndex(a => new { a.QuestionId, a.ReceptorId })
                        .IsUnique();

            // --- Comment Entity Configuration ---
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(c => c.Answer).WithMany(a => a.Comments).HasForeignKey(c => c.AnswerId);
                // Note: The FK to user is already defined in the AppUser configuration.
                entity.HasIndex(c => new { c.AnswerId, c.CreatedAt });
            });

            // --- Other Configurations (Likes, Follows, Blocks) ---
            modelBuilder.Entity<Like>(entity =>
            {
                entity.ToTable("Likes");
                entity.HasKey(l => new { l.UserId, l.AnswerId });
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(l => l.Answer).WithMany(a => a.Likes).HasForeignKey(l => l.AnswerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(l => new { l.AnswerId, l.CreatedAt });
            });

            modelBuilder.Entity<Follow>(entity =>
            {
                entity.ToTable("Follows");
                entity.HasKey(f => new { f.FollowerId, f.FolloweeId });
                entity.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(f => f.Follower).WithMany(u => u.Following).HasForeignKey(f => f.FollowerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.Followee).WithMany(u => u.Followers).HasForeignKey(f => f.FolloweeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Block>(entity =>
            {
                entity.ToTable("Blocks");
                entity.HasKey(b => new { b.BlockerId, b.BlockedId });
                entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(b => b.Blocker).WithMany(u => u.BlocksMade).HasForeignKey(b => b.BlockerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(b => b.Blocked).WithMany(u => u.BlocksReceived).HasForeignKey(b => b.BlockedId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}