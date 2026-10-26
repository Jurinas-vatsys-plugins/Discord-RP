# vatSys Discord Rich Presence

An x86 .NET Framework 4.7.2 MEF plugin that publishes the current vatSys session to Discord Rich Presence.

## Features

- Shows `Idle` while vatSys is not connected to a network session.
- Shows only the active controller callsign when connected, for example `ZBAA_TWR`.
- Shows `Watching your six` for VATSIM OBS sessions.
- Keeps the elapsed time stable until the connection mode or callsign changes.
- Reconnects automatically when Discord is restarted or its RPC pipe disconnects.
- Uses the supplied vatSys `app.ico` for the Discord activity artwork.
- Runs without a Discord SDK DLL or bot token.

## Discord application setup

The Discord activity title comes from the Discord Developer Portal application name. Set the application name to **vatSys**.

1. Open the application at <https://discord.com/developers/applications>.
2. Set its name to `vatSys`.
3. Confirm that the Application ID in `DiscordRichPresence/presence.json` is correct.
4. Export the supplied icon to a PNG file:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\DiscordRichPresence\Export-DiscordAsset.ps1
   ```

5. Upload `DiscordRichPresence/vatsys.png` under **Rich Presence → Art Assets**.
6. Use `vatsys` as the uploaded asset key.
7. Run the Discord desktop client and enable activity sharing in Discord privacy settings.

Discord cannot use a local `.ico` file directly as a Rich Presence asset. The icon must be uploaded to the Developer Portal.

## Build

The plugin references the vatSys executable because it implements the vatSys plugin API. vatSys itself does not need to be rebuilt.

```powershell
dotnet restore .\DiscordRichPresence\DiscordRichPresence.csproj --ignore-failed-sources
dotnet build .\DiscordRichPresence\DiscordRichPresence.csproj -c Release --no-restore `
  -p:VatSysPath="C:\Program Files (x86)\vatSys\bin\vatSys.exe" `
  -p:AppIconPath="C:\path\to\source-code\vatSys\app.ico"
```

Replace `VatSysPath` with your installed vatSys executable and `AppIconPath` with the vatSys `app.ico`. By default the project looks for `..\..\vatSys\bin\vatSys.exe` and `..\..\source-code\vatSys\app.ico` relative to the project folder. The build requires:

- .NET SDK
- .NET Framework 4.7.2 reference assemblies
- An x86-compatible vatSys executable and its `RossCarlson.Vatsim.Network.dll`

The release helper builds the plugin and creates `vatSys-Discord-RP-v<version>.zip`:

```powershell
powershell -ExecutionPolicy Bypass -File .\build-release.ps1 `
  -VatSysExe "C:\Program Files (x86)\vatSys\bin\vatSys.exe" `
  -AppIcon "C:\path\to\source-code\vatSys\app.ico"
```

## Installation

The release zip mirrors the vatSys installation folder. Extract it directly into the vatSys root, which is the folder that contains `bin\vatSys.exe` (by default `C:\Program Files (x86)\vatSys`). Choose to merge folders if Windows asks. The result is:

```text
<vatSys root>\bin\Plugins\DiscordRichPresence\VatSys.DiscordRichPresence.dll
<vatSys root>\bin\Plugins\DiscordRichPresence\presence.json
<vatSys root>\bin\Plugins\DiscordRichPresence\app.ico
```

vatSys loads every plugin under `bin\Plugins` for all profiles, so one copy covers every profile. Writing to `C:\Program Files (x86)` requires administrator rights.

You can also run `DiscordRichPresence\Install-Plugin.ps1 -VatSysRoot "<vatSys root>"` from a folder containing the built files.

### Profile installation (alternative)

To enable the plugin for one profile only, copy the three files into:

```text
Documents\vatSys Files\Profiles\<Your Profile>\Plugins\DiscordRichPresence
```

Do not install the plugin in both the vatSys root and a profile. vatSys loads profile plugins first and ignores the second copy, so the `presence.json` beside the profile copy is the one that is used. Remove old duplicate folders such as `Discord-RP` before starting vatSys.

Restart vatSys after installation. Start Discord before or after vatSys; the plugin retries the Discord connection automatically.

## Configuration

`presence.json` contains the Discord Application ID and activity text:

```json
{
  "ApplicationId": "1559498001934975076",
  "LargeImageKey": "vatsys",
  "LargeImageText": "vatSys",
  "IdleDetails": "Idle",
  "IdleState": "Radar on standby",
  "ObserverDetails": "Watching your six",
  "ObserverState": "Eyes on the skies"
}
```

The plugin uses the numeric Application ID only. No bot token, client secret, or Discord SDK is required.

## Troubleshooting

The plugin log is written to:

```text
%LOCALAPPDATA%\VatSys.DiscordRichPresence\presence.log
```

If the activity appears briefly and disappears:

1. Close vatSys completely.
2. Remove old `Discord-RP` or duplicate Discord Presence plugin folders.
3. Keep one `DiscordRichPresence` folder, either in `<vatSys root>in\Plugins` or in the active profile's `Plugins` folder.
4. Confirm that Discord Desktop is running and activity sharing is enabled.
5. Start vatSys.

## Verification

Run the protocol and state checks:

```powershell
dotnet run --project .\DiscordRichPresence.Checks\DiscordRichPresence.Checks.csproj
```

The checks cover idle state, controller callsign state, OBS state, elapsed-session handling, Discord IPC framing, fragmented reads, ping/pong, and disconnect handling.

## License

This project is distributed under the GNU General Public License, version 3. See [LICENSE](LICENSE).
