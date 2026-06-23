/// <reference types="nakama-runtime" />

// Elementborn Nakama server runtime (TypeScript → goja JS).
//
// This is the server-authoritative counterpart to the client adapters in
// Assets/Elementborn/Game/Social/Nakama. It closes the gaps a client cannot do safely on its own:
//   • submit_feedback  — store feedback AND deliver a notification to every admin (cross-user delivery)
//   • notify_user      — trusted server-side notification to one user (e.g. invite pings)
//   • create_session   — create an authoritative match the host owns; its id is the "session" id
//   • ban / unban      — write/remove SYSTEM-owned ban records (authoritative) and notify the user
//   • the match's join attempt is ban-gated, so a banned user genuinely cannot join
//   • dev_seed_admin   — bootstrap the first admin in development (only when none exist)
//
// Build:  cd nakama && npm install && npx tsc      → build/index.js
// Load:   docker-compose passes --runtime.js_entrypoint "build/index.js"

const SYSTEM_USER = "00000000-0000-0000-0000-000000000000";
const CONFIG_COLLECTION = "config";
const ADMINS_KEY = "admins";
const BANS_COLLECTION = "bans";
const FEEDBACK_COLLECTION = "feedback";
const MATCH_MODULE = "elementborn_session";

const CODE_SYSTEM = 1000;
const CODE_FEEDBACK = 1001;
const CODE_MODERATION = 1002;

function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer): void {
    initializer.registerRpc("submit_feedback", rpcSubmitFeedback);
    initializer.registerRpc("notify_user", rpcNotifyUser);
    initializer.registerRpc("create_session", rpcCreateSession);
    initializer.registerRpc("ban", rpcBan);
    initializer.registerRpc("unban", rpcUnban);
    initializer.registerRpc("dev_seed_admin", rpcDevSeedAdmin);
    initializer.registerMatch(MATCH_MODULE, {
        matchInit: matchInit,
        matchJoinAttempt: matchJoinAttempt,
        matchJoin: matchJoin,
        matchLeave: matchLeave,
        matchLoop: matchLoop,
        matchTerminate: matchTerminate,
        matchSignal: matchSignal,
    });
    logger.info("Elementborn runtime loaded.");
}

// ---- admin / ban helpers ----------------------------------------------------------------------------

function adminIds(nk: nkruntime.Nakama): string[] {
    try {
        const objs = nk.storageRead([{ collection: CONFIG_COLLECTION, key: ADMINS_KEY, userId: SYSTEM_USER }]);
        if (objs.length > 0) {
            const v = objs[0].value as any;
            if (v && v.ids && v.ids.length !== undefined) return v.ids as string[];
        }
    } catch (e) { /* none configured yet */ }
    return [];
}

function isAdmin(nk: nkruntime.Nakama, userId: string): boolean {
    const ids = adminIds(nk);
    for (let i = 0; i < ids.length; i++) if (ids[i] === userId) return true;
    return false;
}

function banKey(scope: string, userId: string, sessionId: string): string {
    return scope === "global" ? "g_" + userId : "s_" + sessionId + "_" + userId;
}

function isBanned(nk: nkruntime.Nakama, userId: string, sessionId: string): boolean {
    const objs = nk.storageRead([
        { collection: BANS_COLLECTION, key: banKey("global", userId, ""), userId: SYSTEM_USER },
        { collection: BANS_COLLECTION, key: banKey("session", userId, sessionId), userId: SYSTEM_USER },
    ]);
    return objs.length > 0;
}

// ---- RPCs -------------------------------------------------------------------------------------------

function rpcSubmitFeedback(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const input = JSON.parse(payload || "{}");
    const id: string = input.id || nk.uuidv4();
    const report = {
        id: id,
        kind: input.kind || 0,
        title: input.title || "",
        body: input.body || "",
        fromUserId: ctx.userId,
        createdTicks: Date.now(),
    };
    nk.storageWrite([{
        collection: FEEDBACK_COLLECTION, key: id, userId: SYSTEM_USER,
        value: report, permissionRead: 2, permissionWrite: 0,
    }]);
    const admins = adminIds(nk);
    for (let i = 0; i < admins.length; i++) {
        nk.notificationSend(admins[i], "New feedback: " + report.title,
            { id: id, kind: report.kind, body: report.body }, CODE_FEEDBACK, ctx.userId, true);
    }
    return JSON.stringify({ ok: true, id: id, notified: admins.length });
}

function rpcNotifyUser(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const input = JSON.parse(payload || "{}");
    if (!input.userId) throw Error("userId required");
    nk.notificationSend(input.userId, input.subject || "Notification",
        input.content || {}, input.code || CODE_SYSTEM, ctx.userId, true);
    return JSON.stringify({ ok: true });
}

function rpcCreateSession(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    // The host owns an authoritative match; its id is the session id friends are invited to.
    const matchId = nk.matchCreate(MATCH_MODULE, { hostUserId: ctx.userId });
    return JSON.stringify({ sessionId: matchId });
}

function rpcBan(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const input = JSON.parse(payload || "{}");
    if (!input.userId) throw Error("userId required");
    const scope = input.scope === "session" ? "session" : "global";
    // Authority: this reference requires an admin. Session-host authority can additionally read the match label
    // (hostUserId) to let a host ban from their own session.
    if (!isAdmin(nk, ctx.userId)) throw Error("not permitted");
    const key = banKey(scope, input.userId, input.sessionId || "");
    nk.storageWrite([{
        collection: BANS_COLLECTION, key: key, userId: SYSTEM_USER,
        value: {
            userId: input.userId, scope: scope, sessionId: input.sessionId || "",
            byUserId: ctx.userId, reason: input.reason || "", createdTicks: Date.now(),
        },
        permissionRead: 2, permissionWrite: 0,
    }]);
    nk.notificationSend(input.userId, scope === "global" ? "Account banned" : "Removed from session",
        { reason: input.reason || "" }, CODE_MODERATION, ctx.userId, true);
    return JSON.stringify({ ok: true });
}

function rpcUnban(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const input = JSON.parse(payload || "{}");
    if (!input.userId) throw Error("userId required");
    if (!isAdmin(nk, ctx.userId)) throw Error("not permitted");
    const scope = input.scope === "session" ? "session" : "global";
    nk.storageDelete([{ collection: BANS_COLLECTION, key: banKey(scope, input.userId, input.sessionId || ""), userId: SYSTEM_USER }]);
    return JSON.stringify({ ok: true });
}

function rpcDevSeedAdmin(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    // Dev bootstrap: the first caller becomes admin, but only while no admins exist.
    if (adminIds(nk).length > 0) throw Error("admins already configured");
    nk.storageWrite([{
        collection: CONFIG_COLLECTION, key: ADMINS_KEY, userId: SYSTEM_USER,
        value: { ids: [ctx.userId] }, permissionRead: 0, permissionWrite: 0,
    }]);
    return JSON.stringify({ ok: true, admin: ctx.userId });
}

// ---- authoritative match (a minimal relay; the join is ban-gated) -----------------------------------

function matchInit(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: { [key: string]: any }): { state: nkruntime.MatchState, tickRate: number, label: string } {
    const label = JSON.stringify({ host: params.hostUserId || "" });
    return { state: { presences: {} }, tickRate: 10, label: label };
}

function matchJoinAttempt(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presence: nkruntime.Presence, metadata: { [key: string]: any }): { state: nkruntime.MatchState, accept: boolean, rejectMessage?: string } {
    if (isBanned(nk, presence.userId, ctx.matchId)) {
        return { state: state, accept: false, rejectMessage: "You are banned from this session." };
    }
    return { state: state, accept: true };
}

function matchJoin(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } {
    for (let i = 0; i < presences.length; i++) (state.presences as any)[presences[i].userId] = presences[i];
    return { state: state };
}

function matchLeave(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } {
    for (let i = 0; i < presences.length; i++) delete (state.presences as any)[presences[i].userId];
    return { state: state };
}

function matchLoop(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, messages: nkruntime.MatchMessage[]): { state: nkruntime.MatchState } {
    for (let i = 0; i < messages.length; i++) {
        // Minimal relay: rebroadcast each message to everyone.
        dispatcher.broadcastMessage(messages[i].opCode, messages[i].data, null, messages[i].sender);
    }
    return { state: state };
}

function matchTerminate(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, graceSeconds: number): { state: nkruntime.MatchState } {
    return { state: state };
}

function matchSignal(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, data: string): { state: nkruntime.MatchState, data?: string } {
    return { state: state, data: data };
}

// Keep InitModule referenced so the bundler/compiler doesn't drop it.
!InitModule && InitModule.bind(null);
