# Release checklist

The release archive contains the compiled x86 MEF plugin and the runtime configuration. Install it under the active profile, not in multiple locations:

```text
VatSys.DiscordRichPresence.dll
presence.json
app.ico
```

Install all three files into:

```text
Documents\vatSys Files\Profiles\<Your Profile>\Plugins\DiscordRichPresence
```

The `presence.json` in the release is configured for Discord application `1559498001934975076` and uses the `vatsys` Discord asset key. Remove old `Discord-RP` copies and restart vatSys after installing the plugin.
