# Self-hosting Elementborn

This guide covers running your own server for the online layer (accounts, friends, chat, invites, moderation,
authoritative sessions) and shipping a client that connects to it.

> **You only need a server for the online layer.** The game runs fully offline/single-player on the in-memory
> backend with no server at all (that's the default build). Self-host when you want cross-player features. The
> server is **Nakama** (open-source); see `NETCODE.md` for how the game talks to it and `SOCIAL.md` for what the
> layer does.

---

## 1. What you're hosting

Two things ship together:

1. **The server** — a Nakama instance + its database (CockroachDB), plus the Elementborn server module
   (`nakama/src/main.ts`, which provides feedback-to-admin, trusted notifications, authoritative sessions, and
   the ban-gated match join).
2. **The client** — a Unity build of the game, compiled with the `ELEMENTBORN_NAKAMA` define and pointed at your
   server's address.

The repo already contains the server stack (`nakama/docker-compose.yml`), the module and its build setup
(`nakama/package.json`, `nakama/tsconfig.json`), and the client-side connection code
(`Assets/Elementborn/Game/Social/Nakama/`).

---

## 2. Requirements

### Server host
- A Linux machine you control — a VPS, a cloud VM, or bare metal. A small instance is fine to start:
  **2 vCPU / 4 GB RAM / 20 GB SSD** for light use; scale up with concurrent players.
- **Docker** and **Docker Compose** installed.
- Outbound internet for pulling images.
- For production: a **domain name** and a **TLS certificate** (via a reverse proxy — see §6).

### Ports
Nakama uses three:
| Port | Purpose | Exposure |
| --- | --- | --- |
| 7350 | Client API + realtime socket | open to players (behind TLS in prod) |
| 7349 | gRPC API | internal / admin only |
| 7351 | Developer console (web UI) | restrict to you (VPN / IP allowlist) |

### Build tools (on your dev machine, not the server)
- **Unity** (the version this project targets) to build the client.
- The **Nakama Unity SDK** (added to the client — see `NETCODE.md` §1).
- **Node.js + npm** to build the server module.

---

## 3. Build the server module

The module is TypeScript compiled to a single JS file Nakama loads.

```sh
cd nakama
npm install        # pulls the nakama-runtime types + typescript
npx tsc            # produces nakama/build/index.js
```

The compose file already passes `--runtime.js_entrypoint "build/index.js"`, so the built file is picked up
automatically when the container starts.

---

## 4. Configure before first run

The shipped compose is a **development** setup. Before exposing it anywhere, change the defaults:

- **Server key** — replace `defaultkey` (in `nakama/docker-compose.yml`, the `--socket.server_key` flag) with a
  long random string. The client must use the same key (`NakamaSocialInstaller.serverKey`).
- **Console password** — set a strong Nakama console login (configure via a Nakama config file or the
  `--console.username` / `--console.password` flags).
- **Database** — the dev compose runs CockroachDB single-node insecure. For production, run CockroachDB secure
  (or a managed Postgres-compatible DB Nakama supports), with real credentials and persistent volumes.
- **Token expiry, CORS, rate limits** — set in a Nakama YAML config mounted into the container, rather than only
  on the command line, for anything beyond a quick test.

A config file keeps this manageable; mount it and pass `--config /nakama/data/config.yml`.

---

## 5. Start the server

```sh
cd nakama
docker compose up -d          # start in the background
docker compose logs -f nakama # watch it boot (look for "Elementborn runtime loaded.")
```

The console is at `http://YOUR_HOST:7351`. Confirm the module loaded (Runtime ▸ Modules) and that the database
migration ran.

---

## 6. Production: TLS and a reverse proxy

Players should connect over **HTTPS/WSS**, not plain HTTP. Put a reverse proxy in front of port 7350 that
terminates TLS:

- **Caddy** is the simplest (automatic Let's Encrypt): proxy `your.domain` → `127.0.0.1:7350`.
- **Nginx / Traefik** work too; make sure to proxy **WebSocket upgrades** (the realtime socket needs them).

Then firewall the host so only 443 (your proxy) is public; keep 7349/7351 private (VPN or IP allowlist). Point
the client at `https` + your domain (see §8).

---

## 7. Seed the first admin

Moderation RPCs (`ban` / `unban`) and feedback delivery need at least one admin. The module includes a dev
bootstrap: while no admins exist, the first caller of the `dev_seed_admin` RPC becomes admin. Call it once from a
trusted client (or remove/guard it before going public and instead write the `config/admins` storage record from
the console). After that, admins are configured server-side.

---

## 8. Build and ship the client

On your dev machine:

1. Add the **Nakama Unity SDK** to the project (`NETCODE.md` §1).
2. Add the **`ELEMENTBORN_NAKAMA`** scripting define (Project Settings ▸ Player).
3. Put a **`NakamaSocialInstaller`** on a bootstrap object that runs before `SocialHub`, and set its connection
   fields to your server: `scheme = "https"`, `host = "your.domain"`, the production `serverKey`, and the right
   port (443 behind your proxy, or 7350 direct).
4. Build per target (Meta Quest / PCVR / flat PC) as usual. The flat/third-person path is the most
   straightforward; VR also needs the XR setup in `VR_SETUP.md`.
5. Distribute the build (store, sideload, installer). Every copy points at your server.

A mismatched server key, wrong scheme/port, or a proxy that drops WebSocket upgrades are the usual first-connect
failures — check the client log and `docker compose logs nakama`.

---

## 9. Operating it

- **Backups** — back up the database volume regularly (CockroachDB backups, or volume snapshots). That's where
  accounts, friends, storage (feedback/bans), and notifications live.
- **Upgrades** — pull newer images, re-run the migrate step, and `docker compose up -d`. Rebuild the module
  (`npx tsc`) when you change `main.ts`.
- **Monitoring** — Nakama exposes metrics (Prometheus) and structured logs; watch CPU/RAM and socket counts.
- **Scaling** — Nakama scales horizontally behind a load balancer, with CockroachDB clustered; start single-node
  and grow when concurrency demands it.
- **Security** — change all default credentials, keep 7349/7351 private, set rate limits, and keep the host
  patched. Treat the server key like a secret.

---

## 10. Quick checklist

- [ ] Linux host with Docker + Compose
- [ ] Server module built (`npx tsc` → `build/index.js`)
- [ ] Default server key / console password / DB changed
- [ ] `docker compose up -d`, module loaded (console)
- [ ] TLS reverse proxy in front of 7350; 7349/7351 firewalled
- [ ] First admin seeded
- [ ] Client built with `ELEMENTBORN_NAKAMA` + installer pointed at your domain
- [ ] Backups + monitoring in place

That's the whole path from a bare server to players connecting. The offline build needs none of it — this is
purely for the online layer.
