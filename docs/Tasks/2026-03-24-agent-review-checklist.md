# Agent Review Checklist (Loop)

Date: 2026-03-24

## Loop Steps

- [x] Review 10 read-only agents and summarize findings
- [x] Create/refresh actionable checklist items below
- [x] Fix highest-impact incomplete/stubbed commands (keep code files <= 300 LOC; hard cap 500)
- [x] Validate: dotnet build -c Release and dotnet test -c Release
- [x] Validate: pre-commit hook behavior on staged changes
- [ ] Commit with a focused message
- [ ] Re-run agents; stop when no useful findings

## Findings

- [x] Fix `advanced raw request`: remove the fake `--method` flag and show the response payload in human mode.
- [x] Fix `advanced content favicon` description to match the actual `--hoster` input.
- [x] Fix CI publish job: unquoted globbing for `.nupkg/.snupkg` and ensure `setup-dotnet` in the publish job.
- [x] Add SDK pinning with `global.json` and remove the stale `.config/dotnet-tools.json` manifest.
- [x] Reduce oversized code files to <= 300 LOC (hard cap 500): `src/JDownloader.Cli/Transport/MyJdRelay.cs`, `src/JDownloader.Cli/Runtime/HumanDataRenderer.cs`, `src/JDownloader.Cli/Bootstrap/CliApplication.cs`.
- [x] Split bootstrap registration by domain (`captcha`, `events`, `system`, `advanced`) instead of combined file-size buckets.
- [x] Make `advanced raw request` a true escape hatch: reject full relay URLs, preserve raw top-level parameter arrays, and validate dry-run previews.
- [x] Repoint `advanced ingest cnl` to `/flash/addcnl` with real CNL payload input instead of `/flash/add`.
- [x] Replace `captcha forward create-job` positional placeholders with explicit `--arg1..4` flags and clearer provider-specific help.
- [x] Suppress success-path warnings under `--quiet` and align unexpected-failure diagnostic log output with `--verbose`.
- [x] Add regression coverage for request shaping, device sync/resolution, human rendering, preview output, and quiet-mode stderr behavior.
- [x] Fix stale docs for settings config list, extraction settings get, captcha forward create-job, and hook bootstrap steps.
- [x] Make CI honor `global.json`, use a source-controlled package base version, and gate NuGet publish to tags/manual release runs.
- [x] Make the pre-commit hook safe for Windows fresh clones and stage-only formatting (`.gitattributes`, staged-file formatting/restaging only).

