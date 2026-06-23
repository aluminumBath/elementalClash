#if ELEMENTBORN_NAKAMA
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social.NakamaNet
{
    /// <summary>
    /// Put this on a bootstrap object that initializes <b>before</b> <c>SocialHub</c> (set Script Execution Order,
    /// or place it on an earlier-loading object). It points the social backend at Nakama, connects, then fills in
    /// the real identity and hooks invite-accept to an actual match join. With the <c>ELEMENTBORN_NAKAMA</c>
    /// define off this whole file is excluded and the game uses the offline backend. See docs/NETCODE.md.
    /// </summary>
    public sealed class NakamaSocialInstaller : MonoBehaviour
    {
        [SerializeField] private string scheme = "http";
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 7350;
        [SerializeField] private string serverKey = "defaultkey";

        private NakamaConnection _connection;

        private void Awake()
        {
            _connection = new NakamaConnection(scheme, host, port, serverKey);
            SocialBackends.Factory = () => new NakamaSocialBackend(_connection); // SocialHub.Awake picks this up
            DontDestroyOnLoad(gameObject);
            Connect();
        }

        private async void Connect()
        {
            try { await _connection.ConnectAsync(SystemInfo.deviceUniqueIdentifier); }
            catch (System.Exception e) { Debug.LogError("[Nakama] connect failed: " + e.Message); return; }

            var hub = SocialHub.Instance;
            if (hub != null)
                hub.SetIdentity(_connection.UserId, _connection.Username ?? "Player", UserRole.Player, hub.CurrentSessionId);

            var invites = FindObjectOfType<InviteController>();
            if (invites != null) invites.JoinSession += OnJoinSession;
        }

        private async void OnJoinSession(string sessionId)
        {
            // The session id stands in for a Nakama party/match id. With matches:
            if (_connection == null || !_connection.Connected) return;
            try
            {
                await _connection.Socket.JoinMatchAsync(sessionId);
                Debug.Log("[Nakama] Joined match " + sessionId);
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] join failed: " + e.Message); }
        }

        private async void OnDestroy()
        {
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}
#endif
