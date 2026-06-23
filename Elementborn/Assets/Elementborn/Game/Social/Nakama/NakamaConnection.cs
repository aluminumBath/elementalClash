#if ELEMENTBORN_NAKAMA
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

namespace Elementborn.Game.Social.NakamaNet
{
    /// <summary>
    /// Owns the Nakama client, session, and realtime socket. Configure host/port/key, then call
    /// <see cref="ConnectAsync"/> once at startup. Device auth gives a stable per-install identity; replace it
    /// with a real sign-in as needed. Adapters read <see cref="Client"/>/<see cref="Session"/>/<see cref="Socket"/>
    /// and tolerate a not-yet-connected state (empty reads, skipped writes), so they can be constructed before
    /// the connection completes.
    /// </summary>
    public sealed class NakamaConnection
    {
        public IClient Client { get; private set; }
        public ISession Session { get; private set; }
        public ISocket Socket { get; private set; }

        public bool Connected => Session != null && Socket != null && Socket.IsConnected;
        public string UserId => Session != null ? Session.UserId : null;
        public string Username => Session != null ? Session.Username : null;

        private readonly string _scheme;
        private readonly string _host;
        private readonly int _port;
        private readonly string _serverKey;

        public NakamaConnection(string scheme = "http", string host = "127.0.0.1", int port = 7350, string serverKey = "defaultkey")
        {
            _scheme = scheme; _host = host; _port = port; _serverKey = serverKey;
        }

        public async Task ConnectAsync(string deviceId)
        {
            Client = new Client(_scheme, _host, _port, _serverKey, UnityWebRequestAdapter.Instance);
            Session = await Client.AuthenticateDeviceAsync(deviceId);
            Socket = Client.NewSocket();
            await Socket.ConnectAsync(Session, true);
            Debug.Log("[Nakama] Connected as " + Session.UserId);
        }

        public async Task CloseAsync()
        {
            if (Socket != null && Socket.IsConnected) await Socket.CloseAsync();
        }
    }
}
#endif
