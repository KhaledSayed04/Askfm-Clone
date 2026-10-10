using Askfm_Clone.Data;
using Askfm_Clone.Services.Implementation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Tests;

public class BlockServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly BlockService _sut;

    public BlockServiceTests()
    {
        // SQLite in-memory: the DB lives as long as the connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Telling SQLite how to execute the SQL Server specific function.
        _connection.CreateFunction("GETUTCDATE", () => DateTime.UtcNow);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _sut = new BlockService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<(AppUser alice, AppUser bob)> SeedTwoUsersAsync()
    {
        var alice = new AppUser { Name = "Alice", Email = "alice@x.com", PasswordHash = "h" };
        var bob = new AppUser { Name = "Bob", Email = "bob@x.com", PasswordHash = "h" };
        _context.Users.AddRange(alice, bob);
        await _context.SaveChangesAsync();
        return (alice, bob);
    }

    [Fact]
    public async Task BlockAsync_CreatesBlockRow_WhenBothUsersExist()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        var result = await _sut.BlockAsync(alice.Id, bob.Id);

        Assert.True(result);
        Assert.True(await _context.Blocks.AnyAsync(b =>
            b.BlockerId == alice.Id && b.BlockedId == bob.Id));
    }

    [Fact]
    public async Task BlockAsync_ReturnsFalse_WhenTargetUserDoesNotExist()
    {
        var (alice, _) = await SeedTwoUsersAsync();

        var result = await _sut.BlockAsync(alice.Id, blockedId: 9999);

        Assert.False(result);
        Assert.False(await _context.Blocks.AnyAsync());
    }

    [Fact]
    public async Task BlockAsync_IsIdempotent_WhenBlockAlreadyExists()
    {
        var (alice, bob) = await SeedTwoUsersAsync();
        await _sut.BlockAsync(alice.Id, bob.Id);

        var result = await _sut.BlockAsync(alice.Id, bob.Id);

        Assert.True(result);
        Assert.Equal(1, await _context.Blocks.CountAsync());
    }

    [Fact]
    public async Task BlockAsync_PurgesFollows_InBothDirections()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        // Seed reciprocal follows
        _context.Follows.AddRange(
            new Follow { FollowerId = alice.Id, FolloweeId = bob.Id },
            new Follow { FollowerId = bob.Id, FolloweeId = alice.Id }
        );
        await _context.SaveChangesAsync();

        await _sut.BlockAsync(alice.Id, bob.Id);

        // Assert both follow records were deleted by the transaction
        Assert.Empty(await _context.Follows.ToListAsync());
    }

    [Fact]
    public async Task BlockAsync_PurgesLikes_OnBothUsersAnswers()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        var question = new Question { SenderId = alice.Id, Content = "Base Question" };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var qr1 = new QuestionRecipient { QuestionId = question.Id, ReceptorId = alice.Id };
        var qr2 = new QuestionRecipient { QuestionId = question.Id, ReceptorId = bob.Id };
        _context.QuestionRecipients.AddRange(qr1, qr2);
        await _context.SaveChangesAsync();

        var aliceAnswer = new Answer { QuestionId = question.Id, ReceptorId = alice.Id, CreatorId = alice.Id, Content = "Alice A" };
        var bobAnswer = new Answer { QuestionId = question.Id, ReceptorId = bob.Id, CreatorId = bob.Id, Content = "Bob A" };
        _context.Answers.AddRange(aliceAnswer, bobAnswer);
        await _context.SaveChangesAsync();

        // Bob likes Alice's answer; Alice likes Bob's answer
        _context.Likes.AddRange(
            new Like { UserId = bob.Id, AnswerId = aliceAnswer.Id },
            new Like { UserId = alice.Id, AnswerId = bobAnswer.Id }
        );
        await _context.SaveChangesAsync();

        // Act
        await _sut.BlockAsync(alice.Id, bob.Id);

        // Assert all likes were purged in both directions
        Assert.Empty(await _context.Likes.ToListAsync());
    }

    [Fact]
    public async Task UnblockAsync_RemovesBlockRow_WhenBlockExists()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        _context.Blocks.Add(new Block { BlockerId = alice.Id, BlockedId = bob.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.UnblockAsync(alice.Id, bob.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(await _context.Blocks.ToListAsync());
    }

    [Fact]
    public async Task UnblockAsync_ReturnsFalse_WhenNoBlockExists()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        var result = await _sut.UnblockAsync(alice.Id, bob.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task BlockAsync_PurgesComments_OnBothUsersAnswers()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        var question = new Question { SenderId = alice.Id, Content = "q" };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var qr1 = new QuestionRecipient { QuestionId = question.Id, ReceptorId = alice.Id };
        var qr2 = new QuestionRecipient { QuestionId = question.Id, ReceptorId = bob.Id };
        _context.QuestionRecipients.AddRange(qr1, qr2);
        await _context.SaveChangesAsync();

        var aliceAnswer = new Answer { QuestionId = question.Id, ReceptorId = alice.Id, CreatorId = alice.Id, Content = "a" };
        var bobAnswer = new Answer { QuestionId = question.Id, ReceptorId = bob.Id, CreatorId = bob.Id, Content = "b" };
        _context.Answers.AddRange(aliceAnswer, bobAnswer);
        await _context.SaveChangesAsync();

        _context.Comments.AddRange(
            new Comment { AnswerId = aliceAnswer.Id, CreatorId = bob.Id, Content = "c1" },
            new Comment { AnswerId = bobAnswer.Id, CreatorId = alice.Id, Content = "c2" });
        await _context.SaveChangesAsync();

        await _sut.BlockAsync(alice.Id, bob.Id);

        Assert.Empty(await _context.Comments.ToListAsync());
    }

    [Fact]
    public async Task BlockAsync_PurgesPendingQuestionRecipients_InBothDirections()
    {
        var (alice, bob) = await SeedTwoUsersAsync();

        // Bob sends a question to Alice; Alice sends one to Bob
        var q1 = new Question { SenderId = bob.Id, Content = "to alice" };
        var q2 = new Question { SenderId = alice.Id, Content = "to bob" };
        _context.Questions.AddRange(q1, q2);
        await _context.SaveChangesAsync();

        _context.QuestionRecipients.AddRange(
            new QuestionRecipient { QuestionId = q1.Id, ReceptorId = alice.Id },
            new QuestionRecipient { QuestionId = q2.Id, ReceptorId = bob.Id });
        await _context.SaveChangesAsync();

        await _sut.BlockAsync(alice.Id, bob.Id);

        Assert.Empty(await _context.QuestionRecipients.ToListAsync());
    }
}
