# Event logging (session timelines → Neon Postgres)

Every play session gets a unique id and an **ordered, append-only log** of what happened — logins, moves and
their math, status changes, spawn/respawn points, errors — so a session can be followed start-to-finish and a bug
reproduced from the trail. Logs never contain passwords or tokens.

## Pieces

- **`Core/GameEventLog.cs`** — pure, unit-tested. `GameEvent` (seq, UTC time, kind, name, detail) and
  `SessionEventLog` (monotonic sequence, buffer, `Drain()`, `ToJsonBatch(...)`). No Unity, no secrets.
- **`Game/GameEventLogger.cs`** — the per-session service. Self-bootstraps (`RuntimeInitializeOnLoadMethod`),
  opens a session, **auto-captures** gameplay off the `QuestEvents` bus, exposes typed `Log*` calls, buffers, and
  flushes batches every ~15s / at 50 events / on pause / on quit.
- **`Game/EventSink.cs`** — `IEventSink` (where a batch goes). `ConsoleEventSink` is the dev default; a
  Neon-backed sink is installed at startup with `GameEventLogger.Instance.SetSink(...)`.

## What it captures

Auto (off the existing bus, no extra calls): channel/cast (`element`, `intent`), defeat, tame, summon, craft,
equip, claim-featured, currency, item pickup, NPC talk, quest-complete.

Typed entry points for the rest:

| Call | Logs |
| --- | --- |
| `LogLogin(userId, displayName)` | sign-in — **id + name only, never a password** |
| `LogAction(action, ok, error)` | a discrete action and its error if it failed |
| `LogMath(label, total, breakdown)` | a move's math totals (e.g. `damage = base*mult + bonus`) |
| `LogStatus(stat, value)` | a user/world status (health, level, silver) |
| `LogSpawn(point)` / `LogRespawn(point)` | spawn anchor set / where the player revived |
| `LogLeaderboard(summary)` | the final leaderboard |

**Wired now:** session start/end + the whole bus, **login** (`SocialHub.SetIdentity`), **respawn-anchor set**
(`CheckpointObject`), **respawn** (`RespawnController`, with the source: checkpoint / house / spawn), **move math**
(`PlayerCombatController` → `cast_damage` with the final post-scaling total), and **status** (player health at
death, character level on level-up). **Ready, not yet called:** `LogLeaderboard` — there is no leaderboard system
yet; the call is ready for when one lands (`GameEventLogger.Instance?.LogLeaderboard(...)`).

## Getting events to Neon (transport — wired)

A game client must **not** hold the Postgres connection string — shipping DB credentials in a distributed build
is a serious leak. So events route through the server, which holds the secret:

```
client (GameEventLogger → NeonEventSink, #if ELEMENTBORN_NAKAMA)
        → Nakama RPC "events_ingest" (server-authoritative)
        → Postgres INSERT via nk.sqlExec
```

**What's built:** the `events_ingest` RPC (`nakama/src/main.ts`) creates the tables on startup (idempotent) and
inserts each event (`ON CONFLICT (session, seq) DO NOTHING`, so retries are safe); the client `NeonEventSink`
posts each flushed batch and is installed at startup by `NakamaSocialInstaller` (before identity, so the login
event lands too). Build the module with `cd nakama && npm install && npx tsc`.

**Key point:** `nk.sqlExec` writes to the database **Nakama itself uses**, so for events to land in Neon you run
Nakama against Neon:

- Set `NAKAMA_DB_ADDRESS` (read by `nakama/docker-compose.yml`) to your Neon DSN in an **untracked**
  `nakama/.env`. Never commit it.
- Use the Neon **direct** endpoint, **not** the `-pooler` host: Nakama keeps its own connection pool and Neon's
  PgBouncer pooler (transaction mode) breaks prepared statements. Drop `-pooler` from the host.
- **Rotate the credential** in the Neon console if it has ever been shared (e.g. pasted into a chat).

If you'd rather keep Nakama on its own DB and isolate logs in a separate Neon database, that needs a different
sink (a Go module with its own `sql.DB`, or Neon's HTTP SQL endpoint) — say the word and I'll add it.

## Neon Postgres schema

The RPC creates this automatically; it's here for reference. `detail` is `text` (the client sends `k=v;k=v`),
`session_id` is `uuid` (the client now sends a dashed Guid):

```sql
create table if not exists game_sessions (
    session_id   uuid primary key,
    user_id      text,
    display_name text,
    platform     text,
    app_version  text,
    started_at   timestamptz not null default now(),
    ended_at     timestamptz
);

create table if not exists session_events (
    id          bigserial primary key,
    session_id  uuid not null references game_sessions(session_id),
    seq         bigint not null,          -- monotonic within the session
    kind        text  not null,           -- SessionStart | Login | Action | Math | Status | Spawn | Respawn | Leaderboard | Error
    name        text  not null,           -- short verb: "cast", "respawn", "login"
    detail      text,                     -- "k=v;k=v" payload (never secrets)
    occurred_at timestamptz not null,
    unique (session_id, seq)
);

create index if not exists session_events_by_session on session_events (session_id, seq);
create index if not exists session_events_by_kind    on session_events (kind, occurred_at);
```

The client batch is a JSON array of `{session, seq, ts, kind, name, detail}`; the RPC maps each element to one
`session_events` row and upserts `game_sessions`. No passwords are ever in the payload, so none reach the database.
