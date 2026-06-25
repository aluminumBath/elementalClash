using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>Player-run guild on top of the pure, tested <see cref="Guild"/>: a persistent named group with a
    /// ranked roster (Leader/Officer/Member), rank-gated management, a light guild chat, and full save/load — distinct
    /// from the NPC lore factions. You found a guild, invite friends, and manage ranks; offline an invited friend
    /// joins immediately so it's demonstrable solo, while the networked build joins them on accept and routes chat
    /// through Nakama. Persisted with the game via CaptureInto / RestoreFrom.</summary>
    public sealed class GuildController : MonoBehaviour
    {
        public static GuildController Instance { get; private set; }

        private static readonly string[] QuickPhrases = { "Hello, guild!", "Anyone up for a hunt?", "On my way!", "Nice work, team." };
        private static readonly string[] Replies = { "Hey!", "Count me in.", "See you there.", "Welcome!" };

        private Guild _guild;
        private readonly List<string> _chat = new List<string>();
        private int _echo;
        private Transform _content;
        private GameObject _panel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public Guild Current => _guild;
        private string LocalId => SocialHub.Instance != null ? SocialHub.Instance.CurrentUser.Id : "local";
        private string LocalName { get { var h = SocialHub.Instance; return h != null ? h.CurrentUser.Name : "You"; } }

        private string Name(string id)
        {
            if (id == LocalId) return LocalName;
            var h = SocialHub.Instance;
            if (h != null && h.Directory.TryGet(id, out var u)) return u.Name;
            return id;
        }

        // ---- actions ----
        public void Found()
        {
            if (_guild != null) return;
            _guild = new Guild("guild:" + LocalId, LocalName + "'s Circle", LocalId);
            GameHud.Instance?.Toast("Founded " + _guild.Name + ".");
            Refresh();
        }

        public void Invite(string friendId)
        {
            if (_guild == null || !_guild.CanManage(LocalId)) return;
            if (_guild.Join(friendId))
            {
                SocialHub.Instance?.Invites?.InviteFriend(friendId);
                GameHud.Instance?.Toast(Name(friendId) + " joined the guild.");
            }
            Refresh();
        }

        public void LeaveGuild()
        {
            if (_guild == null) return;
            string n = _guild.Name;
            _guild.Leave(LocalId);
            _guild = null; _chat.Clear();
            GameHud.Instance?.Toast("You left " + n + ".");
            Refresh();
        }

        public void Kick(string id) { if (_guild != null && _guild.Kick(LocalId, id)) { GameHud.Instance?.Toast("Removed " + Name(id) + "."); Refresh(); } }
        public void Promote(string id) { if (_guild != null && _guild.Promote(LocalId, id)) { GameHud.Instance?.Toast("Promoted " + Name(id) + "."); Refresh(); } }
        public void Demote(string id) { if (_guild != null && _guild.Demote(LocalId, id)) { GameHud.Instance?.Toast("Demoted " + Name(id) + "."); Refresh(); } }
        public void Disband() { if (_guild != null && _guild.IsLeader(LocalId)) { string n = _guild.Name; _guild = null; _chat.Clear(); GameHud.Instance?.Toast(n + " disbanded."); Refresh(); } }

        public void PostChat(string text)
        {
            if (_guild == null) return;
            Push(LocalName + ": " + text);
            foreach (var m in _guild.Members)
                if (m.Id != LocalId) { Push(Name(m.Id) + ": " + Replies[_echo++ % Replies.Length]); break; }
            Refresh();
        }

        private void Push(string line) { _chat.Add(line); if (_chat.Count > 12) _chat.RemoveAt(0); }

        // ---- persistence ----
        public void CaptureInto(SaveData d)
        {
            d.guildMemberIds.Clear();
            d.guildMemberRanks.Clear();
            if (_guild == null) { d.guildId = ""; d.guildName = ""; return; }
            d.guildId = _guild.Id;
            d.guildName = _guild.Name;
            foreach (var m in _guild.Members) { d.guildMemberIds.Add(m.Id); d.guildMemberRanks.Add((int)m.Rank); }
        }

        public void RestoreFrom(SaveData d)
        {
            _chat.Clear();
            if (string.IsNullOrEmpty(d.guildId) || d.guildMemberIds.Count == 0) { _guild = null; return; }
            var ranks = new List<GuildRank>();
            foreach (var r in d.guildMemberRanks) ranks.Add((GuildRank)r);
            _guild = Guild.Restore(d.guildId, d.guildName, d.guildMemberIds, ranks);
        }

        // ---- UI ----
        public void Open()
        {
            if (_panel != null) { Refresh(); return; }
            var panel = OverlayUi.Panel("GuildCanvas", "GUILD", 212, new Vector2(600, 760), Close);
            _panel = panel.canvas.gameObject;
            _content = panel.content;
            Refresh();
        }

        public void Close()
        {
            if (_panel != null) Destroy(_panel);
            _panel = null; _content = null;
        }

        private void Refresh()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);
            string me = LocalId;

            if (_guild == null)
            {
                OverlayUi.Body(_content, "You're not in a guild. Found one to gather a roster and chat with your group.");
                UiTheme.Button(_content, "Found a guild", Found, 360, 50);
                return;
            }

            OverlayUi.Header(_content, _guild.Name + "  (" + _guild.Count + ")");
            foreach (var m in _guild.Members)
            {
                string id = m.Id;
                OverlayUi.Body(_content, Name(id) + (id == me ? " (you)" : "") + " — " + m.Rank);
                if (_guild.IsLeader(me) && id != me)
                {
                    UiTheme.Button(_content, "Promote " + Name(id), () => Promote(id), 340, 40);
                    if (m.Rank == GuildRank.Officer) UiTheme.Button(_content, "Demote " + Name(id), () => Demote(id), 340, 40);
                    UiTheme.Button(_content, "Remove " + Name(id), () => Kick(id), 340, 40);
                }
            }

            var hub = SocialHub.Instance;
            if (hub != null && _guild.CanManage(me))
            {
                OverlayUi.Header(_content, "Invite a friend");
                bool any = false;
                foreach (var fid in hub.Friends.FriendsOf(me))
                {
                    if (_guild.Contains(fid)) continue;
                    any = true;
                    string f = fid;
                    UiTheme.Button(_content, "Invite " + Name(f), () => Invite(f), 340, 40);
                }
                if (!any) OverlayUi.Body(_content, "No friends available to invite.", 18);
            }

            OverlayUi.Header(_content, "Guild chat");
            if (_chat.Count == 0) OverlayUi.Body(_content, "(no messages yet)", 18);
            else foreach (var line in _chat) OverlayUi.Body(_content, line, 18);
            foreach (var phrase in QuickPhrases)
            {
                string p = phrase;
                UiTheme.Button(_content, "Say: " + p, () => PostChat(p), 340, 38);
            }

            UiTheme.Button(_content, "Leave guild", LeaveGuild, 360, 46);
            if (_guild.IsLeader(me)) UiTheme.Button(_content, "Disband guild", Disband, 360, 46);
        }
    }
}
