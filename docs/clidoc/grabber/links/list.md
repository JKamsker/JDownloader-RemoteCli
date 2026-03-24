# `grabber links list`

- Root: [index](../../index.md)
- Parent: [grabber links](index.md)

List linkgrabber links.

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --fields | — | <CSV> | No | No | Declared | — | Comma-separated field projection for query-style endpoints. | CSV · required · arity 1 |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --limit | — | <NUMBER> | No | No | Declared | — | Maximum number of results. | NUMBER · required · arity 1 |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --offset | — | <NUMBER> | No | No | Declared | — | Result offset. | NUMBER · required · arity 1 |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --package-id | — | <ID> | No | No | Declared | — | Repeatable package identifier filter. | ID · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --query-json | — | <JSON> | No | No | Declared | — | Raw query object JSON or @file override. | JSON · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `grabber links list --package-id 12345`
