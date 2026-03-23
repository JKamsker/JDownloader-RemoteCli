# Profiles and Config

Profiles hold defaults (like output mode and device selection) and stored account/device metadata.

## Resolution order (high level)

Most commands resolve settings in this order:

1. Flags (for example `--profile`, `--device`, `--json`)
2. Environment variables (for example `JD2_PROFILE`, `JD2_DEVICE`, `JD2_OUTPUT`)
3. Saved profile config
4. Defaults / safe inference

## Config locations

Run:

```bash
jdr doctor
```

Or in JSON:

```bash
jdr doctor --json
```

Environment variables:

- `JD2_CONFIG`: config root override
- `JD2_KEYFILE`: key file override (credential protector key material)

Defaults by OS:

- Windows: `%APPDATA%\\jd2\\`
- macOS: `~/Library/Application Support/jd2/`
- Linux: `$XDG_CONFIG_HOME/jd2/` or `~/.config/jd2/`

## Output defaults

Human output is the default. For scripting:

```bash
jdr downloads status --json
```

To persist a default output mode per profile, set it in your profile record (or use `JD2_OUTPUT=json`).

