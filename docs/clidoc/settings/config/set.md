# `settings config set`

- Root: [index](../../index.md)
- Parent: [settings config](index.md)

Set a config entry.

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --interface-name | — | <NAME> | No | No | Declared | — | Config interface name. | NAME · required · arity 1 |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --key | — | <KEY> | No | No | Declared | — | Config key to set. | KEY · required · arity 1 |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --storage | — | <NAME> | No | No | Declared | — | Config storage name. Omit for entries without a dedicated storage. | NAME · required · arity 1 |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --value | — | <VALUE> | No | No | Declared | — | String value to set. | VALUE · required · arity 1 |
| --value-json | — | <JSON> | No | No | Declared | — | Raw JSON value or @file for non-string values. | JSON · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `settings config set --interface-name GeneralSettings --key downloadFolder --value /downloads`
