# Discord-RP

A .NET Framework 4.7.2 / x86 MEF plugin for vatSys. All activity text is English.

| vatSys state | Discord details | Discord state |
| --- | --- | --- |
| Disconnected | Idle | Radar on standby |
| Connected controller | Actual callsign, e.g. ZBAA_TWR | Omitted |
| Connected observer (OBS facility) | Watching your six | Eyes on the skies |

The elapsed timer resets when the connection mode or callsign changes. Presence updates within approximately five seconds. Discord can start after vatSys or restart during a session; the plugin reconnects automatically. Closing vatSys closes its RPC connection and removes the activity. `Idle` is activity text, not a change to your Discord account's availability status. OBS refers to the VATSIM observer facility.

## Discord setup

1. In https://discord.com/developers/applications, set the Discord application's name to **vatSys**. Discord displays the Developer Portal application name as the activity title; the plugin cannot rename that application through RPC.
2. Copy its **Application ID** into `ApplicationId` in `presence.json`. No bot token is needed.
3. Export the supplied vatSys `app.ico` to PNG:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\Discord-RP\Export-DiscordAsset.ps1
   ```

4. Upload `Discord-RP\vatsys.png` to your application's **Rich Presence / Art Assets** with the key `vatsys`. Set the application's icon to the same image if desired. Discord cannot use a local ICO path as a presence asset.
5. Run the Discord desktop client and enable activity sharing in its activity privacy settings.

## Build

Use an installed vatSys executable as the API reference. Rebuilding vatSys itself is unnecessary. By default the project looks for `..\..\vatSys\bin\vatSys.exe` and the icon at `..\..\source-code\vatSys\app.ico`, relative to this folder.

```powershell
dotnet build .\Discord-RP\Discord-RP.csproj -c Release -p:VatSysPath="C:\Program Files (x86)\vatSys\bin\vatSys.exe"
```

Change the path to match your installation. This project uses .NET Framework libraries only; it does not require a Discord SDK DLL. Building requires a .NET SDK and .NET Framework 4.7.2 reference assemblies.

## Install

Extract the release zip directly into the vatSys root, the folder that contains `bin\vatSys.exe` (by default `C:\Program Files (x86)\vatSys`). The zip already contains the `bin\Plugins\Discord-RP` folder, and vatSys loads plugins there for every profile. If you build it yourself, copy these files from `Discord-RP\bin\Release\net472` into `<vatSys root>\bin\Plugins\Discord-RP`:

```text
Discord-RP.dll
presence.json
app.ico
```

To enable it for a single profile instead, use `Documents\vatSys Files\Profiles\<Your Profile>\Plugins\Discord-RP`. Install only one copy; do not place the same DLL in both the profile and application `bin\Plugins` directories.

Set the Application ID in the deployed `presence.json`, then restart vatSys. Remove any old `DiscordRichPresence` folder from 1.0.x; its DLL has a different name and would load as a second plugin. If you edit configuration while connected, restart vatSys to apply it immediately; it is also reloaded on reconnection.

Logs: `%LOCALAPPDATA%\Discord-RP\presence.log`. Missing Discord is retried quietly. Invalid configuration and Discord RPC errors are logged without interrupting vatSys.

## Verification

```powershell
dotnet run --project .\Discord-RP.Checks\Discord-RP.Checks.csproj
```

The checks use a simulated vatSys API for compile validation and a local named-pipe server for protocol checks. A production build must reference the real `vatSys.exe`. Live Discord display requires your own Application ID and uploaded asset.
