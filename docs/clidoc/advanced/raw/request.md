# `advanced raw request`

- Root: [index](../../index.md)
- Parent: [advanced raw](index.md)

Send a raw My.JDownloader endpoint request.

## Arguments

| Name | Required | Arity | Accepted Values | Group | Description |
| --- | --- | --- | --- | --- | --- |
| ENDPOINT | Yes | 1 | — | — | My.JDownloader endpoint path. Example: /downloadsV2/queryLinks. |

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --body-json | — | <JSON> | No | No | Declared | — | Raw body JSON or @file. | JSON · required · arity 1 |
| --destructive | — | flag | No | No | Declared | — | Mark this call as destructive and require confirmation (unless -y/--yes). | — |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --output-file | — | <PATH> | No | No | Declared | — | Destination for binary response modes. | PATH · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --query-json | — | <JSON> | No | No | Declared | — | Raw query JSON or @file. | JSON · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `advanced raw request /downloadsV2/queryLinks --query-json {}`
