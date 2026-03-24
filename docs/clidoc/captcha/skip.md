# `captcha skip`

- Root: [index](../index.md)
- Parent: [captcha](index.md)

Skip a captcha.

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --id | — | <ID> | No | No | Declared | — | Captcha identifier to skip. | ID · required · arity 1 |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --legacy | — | flag | No | No | Declared | — | Use the deprecated id-only skip overload (no --type). | — |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --type | — | <TYPE> | No | No | Declared | — | Skip request type (required unless --legacy). | TYPE · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `captcha skip --type http`
