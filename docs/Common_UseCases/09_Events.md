# Events

Events let you subscribe to JDownloader event streams and then listen for changes.

## Publishers

```bash
jdr events publishers
```

## Subscribe

```bash
jdr events subscribe --subscription "downloadsV2" --subscription "linkgrabberv2"
```

The response includes a `subscriptionid` which you use for status/listen.

## Status and listen

```bash
jdr events status --subscription-id 123
jdr events listen --subscription-id 123
```

