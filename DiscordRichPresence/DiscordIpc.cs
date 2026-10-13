using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VatSys.DiscordRichPresence
{
    // Discord's local RPC framing: little-endian opcode, byte length, UTF-8 JSON.
    public sealed class DiscordIpc : IDisposable
    {
        private readonly NamedPipeClientStream pipe;
        private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
        public DiscordIpc(string name)
        {
            pipe = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public void Connect(int timeout) => pipe.Connect(timeout);

        public async Task Send(int opcode, string json, CancellationToken token)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            byte[] header = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(opcode), 0, header, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(body.Length), 0, header, 4, 4);
            await writeLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                await pipe.WriteAsync(header, 0, header.Length, token).ConfigureAwait(false);
                await pipe.WriteAsync(body, 0, body.Length, token).ConfigureAwait(false);
                await pipe.FlushAsync(token).ConfigureAwait(false);
            }
            finally { writeLock.Release(); }
        }

        public async Task ReadLoop(Func<string, Dictionary<string, object>> decode,
            TaskCompletionSource<bool> ready, Action<string> error, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] header = new byte[8];
                    await ReadExactly(header, token).ConfigureAwait(false);
                    int opcode = BitConverter.ToInt32(header, 0);
                    int length = BitConverter.ToInt32(header, 4);
                    if (length < 0 || length > 1024 * 1024) throw new IOException("Invalid Discord frame size.");
                    byte[] body = new byte[length];
                    await ReadExactly(body, token).ConfigureAwait(false);
                    string json = Encoding.UTF8.GetString(body);
                    if (opcode == 2) throw new IOException("Discord closed the RPC connection: " + json);
                    if (opcode == 3)
                    {
                        await Send(4, json, token).ConfigureAwait(false);
                        continue;
                    }
                    if (opcode != 1) continue;
                    var message = decode(json);
                    if (message.TryGetValue("evt", out object evt))
                    {
                        if (Equals(evt, "READY")) ready.TrySetResult(true);
                        if (Equals(evt, "ERROR")) error("Discord RPC error: " + json);
                    }
                }
            }
            finally { ready.TrySetCanceled(); }
        }

        private async Task ReadExactly(byte[] buffer, CancellationToken token)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = await pipe.ReadAsync(buffer, offset, buffer.Length - offset, token).ConfigureAwait(false);
                if (read == 0) throw new EndOfStreamException("Discord disconnected.");
                offset += read;
            }
        }

        public void Dispose() => pipe.Dispose();
    }
}
