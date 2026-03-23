# Devices

Most commands require a resolved **device** (your running JDownloader instance connected to My.JDownloader).

## List devices

```bash
jdr device list
```

This command will try to sync the device list when possible (and fall back to cached known devices if offline).

## Select a device for future commands

```bash
jdr device use --device "My Laptop"
```

You can also override per-call:

```bash
jdr downloads status --device "My Laptop"
```

Or via environment variable:

```bash
export JD2_DEVICE="My Laptop"
```
