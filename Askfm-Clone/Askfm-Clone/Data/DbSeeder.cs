using Microsoft.EntityFrameworkCore;

namespace Askfm_Clone.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Ensure database exists & migrations are applied
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                var users = new List<AppUser>
                {
                    new AppUser { Name = "Alice", Email = "alice@test.com", PasswordHash = "$2a$11$dxnBHGVQL1IGXrQTe.oHUugshxG19nw/4qLg/1zpLTVJHl3nPAvz.", Coins = 100, AllowAnonymous = true }, // password: john_456
                    new AppUser { Name = "Bob", Email = "bob@test.com", PasswordHash = "$2a$11$dxnBHGVQL1IGXrQTe.oHUugshxG19nw/4qLg/1zpLTVJHl3nPAvz.", Coins = 50, AllowAnonymous = false },
                    new AppUser { Name = "Charlie", Email = "charlie@test.com", PasswordHash = "$2a$11$dxnBHGVQL1IGXrQTe.oHUugshxG19nw/4qLg/1zpLTVJHl3nPAvz.", Coins = 200, AllowAnonymous = true }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (!context.Questions.Any())
            {
                var alice = context.Users.First(u => u.Name == "Alice");
                var bob = context.Users.First(u => u.Name == "Bob");

                var q1 = new Question
                {
                    Sender = bob,
                    //Receiver = alice,
                    Content = "What’s your favorite book?",
                    IsAnonymous = false,
                    CreatedAt = DateTime.UtcNow
                };

                var q2 = new Question
                {
                    Sender = alice,
                    //Receiver = bob,
                    Content = "What’s your dream job?",
                    IsAnonymous = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Questions.AddRange(q1, q2);
                context.SaveChanges();
            }

            if (!context.Answers.Any())
            {
                var question = context.Questions.First();
                var alice = context.Users.First(u => u.Name == "Alice");

                var answer = new Answer
                {
                    //QuestionId = question.Id,
                    CreatorId = alice.Id,
                    Content = "I love reading science fiction.",
                    CreatedAt = DateTime.UtcNow
                };

                context.Answers.Add(answer);
                context.SaveChanges();
            }

            if (!context.Comments.Any())
            {
                var answer = context.Answers.First();
                var bob = context.Users.First(u => u.Name == "Bob");

                var comment = new Comment
                {
                    AnswerId = answer.Id,
                    CreatorId = bob.Id,
                    Content = "Nice choice!",
                    CreatedAt = DateTime.UtcNow,
                    IsAnonymous = false
                };

                context.Comments.Add(comment);
                context.SaveChanges();
            }

            if (!context.Follows.Any())
            {
                var alice = context.Users.First(u => u.Name == "Alice");
                var bob = context.Users.First(u => u.Name == "Bob");

                var follow = new Follow
                {
                    FollowerId = alice.Id,
                    FolloweeId = bob.Id,
                    CreatedAt = DateTime.UtcNow
                };

                context.Follows.Add(follow);
                context.SaveChanges();
            }

            if (!context.CoinsTransactions.Any())
            {
                var charlie = context.Users.First(u => u.Name == "Charlie");

                var tx = new CoinsTransaction
                {
                    ReceiverId = charlie.Id,
                    Amount = 50,
                    Type = "Reward",
                    CreatedAt = DateTime.UtcNow
                };

                context.CoinsTransactions.Add(tx);
                context.SaveChanges();
            }
        }
    }
}
