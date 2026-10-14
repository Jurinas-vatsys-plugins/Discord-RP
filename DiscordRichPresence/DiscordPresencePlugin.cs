using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using vatsys;
using vatsys.Plugin;

namespace VatSys.DiscordRichPresence
{
    [Export(typeof(IPlugin))]
    public sealed class DiscordPresencePlugin : IPlugin, IDisposable
    {
        private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
        private readonly PresenceBuilder builder = new PresenceBuilder();
        private readonly string folder = Path.GetDirectoryName(typeof(DiscordPresencePlugin).Assembly.Location);
        private DiscordIpc current;
        private readonly object connectionLock = new object();
        private int disposed;

        public string Name => "vatSys";

        public DiscordPresencePlugin()
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            Log("Plugin loaded from " + typeof(DiscordPresencePlugin).Assembly.Location);
            Task.Run(Run);
        }

        public void OnFDRUpdate(FDP2.FDR updated) { }
        public void OnRadarTrackUpdate(RDP.RadarTrack updated) { }

        private async Task Run()
        {
            CancellationToken token = shutdown.Token;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var serializer = new JavaScriptSerializer();
                    var settings = serializer.Deserialize<PresenceSettings>(File.ReadAllText(Path.Combine(folder, "presence.json")));
                    if (settings == null || string.IsNullOrWhiteSpace(settings.ApplicationId) ||
                        !settings.ApplicationId.All(char.IsDigit))
                        throw new InvalidOperationException("Set ApplicationId in presence.json to your Discord application's numeric ID.");
                    Log("Configuration loaded for application " + settings.ApplicationId);

                    for (int index = 0; index < 10 && !token.IsCancellationRequested; index++)
                    {
                        using (var ipc = new DiscordIpc("discord-ipc-" + index))
                        {
                            try { ipc.Connect(500); }
                            catch (TimeoutException) { continue; }
                            catch (IOException) { continue; }
                            Log("Connected to discord-ipc-" + index);
                            lock (connectionLock)
                            {
                                token.ThrowIfCancellationRequested();
                                current = ipc;
                            }
                            await RunConnection(ipc, settings, serializer, token).ConfigureAwait(false);
                            break;
                        }
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch (Exception ex) { Log(ex.ToString()); }
                finally { lock (connectionLock) current = null; }
                try { await Task.Delay(5000, token).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task RunConnection(DiscordIpc ipc, PresenceSettings settings,
            JavaScriptSerializer serializer, CancellationToken token)
        {
            var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var decoder = new JavaScriptSerializer();
            Task reader = ipc.ReadLoop(json => decoder.Deserialize<Dictionary<string, object>>(json), ready, Log, token);
            try
            {
                await ipc.Send(0, serializer.Serialize(new { v = 1, client_id = settings.ApplicationId }), token).ConfigureAwait(false);
                Task first = await Task.WhenAny(ready.Task, reader, Task.Delay(10000, token)).ConfigureAwait(false);
                if (first == reader) await reader.ConfigureAwait(false);
                if (first != ready.Task) throw new IOException("Discord RPC handshake timed out.");
                await ready.Task.ConfigureAwait(false);
                Log("Discord RPC ready.");
                string previous = null;
                while (!token.IsCancellationRequested)
                {
                    if (reader.IsCompleted) await reader.ConfigureAwait(false);
                    bool connected = false;
                    bool observer = false;
                    string callsign = null;
                    try
                    {
                        connected = Network.IsConnected;
                        callsign = connected ? Network.Callsign : null;
                        observer = connected &&
                            string.Equals(Network.Facility.ToString(), "OBS", StringComparison.OrdinalIgnoreCase);
                    }
                    catch (Exception ex) { Log("Could not read vatSys network state: " + ex.Message); }
                    var activity = builder.Build(settings, connected, observer, callsign);
                    string signature = serializer.Serialize(activity);
                    if (signature != previous)
                    {
                        await ipc.Send(1, serializer.Serialize(new
                        {
                            cmd = "SET_ACTIVITY",
                            args = new { pid = Process.GetCurrentProcess().Id, activity },
                            nonce = Guid.NewGuid().ToString("N")
                        }), token).ConfigureAwait(false);
                        Log("Presence sent: " + signature);
                        previous = signature;
                    }
                    await Task.WhenAny(reader, Task.Delay(5000, token)).ConfigureAwait(false);
                }
            }
            finally
            {
                Log("Discord connection ended.");
                ipc.Dispose();
                try { await reader.ConfigureAwait(false); }
                catch (Exception) { /* Closing the pipe releases its pending read. */ }
            }
        }

        private void Log(string message)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VatSys.DiscordRichPresence");
                Directory.CreateDirectory(path);
                string file = Path.Combine(path, "presence.log");
                if (File.Exists(file) && new FileInfo(file).Length > 1024 * 1024) File.Delete(file);
                File.AppendAllText(file, DateTime.UtcNow.ToString("O") + " " + message + Environment.NewLine);
            }
            catch (Exception) { /* Presence logging must never interrupt the host. */ }
        }

        private void OnExit(object sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            AppDomain.CurrentDomain.ProcessExit -= OnExit;
            Log("Plugin disposed.");
            shutdown.Cancel();
            lock (connectionLock) current?.Dispose();
        }
    }
}
