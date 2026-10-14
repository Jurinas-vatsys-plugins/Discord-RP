using System;
using System.Collections.Generic;

namespace VatSys.DiscordRichPresence
{
    public sealed class PresenceSettings
    {
        public string ApplicationId { get; set; } = "";
        public string LargeImageKey { get; set; } = "vatsys";
        public string LargeImageText { get; set; } = "vatSys";
        public string IdleDetails { get; set; } = "Idle";
        public string IdleState { get; set; } = "Radar on standby";
        public string ObserverDetails { get; set; } = "Watching your six";
        public string ObserverState { get; set; } = "Eyes on the skies";
    }

    public sealed class PresenceBuilder
    {
        private string sessionKey;
        private long started;

        public Dictionary<string, object> Build(PresenceSettings settings, bool connected,
            bool observer, string callsign)
        {
            string key = !connected ? "idle" : (observer ? "observer:" : "control:") + callsign;
            if (key != sessionKey)
            {
                sessionKey = key;
                started = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            var activity = new Dictionary<string, object>
            {
                ["details"] = Limit(!connected ? settings.IdleDetails : observer
                    ? settings.ObserverDetails : callsign),
                ["timestamps"] = new Dictionary<string, object> { ["start"] = started },
                ["assets"] = new Dictionary<string, object>
                {
                    ["large_image"] = Limit(settings.LargeImageKey),
                    ["large_text"] = Limit(settings.LargeImageText)
                }
            };
            // The active control position is deliberately just the callsign.
            if (!connected || observer)
                activity["state"] = Limit(connected ? settings.ObserverState : settings.IdleState);
            return activity;
        }

        private static string Limit(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "vatSys";
            value = value.Trim();
            if (value.Length <= 128) return value;
            int length = char.IsHighSurrogate(value[127]) ? 127 : 128;
            return value.Substring(0, length);
        }
    }
}
