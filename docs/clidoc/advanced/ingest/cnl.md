# `advanced ingest cnl`

- Root: [index](../../index.md)
- Parent: [advanced ingest](index.md)

Ingest a Click'n'Load payload via /flash/addcnl.

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --cnl-json | — | <JSON> | No | No | Declared | — | Full CNL query object JSON or @file override. | JSON · required · arity 1 |
| --comment | — | <TEXT> | No | No | Declared | — | Optional comment for the ingested package. | TEXT · required · arity 1 |
| --crypted | — | <TEXT> | No | No | Declared | — | Optional crypted CNL payload. | TEXT · required · arity 1 |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dir | — | <PATH> | No | No | Declared | — | Optional destination directory hint. | PATH · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --jk | — | <TEXT> | No | No | Declared | — | Optional Click'n'Load jk value. | TEXT · required · arity 1 |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --key | — | <TEXT> | No | No | Declared | — | Optional Click'n'Load key value. | TEXT · required · arity 1 |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --org-referrer | — | <URL> | No | No | Declared | — | Optional original referrer URL. | URL · required · arity 1 |
| --org-source | — | <TEXT> | No | No | Declared | — | Optional original source label. | TEXT · required · arity 1 |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --package-name | — | <NAME> | No | No | Declared | — | Optional package name override. | NAME · required · arity 1 |
| --password | — | <PASSWORD> | No | No | Declared | — | Repeatable extraction password to attach to the CNL payload. | PASSWORD · required · arity 1 |
| --permission | — | flag | No | No | Declared | — | Set the Click'n'Load permission flag. | — |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --referrer | — | <URL> | No | No | Declared | — | Optional referrer URL. | URL · required · arity 1 |
| --source | — | <TEXT> | No | No | Declared | — | Optional source label sent with the payload. | TEXT · required · arity 1 |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --urls | — | <TEXT> | No | No | Declared | — | Plain-text URLs for the Click'n'Load payload. | TEXT · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `advanced ingest cnl --urls https://example.invalid/file`
