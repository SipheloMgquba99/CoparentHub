# Deploying CoparentHub (free tier, for now)

This is the guide for getting CoparentHub live on the internet at **$0/month**, using
[Render](https://render.com) to run the API and serve the frontend, and
[Neon](https://neon.tech) for a permanent free Postgres database. It's meant as a bridge —
see [Moving to AWS later](#moving-to-aws-later) for what changes (and what doesn't) when you're
ready to leave the free tier behind.

## Why Render + Neon, not Fly.io

Fly.io was the natural first choice — the app is already fully Dockerized — but Fly removed
its free tier in 2024. Every Fly account now needs a payment method on file, and their managed
Postgres starts at $38/month. That's not "free."

Render's free plan needs no card: free static-site hosting (unlimited, not time-boxed) plus
750 free web-service instance-hours per month — enough to run the API continuously all month
with room to spare. The one real limitation: a free Render **web service** spins down after 15
minutes of no traffic and takes about a minute to wake back up on the next request. Fine for a
family testing the app casually; not fine if you need instant availability at 2am — that's a
sign you've outgrown the free tier, not a bug.

Render's own free Postgres expires after 30 days (then a 14-day grace period before deletion),
which isn't a real option for data you care about. **Neon**'s free Postgres has no such
expiration — it's a permanent tier (0.5GB storage, 100 compute-hours/month, scales to zero
when idle) — so the database is the one piece that isn't hosted on Render at all.

## Architecture

```
                    ┌─────────────────────────┐
   Browser  ─────▶  │  coparenthub-web        │   Render static site (free, unlimited)
                    │  (built frontend/dist)  │
                    └───────────┬─────────────┘
                                │ HTTPS (VITE_API_BASE_URL)
                                ▼
                    ┌─────────────────────────┐
                    │  coparenthub-api        │   Render web service (free, Docker,
                    │  (backend/Dockerfile)   │   750 instance-hrs/mo, sleeps when idle)
                    └───────────┬─────────────┘
                                │ Postgres connection string
                                ▼
                    ┌─────────────────────────┐
                    │  Neon Postgres          │   External, permanent free tier
                    └─────────────────────────┘
```

## Prerequisites

- This repo pushed to GitHub (Render deploys straight from a connected GitHub repo).
- A [Render account](https://render.com) (sign up with GitHub — no card required for the free plan).
- A [Neon account](https://neon.tech) (also no card required for the free tier).

## Step 1 — Create the Neon database

1. Sign in to Neon, create a new project (any name/region).
2. Neon gives you a connection string immediately — copy it. It looks like:
   ```
   postgresql://<user>:<password>@<host>/<dbname>?sslmode=require
   ```
3. Convert it to the `Host=...;` format this app's `ConnectionStrings:DefaultConnection`
   expects:
   ```
   Host=<host>;Port=5432;Database=<dbname>;Username=<user>;Password=<password>;Ssl Mode=Require;Trust Server Certificate=true
   ```
   Keep this handy — you'll paste it into Render in the next step.

## Step 2 — Generate your secrets

Two values need to be cryptographically random and are never committed to the repo. Generate
your own (don't reuse examples from this doc or any chat history):

```sh
# JWT signing secret (must decode to at least 32 bytes)
openssl rand -hex 32

# Field-encryption key (must be base64, decoding to exactly 32 bytes)
openssl rand -base64 32
```

Keep both outputs handy for the next step. If you ever need to rotate the encryption key,
note: any data already encrypted with the old key becomes unreadable — treat it like a
database backup, not a password.

## Step 3 — Deploy the Render Blueprint

This repo includes `render.yaml`, which defines both services (`coparenthub-api` and
`coparenthub-web`) as one Blueprint.

1. In the Render dashboard: **New → Blueprint**, connect this GitHub repo.
2. Render reads `render.yaml` and shows both services it's about to create.
3. It will prompt you for the values marked `sync: false` in the file. Fill them in:

   | Prompt | Value |
   |---|---|
   | `ConnectionStrings__DefaultConnection` (on `coparenthub-api`) | the Neon connection string from Step 1 |
   | `Jwt__Secret` (on `coparenthub-api`) | the `openssl rand -hex 32` output from Step 2 |
   | `Encryption__Key` (on `coparenthub-api`) | the `openssl rand -base64 32` output from Step 2 |
   | `Cors__AllowedOrigins__0` (on `coparenthub-api`) | leave as a placeholder for now (e.g. `https://placeholder.onrender.com`) — fixed in Step 4 |
   | `VITE_API_BASE_URL` (on `coparenthub-web`) | leave as a placeholder for now — fixed in Step 4 |

4. Click **Apply**. Render builds and deploys both services. This takes a few minutes,
   especially the first Docker build.

## Step 4 — Wire up the real URLs

The frontend needs to know the API's URL, and the API needs to know the frontend's URL for
CORS — but neither is known until after the first deploy in Step 3. This is normal, not a
mistake — fix it now:

1. Once both services show as live, copy their actual URLs from the Render dashboard (they'll
   look like `https://coparenthub-api-xxxx.onrender.com` and
   `https://coparenthub-web-xxxx.onrender.com` — Render appends a random suffix if your exact
   name was taken).
2. On `coparenthub-api` → **Environment**: set `Cors__AllowedOrigins__0` to the real
   `coparenthub-web` URL. Saving triggers an automatic redeploy.
3. On `coparenthub-web` → **Environment**: set `VITE_API_BASE_URL` to the real
   `coparenthub-api` URL. Since this is baked in at build time, saving triggers a rebuild —
   necessary, not optional, for the change to actually take effect.

## Step 5 — Verify it's live

```sh
curl https://<your-api-url>.onrender.com/health
# {"status":"healthy"}
```

Then open the `coparenthub-web` URL in a browser, register an account, create a family, and
confirm the whole flow works. The first request after a period of inactivity will be slow
(~30-60s cold start) — that's the free plan spinning the container back up, not a bug.

## What you get automatically after this

- **Migrations apply on every startup** — no separate manual migration step. This is normally
  risky with multiple instances racing to apply the same migration, but this app only ever
  runs as a single instance (the free plan doesn't support horizontal scaling anyway), so that
  risk doesn't apply here.
- **Pushing to your main branch redeploys both services automatically** (Render watches the
  connected GitHub repo).
- **Structured logs** are already being written to console via Serilog — Render's dashboard has
  a built-in log viewer, so you don't need to run Seq in this environment (it's still useful
  locally via `docker compose up`, just not worth the resource cost here).

## Known free-tier limits to expect

| Limit | What it means |
|---|---|
| API sleeps after 15 min idle | First request after a while takes ~30-60s to wake up |
| 750 instance-hours/month (workspace-wide) | Covers one service running 24/7 with room to spare; don't add more always-on free services in the same workspace without checking the math |
| Neon: 0.5GB storage, 100 compute-hours/month | Plenty for a family testing the app; the DB itself suspends (not deletes) if you somehow exceed it, resuming next cycle |
| No horizontal scaling on free plan | Exactly why auto-migrate-on-startup is safe here — revisit if you ever move to a paid multi-instance plan |

## Moving to AWS later

Nothing here is a dead end. When you're ready:

- The same `backend/Dockerfile` and `frontend/Dockerfile` run unchanged on **ECS/Fargate** or
  **App Runner** — no rewrite, just a different place to point `docker build`.
- Neon's connection string can be swapped for an **RDS Postgres** instance by changing exactly
  one environment variable (`ConnectionStrings__DefaultConnection`) — the schema and migrations
  are unaffected.
- Secrets move from Render's dashboard into **AWS Secrets Manager** or **Parameter Store**.
- If you outgrow single-instance, revisit the "migrate on every startup" note above — with
  multiple instances you'd want a one-off migration step (a CI job or a manual
  `dotnet ef database update`) instead of every instance racing to migrate on boot.

## Local development

This deployment path is separate from local development — `docker compose up` (see the root
`docker-compose.yml`) still works exactly as before for running everything on your own machine,
including Seq for log inspection. See `.env.example` for the local-only config it uses.
