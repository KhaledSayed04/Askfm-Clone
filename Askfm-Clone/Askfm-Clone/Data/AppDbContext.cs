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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --------------------
            // User
            // --------------------
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.Property(u => u.Coins)
                      .HasDefaultValue(0);

                entity.HasMany(u => u.QuestionsSent)
                      .WithOne(q => q.Sender)
                      .HasForeignKey(q => q.FromUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.QuestionsReceived)
                      .WithOne(q => q.Receiver)
                      .HasForeignKey(q => q.ToUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Answers)
                      .WithOne(a => a.Creator)
                      .HasForeignKey(a => a.CreatorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.CoinsTransactions)
                      .WithOne(ct => ct.Receiver)
                      .HasForeignKey(ct => ct.ReceiverId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RefreshTokens)
                      .WithOne(rt => rt.User)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --------------------
            // Question
            // --------------------
            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Questions");

                entity.Property(q => q.Content)
                      .IsRequired();

                entity.Property(q => q.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // --------------------
            // Answer
            // --------------------
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.ToTable("Answers");

                entity.Property(a => a.Content)
                      .IsRequired();

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // --------------------
            // CoinsTransaction
            // --------------------
            modelBuilder.Entity<CoinsTransaction>(entity =>
            {
                entity.ToTable("CoinsTransactions");

                entity.Property(ct => ct.Amount)
                      .IsRequired();

                entity.Property(ct => ct.Type)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(ct => ct.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });
            // --------------------
            // Comment
            // --------------------
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");

                entity.Property(c => c.Content)
                      .IsRequired();

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(c => c.Answer)
                      .WithMany(a => a.Comments)
                      .HasForeignKey(c => c.AnswerId);

                entity.HasOne(c => c.Creator)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.FromUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --------------------
            // Like
            // --------------------
            modelBuilder.Entity<Like>(entity =>
            {
                entity.ToTable("Likes");

                entity.HasKey(l => new { l.UserId, l.AnswerId }); // composite key

                entity.HasOne(l => l.Answer)
                      .WithMany(a => a.Likes)
                      .HasForeignKey(l => l.AnswerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // 👈 prevent cascade loop;
            });

            // --------------------
            // Follow
            // --------------------
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.ToTable("Follows");

                entity.HasKey(f => new { f.FollowerId, f.FolloweeId });

                entity.HasOne(f => f.Follower)
                      .WithMany(u => u.Following)
                      .HasForeignKey(f => f.FollowerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Followee)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(f => f.FolloweeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --------------------
            // Block
            // --------------------
            modelBuilder.Entity<Block>(entity =>
            {
                entity.ToTable("Blocks");

                entity.HasKey(b => new { b.BlockerId, b.BlockedId });

                entity.HasOne(b => b.Blocker)
                      .WithMany(u => u.BlocksMade)
                      .HasForeignKey(b => b.BlockerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Blocked)
                      .WithMany(u => u.BlocksReceived)
                      .HasForeignKey(b => b.BlockedId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}