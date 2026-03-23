# Authentication

`jdr` authenticates against **My.JDownloader** and stores encrypted auth material in a local profile.

## Login

Interactive (prompts for password):

```bash
jdr auth login --email you@example.com
```

Non-interactive (recommended for automation):

```bash
echo "YOUR_PASSWORD" | jdr auth login --email you@example.com --password-stdin --json
```

Notes:

- In `--json` (or `--quiet`) mode, `auth login` **requires** `--password-stdin`.
- Password is read from stdin and is not echoed.

## Status / Whoami / Logout

```bash
jdr auth status
jdr auth whoami
jdr auth logout
```

## Multiple profiles

Create and manage profiles:

```bash
jdr auth profiles list
jdr auth profiles add work
jdr auth profiles use work
```

Then target a profile explicitly:

```bash
jdr --profile work auth status
```

