# System

System commands control JDownloader itself and some OS-level actions.

Most `system` actions are destructive and will require confirmation unless `--yes` is provided.

## Info and storage

```bash
jdr system info
jdr system storage
```

## JDownloader operations

```bash
jdr system jd version
jdr system jd revision
jdr system jd uptime
jdr system jd refresh-plugins
jdr system jd restart
jdr system jd exit
```

## Update operations

```bash
jdr system update check
jdr system update run
jdr system update restart
```

## OS operations

```bash
jdr system os shutdown --force
jdr system os hibernate
jdr system os standby
```

