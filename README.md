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
  -p:VatSysPath="I:\vatSys\vatSys\bin\vatSys.exe"
```

Replace the `VatSysPath` value with the location of your installed vatSys executable. The build requires:

- .NET SDK
- .NET Framework 4.7.2 reference assemblies
- An x86-compatible vatSys executable and its `RossCarlson.Vatsim.Network.dll`

The release helper uses the supplied development copy automatically:

```powershell
powershell -ExecutionPolicy Bypass -File .\build-release.ps1
```

## Installation

Install the plugin into the profile that you use in vatSys:

```text
Documents\vatSys Files\Profiles\<Your Profile>\Plugins\DiscordRichPresence
```

Copy these files into that directory:

```text
VatSys.DiscordRichPresence.dll
presence.json
app.ico
```

Install one copy per profile. If you use multiple profiles, copy the directory to each profile's `Plugins` folder.

Do not install the same plugin in both a profile directory and the application-wide directory. Remove old duplicate folders such as `Discord-RP` before starting vatSys.

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
3. Keep one `DiscordRichPresence` folder in the active profile.
4. Confirm that Discord Desktop is running and activity sharing is enabled.
5. Start vatSys and select the profile containing the plugin.

## Verification

Run the protocol and state checks:

```powershell
dotnet run --project .\DiscordRichPresence.Checks\DiscordRichPresence.Checks.csproj
```

The checks cover idle state, controller callsign state, OBS state, elapsed-session handling, Discord IPC framing, fragmented reads, ping/pong, and disconnect handling.

## License

This project is distributed under the GNU General Public License, version 3. See [LICENSE](LICENSE).
