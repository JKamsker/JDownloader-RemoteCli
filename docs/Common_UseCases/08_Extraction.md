# Extraction

Extraction commands control archive extraction on the resolved device.

## Queue and status

```bash
jdr extraction queue
jdr extraction info
```

## Start/cancel

```bash
jdr extraction start --package-id 123
jdr extraction cancel --controller-id 123
```

## Add a password

```bash
jdr extraction add-password --password-stdin
```

## Settings

```bash
jdr extraction settings get --archive-id "some-archive-id"
jdr extraction settings set --archive-id "some-archive-id" --settings-json '{"someSetting":true}'
```
