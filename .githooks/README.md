# Git hooks

This repo keeps Git hooks under `.githooks/` so they can be committed and shared.

Enable them once:

```bash
git config core.hooksPath .githooks
```

## pre-commit

- Expands one-line staged C# files into a readable multi-line format (via `scripts/format-oneline-csharp.ps1`, when `pwsh` is available).
- Runs `dotnet format` (whitespace + style) only for staged C# files and re-stages those files before allowing the commit.

Notes:

- The committed hook is a Bash script. On Windows, this typically requires Git Bash (or another Bash) for hooks to run.
- `.githooks/*` and `*.sh` are normalized to LF via `.gitattributes` so Bash hooks work in fresh Windows clones.
- `pwsh` is optional; when present it runs the one-line C# expander before `dotnet format`.
