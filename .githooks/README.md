# Git hooks

This repo keeps Git hooks under `.githooks/` so they can be committed and shared.

Enable them once:

```bash
git config core.hooksPath .githooks
```

## pre-commit

- Expands one-line staged C# files into a readable multi-line format (via `scripts/format-oneline-csharp.ps1`, when `pwsh` is available).
- Runs `dotnet format` (whitespace + style) only for staged C# files and re-stages those files before allowing the commit.
- Runs `scripts/check-csharp-guidelines.sh --staged` to enforce the C# LOC policy (`<= 500`, warn above `300`), reject non-generated `partial` type declarations, and reject `Foo.Bar.cs`-style filename sharding next to `Foo.cs` unless explicitly excluded.

Notes:

- The committed hook is a Bash script. On Windows, this typically requires Git Bash (or another Bash) for hooks to run.
- `.githooks/*` and `*.sh` are normalized to LF via `.gitattributes` so Bash hooks work in fresh Windows clones.
- `pwsh` is optional; when present it runs the one-line C# expander before `dotnet format`.
- Use `.githooks/loc-limit-exclude.txt` sparingly for justified exceptions; generated files remain ignored automatically.
