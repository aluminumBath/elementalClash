# Netcode — going online with Nakama

The social layer is **backend-agnostic**: every feature runs on the seven Core seams in
`Assets/Elementborn/Core/Social/SocialBackend.cs` (`ISocialBackend`). The default is `LocalSocialBackend`, the
offline in-memory implementation — which is why the whole thing runs and unit-tests with no server. Going online
means supplying a different `ISocialBackend`; nothing in the services, controllers, or UI changes.

This repo ships that implementation for **Nakama** (Heroic Labs, open-source), wrapped in the
`ELEMENTBORN_NAKAMA` scripting define so the project keeps compiling until you opt in. With the define off, none
of the Nakama code compiles and the game uses the local backend.

## What's included (behind the define)

In `Assets/Elementborn/Game/Social/Nakama/`:

- `NakamaConnection` — owns the client, session, and realtime socket (device auth + socket connect).
- `NakamaSocialBackend` — binds the seven seams to Nakama adapters.
- `NakamaAdapters` — the adapters: users → accounts, notifications → Nakama notifications, friends → the friend
  system, chat → Nakama channels, feedback/bans/invites → storage collections. They bridge async Nakama to the
  synchronous seams by **caching**: writes are fire-and-forget (eventually consistent), reads return a cache kept
  fresh by socket events and an initial fetch.
- `NakamaSocialInstaller` — points `SocialBackends.Factory` at Nakama, connects, sets the real identity, and
  hooks `InviteController.JoinSession` to `JoinMatchAsync`.

`SocialBackends.Factory` is the swap point. The default returns a `LocalSocialBackend`; the installer replaces it
before `SocialHub` builds. (`SocialBackendTests` proves the default is local, the override is honored, and a null
factory falls back to local.)

## Setup

1. **Add the Nakama Unity SDK.** Add it to `Packages/manifest.json` as a Git dependency (or import the
   `.unitypackage` from the Nakama releases):

   ```json
   {
     "dependencies": {
       "com.heroiclabs.nakama-unity": "https://github.com/heroiclabs/nakama-unity.git?path=/Packages/Nakama#v3.x.x"
     }
   }
   ```

   Pin `v3.x.x` to a real release tag. (This file is documentation — the manifest isn't modified for you, so the
   project still opens without the SDK.)

2. **Define the symbol.** Project Settings ▸ Player ▸ Scripting Define Symbols → add `ELEMENTBORN_NAKAMA`. Now the
   Nakama files compile. (Adjust any adapter calls if your SDK version's signatures differ — they target the
   documented v3 client/socket API.)

3. **Run a server.** A local dev server is provided:

   ```sh
   cd nakama && docker compose up
   ```

   The Nakama console is at `http://127.0.0.1:7351` (default `admin` / `password`); the client connects to
   `127.0.0.1:7350` with server key `defaultkey`.

4. **Add the installer.** Put `NakamaSocialInstaller` on a bootstrap object that initializes **before**
   `SocialHub` (set Script Execution Order so the installer runs first, or place it on an earlier object). Set its
   host/port/key. On Play it authenticates by device id, swaps in the Nakama backend, and fills in the real
   identity via `SocialHub.SetIdentity`.

5. **Wire the join.** `InviteController.JoinSession(sessionId)` already fires after an accepted, moderation-cleared
   invite; the installer hooks it to `socket.JoinMatchAsync(sessionId)`. Treat the session id as the host's
   match/party id (create the match/party when a session starts and use that id as `SocialHub.CurrentSessionId`).

## Caveats (where server authority is needed)

The client adapters are a working starting point, but some guarantees belong on the server, not the client — so
a Nakama **server runtime module** is included (`nakama/src/main.ts`) that provides exactly those. Build it and
the gaps close:

- **Cross-user notifications** — `submit_feedback` stores the report and notifies every admin via
  `notificationSend`; `notify_user` is a trusted one-user notification (e.g. invite pings). A client can't deliver
  a notification to *another* user; the server can.
- **Ban enforcement** — `ban` / `unban` write SYSTEM-owned ban records and notify the user; the authoritative
  **match join is ban-gated** (`matchJoinAttempt` rejects a banned user), so a ban genuinely blocks a join rather
  than just hiding a button.
- **Authoritative sessions** — `create_session` creates an authoritative match the host owns and returns its id;
  use that as `SocialHub.CurrentSessionId` so invites and session bans key off a real match.
- **Identity** uses device auth for a stable per-install id; swap in a real sign-in (email, Apple, Google, custom)
  as needed.
- **Invites** are kept in a storage collection on the client for the offline-style seam; for real-time invites,
  switch `NakamaStorageInviteStore` to Nakama **parties** (`socket.ReceivedPartyInvite` + `JoinPartyAsync`).

### The server module

`nakama/src/main.ts` (TypeScript → goja). Build and load it:

```sh
cd nakama
npm install        # pulls nakama-runtime types + typescript
npx tsc            # → nakama/build/index.js
docker compose up  # compose passes --runtime.js_entrypoint "build/index.js"
```

RPCs (call from the client with `await client.RpcAsync(session, "<id>", payload)`):

| RPC | Payload | Effect |
| --- | --- | --- |
| `dev_seed_admin` | — | Dev only: makes the caller admin while no admins exist. |
| `submit_feedback` | `{kind,title,body}` | Stores feedback, notifies all admins. |
| `notify_user` | `{userId,subject,content,code}` | Sends one user a notification. |
| `create_session` | — | Creates an authoritative match; returns `{sessionId}`. |
| `ban` / `unban` | `{userId,scope,sessionId,reason}` | Writes/removes an authoritative ban (admin only). |

To make the client authoritative, point the moderation/feedback paths at these RPCs instead of writing storage
directly: e.g. `ModerationController.BanFromSession` → `ban` RPC, `FeedbackController.Submit` → `submit_feedback`
RPC, and set `CurrentSessionId` from `create_session`. The client ban store then stays a read cache that drives
the UI, while the server holds authority.

None of this blocks the offline build: with the define off, the local backend keeps everything working and
unit-tested.
