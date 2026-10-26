# Release checklist

Build the archive with `build-release.ps1`. The archive mirrors the vatSys installation folder and contains the compiled x86 MEF plugin and its runtime configuration:

```text
bin/Plugins/Discord-RP/Discord-RP.dll
bin/Plugins/Discord-RP/presence.json
bin/Plugins/Discord-RP/app.ico
bin/Plugins/Discord-RP/LICENSE
```

Extract it directly into the vatSys root, the folder that contains `bin\vatSys.exe` (by default `C:\Program Files (x86)\vatSys`). vatSys loads plugins under `bin\Plugins` for every profile, so no per-profile copy is needed.

The `presence.json` in the release is configured for Discord application `1559498001934975076`, uses the `vatsys` Discord asset key, and labels the image `vatSys`. Set the Discord Developer Portal application's name to `vatSys` for the activity title. Remove old `DiscordRichPresence` folders (from 1.0.x) and any profile copy of this plugin, then restart vatSys.
