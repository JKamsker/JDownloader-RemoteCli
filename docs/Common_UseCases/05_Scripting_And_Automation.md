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
  "meta": {
    "schemaVersion": 1
  }
}
```

Errors set `"ok": false` and populate `"error"` with `"kind"`, `"message"`, and optionally `"recovery"`.

Notes:

- `data` is omitted when it is null.
- `error` is omitted when it is null.
- `meta.warnings` and `meta.diagnosticLogPath` are omitted when null.

## Stdout vs stderr

- Human output: primary output goes to stdout; warnings and errors go to stderr.
- JSON output: the JSON envelope is written to stdout; avoid mixing additional text into stdout.

## Exit codes

Common exit codes:

- `0`: success
- `1`: unexpected client error
- `2`: usage / validation error
- `3`: not authenticated
- `5`: not found
- `6`: conflict
- `8`: transport error (remote call failed)
- `10`: cancelled (user declined confirmation)

Ambiguous selectors (for example a device name matching multiple devices) currently surface as a usage/validation error (`2`).
