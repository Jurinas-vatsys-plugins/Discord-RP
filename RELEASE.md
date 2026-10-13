# Release checklist

The release archive contains the compiled x86 MEF plugin and the runtime configuration:

```text
VatSys.DiscordRichPresence.dll
presence.json
app.ico
```

Install all three files into:

```text
vatSys\bin\Plugins\DiscordRichPresence
```

The `presence.json` in the release is configured for Discord application `1559498001934975076` and uses the `vatsys` Discord asset key. Restart vatSys after installing the plugin.
