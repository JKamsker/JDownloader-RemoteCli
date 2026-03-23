# Git hooks

This repo keeps Git hooks under `.githooks/` so they can be committed and shared.

Enable them once:

```bash
git config core.hooksPath .githooks
dotnet tool restore
```

## pre-commit

- Expands one-line command stubs (via `scripts/format-oneline-csharp.ps1`, when `pwsh` is available).
- Runs `dotnet format` (whitespace + style) and re-stages any formatting changes before allowing the commit.
