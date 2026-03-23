# JDownloader Remote CLI (`jdr`)

[![NuGet](https://img.shields.io/nuget/v/JDownloader-RemoteCli.svg)](https://www.nuget.org/packages/JDownloader-RemoteCli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/JDownloader-RemoteCli.svg)](https://www.nuget.org/packages/JDownloader-RemoteCli)
[![CI](https://github.com/JKamsker/JDownloader-RemoteCli/actions/workflows/build.yml/badge.svg)](https://github.com/JKamsker/JDownloader-RemoteCli/actions/workflows/build.yml)

A human-first command-line interface for **My.JDownloader** (remote control for JDownloader), built with .NET 10 and Spectre.Console.Cli.

- Default output is **human-friendly** (tables / concise lines).
- Use `--json` for **stable machine-readable** output.

## Installation

```bash
dotnet tool install -g JDownloader-RemoteCli
```

Upgrade:

```bash
dotnet tool update -g JDownloader-RemoteCli
```

## Quick Start

```bash
# 1) Login (stores encrypted auth material in a local profile)
echo "YOUR_PASSWORD" | jdr auth login --email you@example.com --password-stdin

# 2) Pick a device (or set a default in your profile)
jdr device list
jdr device use --device "Your JDownloader Device Name"

# 3) Inspect current state
jdr downloads status
jdr grabber packages list
jdr grabber links list

# 4) Scriptable output
jdr grabber links list --json
```

## Commands

| Group | What it does |
|-------|--------------|
| `auth` | Login/logout/status, and profile management |
| `device` | Discover/select JDownloader devices |
| `downloads` | Status + link/package operations on the download list |
| `grabber` | Linkgrabber ingestion/staging (links, packages, variants) |
| `accounts` | Premium accounts + basic auth management |
| `extraction` | Inspect/control archive extraction |
| `settings` | Config, plugins, extensions |
| `captcha` | Captcha jobs |
| `events` | Event subscriptions/listen/poll |
| `system` | JDownloader + OS + update operations |
| `advanced` | Expert escape hatches (raw request, UI ops, dialogs, ingest) |
| `doctor` | Inspect config paths and resolution |

Run `jdr --help` or `jdr <group> --help` for full details.

## Global Options (Selected)

| Option | Description |
|--------|-------------|
| `--profile <NAME>` | Saved profile to use for auth/defaults |
| `--device <VALUE>` | Override device id/name resolution |
| `--json` | Emit the stable JSON envelope (v1) |
| `--output <human\|json>` | Output mode override |
| `--verbose` | More diagnostic detail on stderr |
| `--quiet` | Suppress prompts and non-essential stderr chatter |
| `--dry-run` | Show request plan and exit (no mutations) |
| `-y, --yes` | Skip confirmation prompts for destructive operations |

Environment variables that influence defaults include: `JD2_PROFILE`, `JD2_DEVICE`, `JD2_OUTPUT`, `JD2_TIMEOUT`, `JD2_CONFIG`, `JD2_KEYFILE`.

## Docs

Common workflow guides:

- Authentication: `docs/Common_UseCases/01_Authentication.md`
- Profiles & config: `docs/Common_UseCases/02_Profiles_And_Config.md`
- Devices: `docs/Common_UseCases/03_Devices.md`
- Grabber + downloads: `docs/Common_UseCases/04_Grabber_And_Downloads.md`
- Scripting & automation: `docs/Common_UseCases/05_Scripting_And_Automation.md`

## Building from Source

```bash
git clone https://github.com/JKamsker/JDownloader-RemoteCli.git
cd JDownloader-RemoteCli
dotnet test -c Release
dotnet run --project src/JDownloader.Cli -- doctor
```

## Dev: formatting + hooks

This repo uses `dotnet format`. To enable the committed pre-commit hook:

```bash
git config core.hooksPath .githooks
dotnet tool restore
```
