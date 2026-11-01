# Direct Connection (no My.JDownloader account)

`jdr` can talk straight to a JDownloader instance over its built-in local HTTP Remote API instead of going through
My.JDownloader. Direct mode needs **no account, no internet and no cloud relay**; every command that works against a
device uses the same CLI surface.

| | `myjd` (default) | `direct` |
|---|---|---|
| Route | CLI -> api.jdownloader.org -> device | CLI -> `http://host:3128` |
| Account / login | required (`auth login`) | none |
| Device selection | `device list` / `device use` | n/a (one instance per connection) |
| Encryption | AES (My.JDownloader protocol) | **none** (plain HTTP) |
| Works offline / on a LAN | no | yes |

Direct mode **never falls back** to My.JDownloader. If the local API is unreachable the command fails with a
transport error (exit code 8).

## 1. Enable the local API in JDownloader

The local API is JDownloader's "Deprecated API". It is off by default.

1. In JDownloader open **Settings -> Advanced Settings**, filter for `RemoteAPIConfig`.
2. Enable `deprecatedapienabled` (or set `"deprecatedapienabled": true` in
   `<JDownloader>/cfg/org.jdownloader.api.RemoteAPIConfig.json` while JDownloader is closed).
3. Optionally change `deprecatedapiport` (default `3128`).
4. Leave `deprecatedapilocalhostonly` at `true` unless you really need remote access (see Security).
5. Restart JDownloader.

## 2. Create and use a connection

```bash
jdr connection add local --mode direct --host 127.0.0.1 --port 3128
jdr connection use local
jdr connection test
jdr downloads status
jdr grabber links list
```

`--host` defaults to `127.0.0.1` and `--port` to `3128`, so `jdr connection add local --mode direct` is enough for a
local JDownloader. A connection is a normal profile: `--profile <name>` selects another one for a single command, and
`jdr connection list` shows all of them with their mode.

```bash
jdr connection add cloud                       # My.JDownloader profile (log in afterwards with 'jdr auth login')
jdr connection add nas --mode direct --host nas.lan --allow-remote
jdr connection list
jdr downloads status --profile nas
```

Other commands: `connection get <name>`, `connection use <name>`, `connection remove <name>`,
`connection add <name> ... --force` (replace the connection settings of an existing profile), `doctor` (shows the
resolved mode and endpoint).

### Diagnostics: `jdr connection test`

Probes `/device/ping`, `/jd/version`, `/jd/getCoreRevision` and `/downloadcontroller/getCurrentState`, reporting latency
per step. On failure it names the failing step and tells you what to check:

| Symptom | Meaning |
|---|---|
| `Connection refused` | JDownloader is not running, the API is disabled, or host/port is wrong |
| `Host ... could not be resolved` | DNS / hostname typo |
| `Timed out after Ns` | host reachable but not answering (firewall, hung JDownloader); raise `--timeout` |
| `non-JSON response` | another service is listening on that port |
| `HTTP 404` on every call | not JDownloader's API, or a very old/new build |
| `HTTP 403` | something in between adds an `Origin` header (JDownloader rejects unknown origins) |

Timeouts: `--timeout <seconds>` (or the profile's `timeoutSeconds`, default 30s). Connecting is additionally capped at
5 seconds. System proxies are never used for direct calls.

## 3. Security

JDownloader's local API has **no authentication and no encryption**. Anyone who can reach the port can add links,
change settings, read your download list and shut JDownloader down.

* `jdr` only accepts loopback hosts (`localhost`, `127.0.0.0/8`, `::1`) by default.
* A non-loopback host needs `--allow-remote` when the connection is created. It is re-checked on every run, so a
  hand-edited config cannot silently target a remote machine. Every command against such a host prints a warning.
* Prefer an SSH tunnel and keep the loopback address:
  `ssh -L 3128:127.0.0.1:3128 user@nas` then `jdr connection add nas --mode direct`.
* Never forward port 3128 to the internet. Keep `deprecatedapilocalhostonly` enabled in JDownloader unless you
  deliberately accept the risk.
* `--https` is only for a TLS-terminating reverse proxy in front of JDownloader.

## 4. How it works / compatibility

Commands build the same request plans for both modes (same endpoints and positional parameters). The direct transport
sends `POST /<namespace>/<method>` with `Content-Type: application/json` and a `{"params": [...]}` body, and
unwraps the `{"data": ...}` envelope. Differences from My.JDownloader that the CLI handles for you:

* Object parameters are sent as real JSON objects (My.JDownloader wraps them in JSON strings).
* No request id, signature or encryption is used. No `Origin` header is ever sent.
* Binary endpoints (icons) return raw bytes and work with `--output-file`.

### Not available over a direct connection

These fail immediately with an explicit message (exit code 2); nothing is sent:

| Command / endpoint | Why |
|---|---|
| `device list`, `device use`, `--device` | a direct connection is exactly one JDownloader instance |
| `auth login`, `auth logout` | no account is involved (`auth status` / `auth whoami` report the direct endpoint) |
| `/my/*`, `/t_*` (via `advanced raw request`) | My.JDownloader cloud endpoints |
| `/session/*`, `/device/getSessionPublicKey` | session handshake / relay encryption do not exist locally |

### Works, with caveats

* `device direct-info` answers, but describes JDownloader's My.JDownloader direct-connection helper, which is not
  meaningful for this transport.
* `events listen` long-polls the local API; the event objects are raw JDownloader events and may be shaped differently
  from what My.JDownloader delivers.
* `flash`/Click'n'Load style adds may open a permission dialog inside JDownloader when not routed via My.JDownloader.
* Requests are limited to 1 MB by JDownloader (very large `grabber add` payloads).

Everything else (downloads, grabber, accounts, extraction, settings, captcha, system, update, advanced raw) uses the same
endpoints on both transports. The request shape was checked against a live JDownloader (build 48637): ping, version,
core revision, download state and object-parameter queries (`downloads links list`, `grabber links list`) work; a bare
JSON array body is rejected with HTTP 500 and JSON-encoded strings for object parameters with HTTP 400, which is why the
CLI sends the `params` envelope with real JSON objects. Mutating endpoints and other JDownloader releases are covered by
the mocked-HTTP and loopback-socket tests only.

## 5. Config file

Connections live in the normal config (`jdr doctor` prints the path):

```json
{
  "defaultProfile": "local",
  "profiles": {
    "local": { "mode": "direct", "host": "127.0.0.1", "port": 3128 },
    "nas":   { "mode": "direct", "host": "nas.lan", "port": 3128, "allowRemote": true },
    "cloud": { "accountEmail": "you@example.com" }
  }
}
```

`mode` is `direct` or absent/`myjd`. `useHttps` and `timeoutSeconds` are optional. Existing My.JDownloader profiles need no
changes.
