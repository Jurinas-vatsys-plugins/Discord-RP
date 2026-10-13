using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VatSys.DiscordRichPresence;

static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var builder = new PresenceBuilder();
var settings = new PresenceSettings();
var idle = builder.Build(settings, false, false, "STALE_TWR");
Require((string)idle["details"] == "Idle", "Disconnected presence leaked the stale callsign.");
var controller = builder.Build(settings, true, false, "ZBAA_TWR");
Require((string)controller["details"] == "ZBAA_TWR" && !controller.ContainsKey("state"), "Controller presence must show only its callsign.");
var observer = builder.Build(settings, true, true, "123_OBS");
Require((string)observer["details"] == "Watching your six", "Observer presence is incorrect.");
var unchanged = builder.Build(settings, true, true, "123_OBS");
Require((long)((Dictionary<string, object>)observer["timestamps"])["start"] ==
    (long)((Dictionary<string, object>)unchanged["timestamps"])["start"], "Unchanged sessions reset elapsed time.");

string name = "vatsys-rpc-check-" + Guid.NewGuid().ToString("N");
using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10));
using var client = new DiscordIpc(name);
Task accept = server.WaitForConnectionAsync(cancel.Token);
client.Connect(1000);
await accept;
Task<(int opcode, string json)> handshakeRead = ReadFrame(server, cancel.Token);
await client.Send(0, "{\"v\":1,\"client_id\":\"123\"}", cancel.Token);
var handshake = await handshakeRead;
Require(handshake.opcode == 0 && handshake.json.Contains("client_id"), "Handshake framing failed.");
var ready = new TaskCompletionSource<bool>();
Task reader = client.ReadLoop(json =>
{
    using var doc = JsonDocument.Parse(json);
    return new Dictionary<string, object> { ["evt"] = doc.RootElement.GetProperty("evt").GetString() };
}, ready, _ => { }, cancel.Token);
await WriteFrame(server, 1, "{\"evt\":\"READY\"}", cancel.Token);
await ready.Task.WaitAsync(cancel.Token);
await WriteFrame(server, 3, "{\"ping\":42}", cancel.Token);
var pong = await ReadFrame(server, cancel.Token);
Require(pong.opcode == 4 && pong.json == "{\"ping\":42}", "Ping was not echoed as pong.");
server.Dispose();
try { await reader; throw new Exception("Disconnect was not detected."); }
catch (EndOfStreamException) { }
Console.WriteLine("PASS: idle, controller, OBS, session timestamps, IPC handshake, fragmented reads, ping/pong, disconnect.");

static async Task<(int opcode, string json)> ReadFrame(Stream stream, CancellationToken token)
{
    byte[] header = new byte[8];
    await stream.ReadExactlyAsync(header, token);
    byte[] body = new byte[BitConverter.ToInt32(header, 4)];
    await stream.ReadExactlyAsync(body, token);
    return (BitConverter.ToInt32(header, 0), Encoding.UTF8.GetString(body));
}

static async Task WriteFrame(Stream stream, int opcode, string json, CancellationToken token)
{
    byte[] body = Encoding.UTF8.GetBytes(json);
    byte[] frame = new byte[8 + body.Length];
    Buffer.BlockCopy(BitConverter.GetBytes(opcode), 0, frame, 0, 4);
    Buffer.BlockCopy(BitConverter.GetBytes(body.Length), 0, frame, 4, 4);
    Buffer.BlockCopy(body, 0, frame, 8, body.Length);
    // Deliberately split the frame to check the client's exact-read behavior.
    for (int i = 0; i < frame.Length; i++) await stream.WriteAsync(frame.AsMemory(i, 1), token);
    await stream.FlushAsync(token);
}
