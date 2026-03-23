# Scripting and Automation

## JSON output

Use `--json` to emit a stable envelope:

```bash
jdr grabber links list --json
```

Shape (v1):

```json
{
  "ok": true,
  "data": { },
  "error": null,
  "meta": {
    "schemaVersion": 1,
    "warnings": [],
    "diagnosticLogPath": null
  }
}
```

Errors set `"ok": false` and populate `"error"` with `"kind"`, `"message"`, and optionally `"recovery"`.

## Stdout vs stderr

- Human output: primary output goes to stdout; warnings and errors go to stderr.
- JSON output: the JSON envelope is written to stdout; avoid mixing additional text into stdout.

## Exit codes

Common exit codes:

- `0`: success
- `2`: usage / validation error
- `3`: not authenticated
- `5`: not found
- `6`: conflict / ambiguous resolution
- `8`: transport error (remote call failed)

