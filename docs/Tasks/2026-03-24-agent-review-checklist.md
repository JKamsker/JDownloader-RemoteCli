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

