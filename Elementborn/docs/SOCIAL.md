# Social layer

Accounts, notifications, feedback, friends/invites, messaging, and moderation for multiplayer. Built so the
**game code never depends on a specific backend**: the logic talks to Core *seams*, with a local in-memory
implementation for offline use and tests, and a networked adapter behind the same seams.

## Backend choice: Nakama

For a cheap, seamless, locally testable stack, the project targets **[Nakama](https://heroiclabs.com/nakama/)**
(Heroic Labs, open-source):

- **Cheap** — self-host the server for free; runs on a ~$5/mo VPS, or use the managed cloud's free dev tier.
- **Seamless** — one official Unity client SDK, and the features we need (accounts, **friends**, **chat**,
  in-app **notifications**, **parties** for invites, **matches** for co-op) are first-class, not bolted on.
- **Locally testable** — `docker compose up` runs the full server + Postgres on your machine; the same client
  code points at `localhost` for dev or your host for production via one config switch.

Alternatives if you'd rather: **Supabase** (Postgres + realtime + auth, self-hostable via Docker) or **Unity
Gaming Services** (Auth + Friends + Lobby/Relay + Vivox). The seams below mean swapping is an adapter change,
not a rewrite.

## Architecture

```
App logic (NotificationCenter, FeedbackService, ...)   <- callers use this
        |
        v
Core seams: IUserDirectory · INotificationStore · IFeedbackStore
        |                                   |
        v                                   v
LocalUserDirectory / InMemory*        NakamaUserDirectory / Nakama* (networked build)
```

`SocialHub` (Game, singleton) constructs one set of implementations and exposes them. Phase 1 wires the local
ones; the networked build constructs Nakama-backed ones there instead — **nothing else in the game changes**.

## Phase 1 (this release)

- **Identity & roles** — `UserRef` + `UserRole { Player, SessionAdmin, Admin }`; `IUserDirectory` /
  `LocalUserDirectory` track who exists and who's an admin. `SocialHub.CurrentUser` is the local identity
  (a real sign-in sets it in the networked build); `GrantSelfAdmin()` is a dev helper.
- **Notifications** (its own system) — `Notification` + `NotificationKind`; `NotificationCenter` posts to a
  user, reads inboxes, tracks unread, and marks read, raising `Posted` for a HUD badge. Storage is the
  `INotificationStore` seam. `NotificationController` (Game) exposes the current user's unread count + inbox.
- **Feedback → admins** — `FeedbackService.Submit(Bug|Suggestion, ...)` records a `FeedbackReport` and posts a
  `Feedback` notification to **every admin**. `FeedbackController` (Game) is the `SubmitBug`/`SubmitSuggestion`
  entry point for an in-game "report a bug / suggest a feature" section open to all users.

All of phase 1 is pure C# with an in-memory backend, so it runs and is unit-tested offline
(`SocialPhase1Tests`). No server needed to develop against it.

## Phase 2 (this release) — friends & invites

- **Friend graph** — `IFriendGraph` / `LocalFriendGraph`: `SendRequest` / `Accept` / `Decline` / `Remove`,
  with `AreFriends`, `FriendsOf`, pending in/out lists, and `StatusBetween`. Friendships are symmetric;
  requests are directed; a reciprocal request auto-accepts. `FriendService` adds the notification side-effects
  (a request notifies the target; an accept notifies the requester) and `FriendController` (Game) backs a
  friends-list UI.
- **Session invites** — `InviteService` + `GameInvite`: invite **a friend** (friendship is required) to your
  `SocialHub.CurrentSessionId`; the target gets an `Invite` notification. `Accept` returns the session id to
  join (and notifies the host); `Decline`/`Cancel` are there too. `InviteController` (Game) raises a
  `JoinSession(sessionId)` event on accept — the **netcode layer** (a Nakama party/match join) hooks that to
  actually connect.

Both are pure C# behind the same seams, unit-tested offline (`SocialPhase2Tests`).

## Phase 3 (this release) — messaging & moderation

- **Messaging** — `IMessageTransport` / `InMemoryMessageTransport` and `ChatService`: direct messages between
  two users (a canonical channel, so both ends resolve the same conversation) and a per-session channel, with
  history and a `MessageReceived` event for a chat window. `ChatController` (Game) backs it.
- **Moderation (ban / allow)** — `ModerationService` enforces roles: only an **admin** bans globally; an admin
  or **session-admin** bans (or allows back into) a session. Every action notifies the affected user.
  `CanJoin(user, session)` is the gate the join flow consults — and `InviteController.Accept` already consults
  it before firing `JoinSession`, so a banned user's accept won't connect. `ModerationController` (Game) backs
  a moderation screen.

Both are pure C# behind seams, unit-tested offline (`SocialPhase3Tests`).

## Status

The social layer is **feature-complete in code** across all three phases — identity/roles, notifications,
feedback, friends, invites, messaging, and moderation — each pure C# behind a seam with an in-memory
implementation, so the whole thing runs and is unit-tested with no server. What remains is **external**: the
Nakama adapters that implement the seams, a running Nakama (local Docker for dev, a host for production), the
netcode that hooks `InviteController.JoinSession` to an actual match join. (The in-game **UI** now exists — see
below — and the Nakama adapters are the next step.)

## In-game social UI

`SocialMenuController` is a code-built, toggled overlay (default key **J**, Esc to close) that puts the social
layer on screen, built through `UiTheme`:

- **Notifications** — recent items with unread count and mark-read / mark-all-read.
- **Friends** — add by id, accept/decline incoming requests, invite a friend to your session, unfriend, and
  accept/decline invites sent to you.
- **Chat** — the session channel: recent history plus a send box (incoming messages refresh the history live).
- **Feedback** — a bug/suggestion form that files through `FeedbackController` (admins get the notification).
- **Moderation** — ban/allow from the session, and (for admins) global ban/lift. The tab only appears when the
  current user can moderate; `SocialHub.GrantSelfAdmin()` flips that on locally for testing.

Drop the one component on any GameObject and it auto-ensures a `SocialHub` and the six controllers. Text entry
uses the new `UiTheme.Input` factory (TextMeshPro when present, legacy `InputField` otherwise). Because the
backend is still the in-memory local implementation, everything here works offline against your own session;
the cross-player reality arrives with the Nakama adapters + a server + netcode — all now provided behind the `ELEMENTBORN_NAKAMA` define; see `NETCODE.md`.

## Testing locally

- **Now:** the in-memory implementations need nothing — play in the Editor or run the EditMode tests.
- **With Nakama:** `docker compose up` a local Nakama, point `SocialHub` at the Nakama adapters, and develop
  against `localhost`; the same build talks to your hosted server in production.

The UI (a notifications panel, a feedback form, friends list, chat window, a moderation screen) wires onto
these controllers and is a presentation follow-up.
