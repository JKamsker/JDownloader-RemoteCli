# Captcha

Captcha commands help you inspect and answer captcha challenges.

## List and inspect jobs

```bash
jdr captcha list
jdr captcha get --id 123
jdr captcha job --id 123
```

## Solve or skip

```bash
jdr captcha solve --id 123 --result "answer"
jdr captcha skip --id 123 --type "SKIPPED"
```

## Captcha forward

The upstream My.JDownloader API documentation lists `createJobRecaptchaV2` as **4 unnamed string parameters**. Their meaning depends on your captcha-forward setup/provider.

`jdr` exposes them as explicit `--arg1..4` flags and passes them through unchanged **in order**.

```bash
jdr captcha forward create-job --arg1 "..." --arg2 "..." --arg3 "..." --arg4 "..."
```

If you prefer explicit naming, you can use the raw escape hatch and provide `arg1..arg4` yourself:

```bash
jdr advanced raw request /captchaforward/createJobRecaptchaV2 --query-json '{"arg1":"...","arg2":"...","arg3":"...","arg4":"..."}'
```

Fetch a forward result by job id:

```bash
jdr captcha forward get-result --job-id 123
```
