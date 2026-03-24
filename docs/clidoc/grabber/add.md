# `grabber add`

- Root: [index](../index.md)
- Parent: [grabber](index.md)

Add links to the linkgrabber.

## Options

| Name | Aliases | Value | Required | Recursive | Scope | Group | Description | Arguments |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| --assign-job-id | — | flag | No | No | Declared | — | Set AddLinksQuery.assignJobID=true. | — |
| --auto-extract | — | flag | No | No | Declared | — | Set AddLinksQuery.autoExtract=true. | — |
| --autostart | — | flag | No | No | Declared | — | Set AddLinksQuery.autostart=true. | — |
| --deep-decrypt | — | flag | No | No | Declared | — | Set AddLinksQuery.deepDecrypt=true. | — |
| --destination-folder | — | <PATH> | No | No | Declared | — | Optional destination folder override. | PATH · required · arity 1 |
| --device | — | <VALUE> | No | No | Declared | — | Device id or device name override (case-insensitive). | VALUE · required · arity 1 |
| --dry-run | — | flag | No | No | Declared | — | Print the resolved request plan and exit without mutating. | — |
| --json | — | flag | No | No | Declared | — | Emit the default stable JSON envelope contract (v1). | — |
| --links | — | <TEXT> | No | No | Declared | — | Raw newline-separated link text to add. | TEXT · required · arity 1 |
| --no-color | — | flag | No | No | Declared | — | Disable ANSI color output. | — |
| --output | — | <MODE> | No | No | Declared | — | Output mode override: human or json. | MODE · required · arity 1 |
| --overwrite-packagizer-rules | — | flag | No | No | Declared | — | Set AddLinksQuery.overwritePackagizerRules=true. | — |
| --package-name | — | <NAME> | No | No | Declared | — | Optional package name override. | NAME · required · arity 1 |
| --profile | — | <NAME> | No | No | Declared | — | Saved profile to use for auth, defaults, and output settings. | NAME · required · arity 1 |
| --query-json | — | <JSON> | No | No | Declared | — | Raw AddLinksQuery JSON object or @file override. Do not combine with other flags. | JSON · required · arity 1 |
| --quiet | — | flag | No | No | Declared | — | Suppress prompts and non-essential stderr chatter. | — |
| --source-url | — | <URL> | No | No | Declared | — | Optional source URL for provenance. | URL · required · arity 1 |
| --timeout | — | <SECONDS> | No | No | Declared | — | Timeout override in seconds. | SECONDS · required · arity 1 |
| --url | — | <URL> | No | No | Declared | — | Repeatable URL to add to the linkgrabber. | URL · required · arity 1 |
| --verbose | — | flag | No | No | Declared | — | Increase diagnostic detail on stderr. | — |
| --yes | -y | flag | No | No | Declared | — | Skip confirmation prompts for destructive operations. | — |

## Examples

- `grabber add --url https://example.invalid/file`
