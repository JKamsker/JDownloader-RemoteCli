# Accounts

Accounts are managed per resolved device (your running JDownloader instance).

## List accounts

```bash
jdr accounts list
```

## Add an account

```bash
jdr accounts add --hoster "example.com" --username "me" --password-stdin
```

## Enable/disable/refresh/remove by id

```bash
jdr accounts disable --account-id 123 --account-id 456
jdr accounts enable --account-id 123
jdr accounts refresh --account-id 123
jdr accounts remove --account-id 123
```

## Premium hosters

List known hosters:

```bash
jdr accounts hosters list
```

Resolve a hoster to its URL:

```bash
jdr accounts hosters url --hoster "example.com"
```

List URLs:

```bash
jdr accounts hosters urls
```

## Basic auth entries

```bash
jdr accounts basic-auth list
jdr accounts basic-auth add --type http --hostmask "example.com" --username "me" --password-stdin
```
