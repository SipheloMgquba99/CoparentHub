# Security & Compliance Notes — CoparentHub API

This document summarizes the security controls already built into this API and what
you (the operator) are responsible for configuring before deploying it.

## Secrets

- **Never commit real secrets.** `Jwt:Secret` and `ConnectionStrings:DefaultConnection`
  must be supplied via environment variables (`Jwt__Secret`, `ConnectionStrings__DefaultConnection`)
  or `dotnet user-secrets` locally, and via your platform's secret manager in production
  (Azure Key Vault, AWS Secrets Manager, etc).
- A JWT signing secret was previously committed to `appsettings.json` in this repository.
  **Treat that value as permanently compromised.** It has been removed from this codebase;
  rotate/replace it with a newly generated 256-bit+ secret before any deployment, and audit
  any tokens that may have been issued with it.
- In `Development`, if `Jwt:Secret` is empty or too short, `Program.cs` automatically
  generates a temporary in-memory secret at startup so local development still works —
  tokens simply stop being valid across app restarts. In any non-Development environment,
  a missing/short secret causes the app to fail to start (fail closed, not open).

## Authentication & Authorization

- Passwords are hashed with BCrypt (work factor 12), never stored or logged in plain text.
- Auth uses short-lived (default 60 minute, configurable via `Jwt:ExpiryMinutes`) signed
  JWTs (HMAC-SHA256). There is currently no refresh-token flow — expired sessions require
  a new login.
- **Session invalidation on password change**: every user has a `SecurityStamp` (a GUID),
  embedded as a claim in every issued token and bumped whenever the password is reset
  (`SetPasswordHashAsync`). A `JwtBearerEvents.OnTokenValidated` check in `Program.cs`
  compares the token's stamp against the current DB value on every request and rejects
  the token if they differ — so a password reset immediately invalidates every previously
  issued token, not just the one used to reset it. This reads through the existing 10-minute
  `CachedUserRepository` cache, so it adds no real per-request DB cost.
- **Per-account lockout**: after `Auth:LockoutThreshold` (default 5) consecutive failed
  logins, an account is locked for `Auth:LockoutMinutes` (default 15), independent of the
  per-IP rate limit below. A locked-out login attempt returns the same generic "Invalid
  email or password" message as a wrong password (never a distinct "locked" message) and
  skips password verification entirely, so an attacker can't use the response to
  fingerprint locked accounts or keep re-extending the lock. The counter resets to 0 on any
  successful login.
- Every endpoint other than `/api/auth/register` and `/api/auth/login` requires a valid
  bearer token (`[Authorize]`), and handlers additionally check family membership before
  returning or mutating any family/child/event data (see `Family.IsMember`).

## Transport & Headers

- HTTPS is enforced (`UseHttpsRedirection`, `UseHsts` outside Development).
- `SecurityHeadersMiddleware` adds `X-Content-Type-Options`, `X-Frame-Options: DENY`,
  a restrictive `Content-Security-Policy`, `Referrer-Policy: no-referrer`, and a locked-down
  `Permissions-Policy`, and strips the `Server`/`X-Powered-By` headers.
- CORS is deny-by-default: only origins listed in `Cors:AllowedOrigins` may call the API
  with credentials. Set this to your real frontend origin(s) in production.

## Rate Limiting

- `/api/auth/*` (and `DELETE /api/account`) is limited per-IP (default: 5 requests / 60s)
  to slow down credential stuffing and brute-force attempts.
- Sending a message is limited per-IP (default: 20 requests / 60s) to slow down spam.
- A global per-IP limiter (default: 100 requests / 60s) applies to the whole API.
- Kestrel additionally caps every request body at 1 MB — there's no file-upload endpoint
  in this API, so any request that large is malformed or abusive.
- Tune via `RateLimiting:*` and `Auth:Lockout*` configuration.

## Audit / Compliance Logging

- `AuditLoggingBehavior<TRequest,TResponse>` (MediatR pipeline behavior) logs every
  state-changing command (registration, login, family create/join, child add/remove,
  event create/update/cancel, RSVP) with: actor user id, client IP, action name, outcome
  (success/failure/exception), duration, and a redacted snapshot of the request fields.
  Any property whose name contains "password", "token", "secret", or "hash" is
  automatically replaced with `[REDACTED]` before logging — it is not possible to
  accidentally log credentials through this path.
- Read-only queries are not audited, to keep the trail focused on actions that change data.
- Failed/invalid JWTs and unauthenticated access attempts are logged separately via
  `JwtBearerEvents` in `Program.cs` (category `Security.Authentication`).
- All audit log lines are tagged with `AUDIT` in the message template, so they can be
  routed to a separate sink/retention policy in your logging pipeline (e.g. Serilog sink,
  Azure Monitor, CloudWatch) if you need a longer retention period than application logs.

## Database Provider

- The connection string used to be in SQL Server LocalDB format and the code called
  `UseSqlServer(...)`, but the existing EF Core migrations were generated for
  **PostgreSQL** (column types like `uuid`, `timestamp with time zone`, `character
  varying`). This meant the app would fail to apply its own migrations. Fixed: the code
  now calls `UseNpgsql(...)`, and `appsettings.Development.json` has a working local
  Postgres connection string. Set `ConnectionStrings:DefaultConnection` to your real
  Postgres instance in every other environment.

## Encryption at Rest

- Children's names and dates of birth, and event titles/notes (which can contain
  medical or school details), are encrypted at the application layer with **AES-256-GCM**
  before being written to the database, and transparently decrypted on read
  (`AesGcmFieldEncryptor`, wired in via EF Core value converters in `AppDbContext`).
  GCM is authenticated encryption: a tampered or corrupted stored value fails to decrypt
  loudly instead of silently returning wrong data.
- **`Encryption:Key`** (base64, 32 bytes) must be set via `Encryption__Key` env var or
  `dotnet user-secrets` — generate one with `openssl rand -base64 32`. Unlike
  `Jwt:Secret`, this key is **never** auto-generated, in any environment: doing so would
  silently make previously-encrypted rows unreadable after a restart, which is a
  data-loss bug, not just an inconvenience. The app fails to start if it's missing or
  the wrong length.
  - `appsettings.Development.json` ships with a fixed, publicly-known placeholder key
    purely so a fresh clone runs immediately against an empty local database. It
    provides no real confidentiality — never point it at real data, and always set a
    private key via `Encryption__Key` outside of local development.
  - **Losing this key permanently loses access to every encrypted field.** Back it up
    with the same care as your database backups, not as a password.
- **`User.Email` is intentionally NOT encrypted.** Login looks users up by
  `WHERE Email = @value`, and AES-GCM's random nonce means encrypting the same address
  twice produces different ciphertext — that would break the lookup. Encrypting it
  safely requires a separate deterministic "blind index" column (e.g. an HMAC-SHA256 of
  the lowercased email, queried instead of the plaintext column) plus a migration to add
  it; this is a reasonable next step but a bigger schema change, so it's called out here
  as a follow-up rather than implemented half-safely.
- **Existing data / migrations**: the `EncryptSensitiveFields` migration only widens
  columns to `text` so encrypted values fit — it does not encrypt rows that already
  exist. If you're applying this to a database that already has data in it, you need a
  one-time backfill (read with the old code, write back with the new code so the
  encryptor runs) before serving traffic on the new build. See the comment at the top of
  that migration file for details. A brand-new/dev database needs no extra steps.
- This migration was authored without access to the .NET SDK. Run
  `dotnet ef migrations add` after pulling it — it should report no pending model
  changes. If it doesn't, reconcile the mismatch between the migration, the model
  snapshot, and `AppDbContext` before deploying.
- **No key-rotation tooling exists.** The app supports exactly one active key, read once
  at startup — there's no key-id/versioning in the stored ciphertext and no built-in way
  to re-encrypt existing rows under a new key. Rotating the key today means writing a
  one-off offline job that decrypts every encrypted column with the old key and
  re-encrypts with the new one before cutting over. Known limitation, not built as part
  of this pass — flagged here rather than left silent.
- This application stores children's names and dates of birth. Treat this database as
  containing sensitive personal data about minors:
  - Restrict database access to the application's service identity only.
  - Enable encryption at rest for the database and backups.
- Error responses never leak stack traces or internal exception details outside of
  Development (`GlobalExceptionHandler`).

## Data Deletion & Referential Integrity (POPIA)

- **Account deletion ("right to erasure")**: `DELETE /api/account` (password-confirmed,
  `DeleteAccountHandler`) permanently deletes a user's account. If they're the sole member
  of a family, that family and everything scoped to it (children, events, expenses,
  messages, notifications) is deleted too. If a co-parent remains, the family and its
  shared history stay intact for that co-parent — the departed user's historical
  expenses/messages/notifications have no foreign key to `User` at all, so they're safely
  orphaned and simply display as "Unknown" wherever a name would otherwise be shown. The
  whole operation runs inside one DB transaction (`IUnitOfWork.ExecuteInTransactionAsync`)
  so a mid-way failure can't leave the account half-deleted.
  **Not currently exposed in the frontend** — the endpoint exists and is fully functional,
  but self-service deletion is deliberately not surfaced to users yet, pending an admin
  panel that will manage account deletion instead.
- **Cascade-enforced family deletion**: `ScheduledEvent`, `Notification`, `Expense`, and
  `Message` all have a DB-level foreign key to `Family` with `ON DELETE CASCADE` — deleting
  a family (via the account-deletion path above, or the existing family-delete feature)
  is guaranteed by the database to remove every row scoped to it, rather than relying on
  application code remembering to clean up each table individually. `FamilyMember`,
  `Child`, `FamilyInvite`, `PasswordResetToken`, and `PushSubscription` already had this
  same cascade behavior from earlier in the project.

## Before Going to Production — Checklist

- [ ] Rotate `Jwt:Secret` and set it via a secret manager, not a config file.
- [ ] Generate a real `Encryption:Key` (`openssl rand -base64 32`) and set it via a
      secret manager — never reuse the placeholder key shipped in
      `appsettings.Development.json`. Back it up; losing it loses access to all
      encrypted data.
- [ ] Set `Cors:AllowedOrigins` to your real frontend domain(s) only.
- [ ] Point `ConnectionStrings:DefaultConnection` at a production database with least-
      privilege credentials, encryption at rest, and automated backups.
- [ ] Route `AUDIT`-tagged logs to durable, access-controlled storage with a retention
      policy matching your compliance requirements.
- [ ] Review `RateLimiting:*` and `Auth:Lockout*` values for your expected traffic.
