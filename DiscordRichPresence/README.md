# vatSys Discord Rich Presence

A .NET Framework 4.7.2 / x86 MEF plugin for vatSys. All activity text is English.

| vatSys state | Discord details | Discord state |
| --- | --- | --- |
| Disconnected | Idle | Radar on standby |
| Connected controller | Actual callsign, e.g. ZBAA_TWR | Omitted |
| Connected observer (OBS facility) | Watching your six | Eyes on the skies |

The elapsed timer resets when the connection mode or callsign changes. Presence updates within approximately five seconds. Discord can start after vatSys or restart during a session; the plugin reconnects automatically. Closing vatSys closes its RPC connection and removes the activity. `Idle` is activity text, not a change to your Discord account's availability status. OBS refers to the VATSIM observer facility.

## Discord setup

1. Create an application at https://discord.com/developers/applications and name it **vatSys**.
2. Copy its **Application ID** into `ApplicationId` in `presence.json`. No bot token is needed.
3. Export the supplied vatSys `app.ico` to PNG:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\DiscordRichPresence\Export-DiscordAsset.ps1
   ```

4. Upload `DiscordRichPresence\vatsys.png` to your application's **Rich Presence / Art Assets** with the key `vatsys`. Set the application's icon to the same image if desired. Discord cannot use a local ICO path as a presence asset.
5. Run the Discord desktop client and enable activity sharing in its activity privacy settings.

## Build

Use an installed vatSys executable as the API reference. Rebuilding vatSys itself is unnecessary. The repository's default build command uses the supplied local copy at `I:\vatSys\vatSys` when this project is built from the original workspace.

```powershell
dotnet build .\DiscordRichPresence\DiscordRichPresence.csproj -c Release -p:VatSysPath="I:\vatSys\vatSys\bin\vatSys.exe"
```

Change the path to match your installation. This project uses .NET Framework libraries only; it does not require a Discord SDK DLL. Building requires a .NET SDK and .NET Framework 4.7.2 reference assemblies.

## Install

Copy these files from `DiscordRichPresence\bin\Release\net472` into a single subfolder of your profile's `Plugins` directory (for example `Plugins\DiscordRichPresence`), or into the installed application's `bin\Plugins\DiscordRichPresence` directory:

```text
VatSys.DiscordRichPresence.dll
presence.json
app.ico
```

Set the Application ID in the deployed `presence.json`, then restart vatSys. Install one copy of the plugin. If you edit configuration while connected, restart vatSys to apply it immediately; it is also reloaded on reconnection.

Logs: `%LOCALAPPDATA%\VatSys.DiscordRichPresence\presence.log`. Missing Discord is retried quietly. Invalid configuration and Discord RPC errors are logged without interrupting vatSys.

## Verification

```powershell
dotnet run --project .\DiscordRichPresence.Checks\DiscordRichPresence.Checks.csproj
```

The checks use a simulated vatSys API for compile validation and a local named-pipe server for protocol checks. A production build must reference the real `vatSys.exe`. Live Discord display requires your own Application ID and uploaded asset.
