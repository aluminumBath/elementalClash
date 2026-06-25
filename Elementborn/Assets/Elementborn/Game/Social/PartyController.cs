using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>Local-authoritative party for co-op: who's grouped with you, who leads, and the
    /// invite/leave/kick/promote actions — backed by the pure, tested <see cref="PartyRoster"/> and wired to the
    /// existing session-invite system. Inviting a friend forms the party (you as leader) and sends a real GameInvite
    /// through SocialHub; in the offline build the invitee is seated immediately so the party is demonstrable solo,
    /// while the networked build seats them when their client accepts. Opens a roster panel from the Social menu.</summary>
    public sealed class PartyController : MonoBehaviour
    {
        public static PartyController Instance { get; private set; }

        private readonly PartyRoster _roster = new PartyRoster();
        private Transform _content;
        private GameObject _panel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public PartyRoster Roster => _roster;
        private string LocalId => SocialHub.Instance != null ? SocialHub.Instance.CurrentUser.Id : "local";

        /// <summary>Form the party if needed, send a real session invite, and (offline) seat the friend so the party
        /// is visible. The networked build seats them when their client accepts instead.</summary>
        public void InviteToParty(string friendId)
        {
            if (!_roster.Active) _roster.Form(LocalId);
            SendSessionInvite(friendId); // real session invite + notification
            var r = _roster.Join(friendId);
            if (r == PartyJoinResult.PartyFull) GameHud.Instance?.Toast("Your party is full.");
            else if (r == PartyJoinResult.Joined) GameHud.Instance?.Toast(Name(friendId) + " joined your party.");
            Refresh();
        }

        private void SendSessionInvite(string friendId)
        {
            var hub = SocialHub.Instance;
            if (hub == null || hub.Invites == null) return;
            hub.Invites.Invite(hub.CurrentUser.Id, friendId, hub.CurrentSessionId);
        }

        public void LeaveParty()
        {
            if (!_roster.Active) return;
            _roster.Leave(LocalId);
            GameHud.Instance?.Toast("You left the party.");
            Refresh();
        }

        public void Kick(string id) { if (_roster.Kick(LocalId, id)) { GameHud.Instance?.Toast("Removed " + Name(id) + "."); Refresh(); } }
        public void Promote(string id) { if (_roster.Promote(LocalId, id)) { GameHud.Instance?.Toast(Name(id) + " now leads."); Refresh(); } }
        public void Disband() { _roster.Disband(); GameHud.Instance?.Toast("Party disbanded."); Refresh(); }

        private string Name(string id)
        {
            var hub = SocialHub.Instance;
            if (hub != null && hub.Directory.TryGet(id, out var u)) return u.Name;
            return id;
        }

        // ---- UI ----
        public void Open()
        {
            if (_panel != null) { Refresh(); return; }
            var panel = OverlayUi.Panel("PartyCanvas", "PARTY", 210, new Vector2(560, 640), Close);
            _panel = panel.canvas.gameObject;
            _content = panel.content;
            Refresh();
        }

        public void Close()
        {
            if (_panel != null) Destroy(_panel);
            _panel = null;
            _content = null;
        }

        private void Refresh()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            string me = LocalId;
            if (!_roster.Active)
            {
                OverlayUi.Body(_content, "You're not in a party. Invite a friend below to start one.");
            }
            else
            {
                OverlayUi.Header(_content, "Members (" + _roster.Count + "/" + PartyRoster.MaxMembers + ")");
                foreach (var id in _roster.Members)
                {
                    string tag = _roster.IsLeader(id) ? "  [leader]" : "";
                    OverlayUi.Body(_content, Name(id) + (id == me ? " (you)" : "") + tag);
                    if (_roster.IsLeader(me) && id != me)
                    {
                        UiTheme.Button(_content, "Make leader: " + Name(id), () => Promote(id), 360, 44);
                        UiTheme.Button(_content, "Remove: " + Name(id), () => Kick(id), 360, 44);
                    }
                }
                UiTheme.Button(_content, "Leave party", LeaveParty, 360, 48);
                if (_roster.IsLeader(me)) UiTheme.Button(_content, "Disband party", Disband, 360, 48);
            }

            OverlayUi.Header(_content, "Invite a friend");
            var hub = SocialHub.Instance;
            if (hub == null) { OverlayUi.Body(_content, "Sign-in required to invite friends.", 18); return; }
            bool any = false;
            foreach (var fid in hub.Friends.FriendsOf(me))
            {
                if (_roster.Contains(fid)) continue;
                any = true;
                UiTheme.Button(_content, "Invite " + Name(fid), () => InviteToParty(fid), 360, 44);
            }
            if (!any) OverlayUi.Body(_content, "No friends available to invite.", 18);
        }
    }
}
