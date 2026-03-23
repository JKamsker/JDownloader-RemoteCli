# Git hooks

This repo keeps Git hooks under `.githooks/` so they can be committed and shared.

Enable them once:

```bash
git config core.hooksPath .githooks
dotnet tool restore
```

## pre-commit

- Expands one-line C# files into a readable multi-line format (via `scripts/format-oneline-csharp.ps1`, when `pwsh` is available).
- Runs `dotnet format` (whitespace + style) and re-stages any formatting changes before allowing the commit.

Notes:

- The committed hook is a Bash script. On Windows, this typically requires Git Bash (or another Bash) for hooks to run.
- `pwsh` is optional; when present it runs the one-line C# expander before `dotnet format`.
