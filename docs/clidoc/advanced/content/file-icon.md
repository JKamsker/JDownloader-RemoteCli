# `advanced content file-icon`

- Root: [index](../../index.md)
- Parent: [advanced content](index.md)

Fetch a file icon by filename (binary).

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --filename | — | <NAME> | No | No | Declared | — | Filename to fetch an icon for. | NAME · required · arity 1 |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --output-file | — | <PATH> | No | No | Declared | — | Destination file for the binary response. | PATH · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `advanced content file-icon --filename demo`
