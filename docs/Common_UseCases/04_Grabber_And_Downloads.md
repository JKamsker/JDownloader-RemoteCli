# Grabber and Downloads

JDownloader has two main lists:

- **Grabber** (Linkgrabber): staged links/packages before you move them into downloads
- **Downloads**: the active download list

## Inspect grabber items

```bash
jdr grabber packages list
jdr grabber links list
```

## Variants

```bash
# List variants for a specific linkgrabber link
jdr grabber variants list --link-id <ID>

# Set a variant
jdr grabber variants set --link-id <ID> --variant-id <VARIANT_ID>
```

## Move items into downloads

```bash
jdr grabber move-to-downloads --package-id <ID>
```

## Downloads: status and link/package listing

```bash
jdr downloads status
jdr downloads links list
jdr downloads packages list
```

## Destructive operations (remove)

These commands require confirmation unless you pass `--yes`, or you can preview with `--dry-run`.

```bash
jdr downloads links remove --link-id <ID> --dry-run
jdr downloads links remove --link-id <ID> --yes
```

