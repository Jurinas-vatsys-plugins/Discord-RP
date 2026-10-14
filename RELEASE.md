# Release checklist

The release archive contains the compiled x86 MEF plugin and the runtime configuration. Install it under the active profile, not in multiple locations:

```text
VatSys.DiscordRichPresence.dll
presence.json
app.ico
LICENSE
```

Install all three files into:

```text
Documents\vatSys Files\Profiles\<Your Profile>\Plugins\DiscordRichPresence
```

The `presence.json` in the release is configured for Discord application `1559498001934975076`, uses the `vatsys` Discord asset key, and labels the image `vatSys`. Set the Discord Developer Portal application's name to `vatSys` for the activity title. Remove old `Discord-RP` copies and restart vatSys after installing the plugin.
