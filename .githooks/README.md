# Git hooks

This repo keeps Git hooks under `.githooks/` so they can be committed and shared.

Enable them once:

```bash
git config core.hooksPath .githooks
dotnet tool restore
```

## pre-commit

- Runs `dotnet format` and re-stages any formatting changes before allowing the commit.

