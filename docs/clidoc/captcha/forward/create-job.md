# `captcha forward create-job`

- Root: [index](../../index.md)
- Parent: [captcha forward](index.md)

Create a provider-specific RecaptchaV2 captcha forward job.

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --arg1 | — | <TEXT> | No | No | Declared | — | Provider-specific RecaptchaV2 argument 1. | TEXT · required · arity 1 |
| --arg2 | — | <TEXT> | No | No | Declared | — | Provider-specific RecaptchaV2 argument 2. | TEXT · required · arity 1 |
| --arg3 | — | <TEXT> | No | No | Declared | — | Provider-specific RecaptchaV2 argument 3. | TEXT · required · arity 1 |
| --arg4 | — | <TEXT> | No | No | Declared | — | Provider-specific RecaptchaV2 argument 4. | TEXT · required · arity 1 |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `captcha forward create-job --arg1 example`
