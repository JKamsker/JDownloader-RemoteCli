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

The API accepts 4 string parameters for RecaptchaV2 job creation:

```bash
jdr captcha forward create-job <PARAM1> <PARAM2> <PARAM3> <PARAM4>
```

Fetch a forward result by job id:

```bash
jdr captcha forward get-result --job-id 123
```

