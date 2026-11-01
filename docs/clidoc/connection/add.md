# `connection add`

- Root: [index](../index.md)
- Parent: [connection](index.md)

Create or update a connection (--mode direct|myjd).

## Arguments

| Name | Required | Arity | Accepted Values | Group | Description |
| --- | --- | --- | --- | --- | --- |
| NAME | Yes | 1 | — | — | Connection (profile) name to create or update. |

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --allow-remote | — | flag | No | No | Declared | — | Direct mode: explicitly allow a non-loopback host. The local API is unauthenticated; see the security notes. | — |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --force | — | flag | No | No | Declared | — | Replace the connection settings of an existing profile. | — |
| --host | — | <HOST> | No | No | Declared | — | Direct mode: hostname or IP of the JDownloader machine. Default: 127.0.0.1. | HOST · required · arity 1 |
| --https | — | flag | No | No | Declared | — | Direct mode: use https (only when JDownloader sits behind a TLS reverse proxy). | — |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --mode | — | <MODE> | No | No | Declared | — | Connection mode: 'direct' (local JDownloader HTTP API, no account) or 'myjd' (My.JDownloader cloud). Default: myjd. | MODE · required · arity 1 |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --port | — | <PORT> | No | No | Declared | — | Direct mode: port of JDownloader's local Remote API. Default: 3128. | PORT · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `connection add local --mode direct --host 127.0.0.1 --port 3128`
