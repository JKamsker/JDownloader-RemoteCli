# Settings

Settings are exposed via three command families: `settings config`, `settings plugins`, and `settings extensions`.

## Config entries

List interface keys:

```bash
jdr settings config list --interface-name "org.jdownloader.settings.GeneralSettings"
```

Get/set/reset a key:

```bash
jdr settings config get --interface-name "org.jdownloader.settings.GeneralSettings" --key "MaxSimultanDownloads"
jdr settings config set --interface-name "org.jdownloader.settings.GeneralSettings" --key "MaxSimultanDownloads" --value-json 5
jdr settings config reset --interface-name "org.jdownloader.settings.GeneralSettings" --key "MaxSimultanDownloads"
```

## Plugins

```bash
jdr settings plugins list
jdr settings plugins get --classname "jd.plugins.hoster.ExampleCom"
```

## Extensions

```bash
jdr settings extensions list
jdr settings extensions get --classname "org.jdownloader.extensions.extraction.ExtractionExtension"
jdr settings extensions install --id "some-extension-id"
jdr settings extensions enable --classname "org.jdownloader.extensions.extraction.ExtractionExtension"
jdr settings extensions disable --classname "org.jdownloader.extensions.extraction.ExtractionExtension"
```

