# Askfm-Clone

A RESTful API for an anonymous Q&A platform in the style of Ask.fm. Users can
ask and answer questions anonymously or on the record, comment on answers, follow
and block others, and spend coins to unlock interaction features.

## Features

- **Anonymous Q&A** — ask with your identity attached or anonymously, subject
  to the recipient's privacy setting.
- **Broadcasting** — send a single question to up to 20 random users in one call.
- **Answers, comments, likes** — paginated feeds sorted by recency or popularity.
- **Social graph** — follow/unfollow, block/unblock. Blocking atomically purges
  existing likes, comments, follows, and pending questions between the two users.
- **Per-user privacy** — recipients opt in or out of receiving anonymous messages.
- **Coins economy** — balance tracking and transaction history per user.

## Tech stack

| Layer | Choice |
|---|---|
| Runtime | .NET 8 / ASP.NET Core |
| Data | EF Core + SQL Server (LocalDB for dev) |
| Auth | JWT (HS256) + BCrypt + SHA256-hashed refresh tokens with replay detection |
| Docs | Swagger / OpenAPI with Bearer-token auth |

## Architecture

- Controllers → Services → `AppDbContext`. DTOs at every API boundary.
- Composite keys and `DeleteBehavior` rules configured via the EF Core Fluent API.
- Multi-table mutations (block-user cascade) wrapped in explicit
  `BeginTransactionAsync` for atomicity.

## Getting started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (ships with Visual Studio on Windows)

### Setup

```bash
git clone <repo-url>
cd Askfm-Clone
dotnet restore
```

The JWT signing key is not committed. Register it on your machine with
user-secrets:

```bash
dotnet user-secrets set "JwtSection:Key" "<any-random-32-byte-string>" \
  --project Askfm-Clone/Askfm-Clone/Askfm-Clone.csproj
```

Apply migrations and run:

```bash
dotnet ef database update --project Askfm-Clone/Askfm-Clone
dotnet run --project Askfm-Clone/Askfm-Clone
```

Swagger is available at `https://localhost:7211/swagger` in development.

## Dummy data

The API seeds a small fixture on first startup so the endpoints are usable
immediately. The seeder lives in
[`Data/DbSeeder.cs`](Askfm-Clone/Askfm-Clone/Data/DbSeeder.cs) and is invoked
once from `Program.cs`. It is guarded by per-table `if (!context.X.Any())`
checks, so it runs only against an empty database.

**Seeded users** — all share the password `john_456`:

| Name    | Email               | Coins | AllowAnonymous |
|---------|---------------------|-------|----------------|
| Alice   | alice@test.com      | 100   | true           |
| Bob     | bob@test.com        | 50    | false          |
| Charlie | charlie@test.com    | 200   | true           |

The seeder also creates: 2 questions (one anonymous), 1 answer from Alice,
1 comment from Bob, a follow relationship (Alice → Bob), and a 50-coin reward
transaction for Charlie.

**To reset the fixture**, drop and recreate the database:

```bash
dotnet ef database drop --force --project Askfm-Clone/Askfm-Clone
dotnet ef database update --project Askfm-Clone/Askfm-Clone
dotnet run --project Askfm-Clone/Askfm-Clone
```

The next startup will re-seed automatically.

## Using the API from Swagger

1. `POST /api/authentication/login` with a seeded user's credentials
   (e.g. `alice@test.com` / `john_456`).
2. Copy the `accessToken` from the response.
3. Click **Authorize** in Swagger, paste the token, and try protected endpoints.

## Tests

```bash
dotnet test
```

`BlockService` is covered by 9 tests exercising the transactional purge of
follows, likes, comments, and question recipients on user block. CI runs on
every push and PR via GitHub Actions.
