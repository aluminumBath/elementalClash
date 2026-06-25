using UnityEngine;
using Elementborn.Core;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>Runs a 1v1 PvP duel on top of the pure, tested <see cref="DuelMatch"/>: challenge a friend and a
    /// first-to-three clash plays out round by round with a live scoreboard, a declared winner, a small
    /// spoils-of-victory reward, and a clean exit back to the world. Offline the rival auto-accepts and each clash is
    /// a fair contested roll so it's playable solo; the networked build accepts on the rival's client and resolves
    /// each round through live arena combat.</summary>
    public sealed class DuelController : MonoBehaviour
    {
        public static DuelController Instance { get; private set; }

        private DuelMatch _match;
        private string _rival;
        private string _lastClash = "";
        private Transform _content;
        private GameObject _panel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private string LocalId => SocialHub.Instance != null ? SocialHub.Instance.CurrentUser.Id : "local";
        private string LocalName { get { var h = SocialHub.Instance; return h != null ? h.CurrentUser.Name : "You"; } }

        private string Name(string id)
        {
            if (id == LocalId) return LocalName;
            var h = SocialHub.Instance;
            if (h != null && h.Directory.TryGet(id, out var u)) return u.Name;
            return id;
        }

        public void Challenge(string friendId)
        {
            _rival = friendId;
            _match = new DuelMatch(LocalId, friendId, 3);
            _match.Accept(); // offline rival accepts; networked build awaits their response
            _lastClash = "The duel begins — first to 3 wins.";
            GameHud.Instance?.Toast("Dueling " + Name(friendId) + "!");
            if (_panel == null)
            {
                var panel = OverlayUi.Panel("DuelCanvas", "DUEL", 216, new Vector2(560, 520), Forfeit);
                _panel = panel.canvas.gameObject;
                _content = panel.content;
            }
            Refresh();
        }

        private void Clash()
        {
            if (_match == null || _match.Phase != DuelPhase.InProgress) return;
            bool youWin = Random.value < 0.5f; // fair contested round; live build resolves this through combat
            _match.Score(youWin ? LocalId : _rival);
            _lastClash = youWin ? "You land a clean strike!" : Name(_rival) + " counters and scores!";
            if (_match.Phase == DuelPhase.Finished) Resolve();
            else Refresh();
        }

        private void Resolve()
        {
            if (_match.Winner == LocalId)
            {
                PlayerInventory.Instance?.AddCurrency(Currency.Silver, 40);
                GameHud.Instance?.Toast("You won the duel! +40 Silver.");
            }
            else GameHud.Instance?.Toast(Name(_rival) + " won the duel. Train and try again.");
            Refresh();
        }

        public void Forfeit()
        {
            _match?.Cancel();
            Close();
        }

        public void Close()
        {
            if (_panel != null) Destroy(_panel);
            _panel = null; _content = null; _match = null;
        }

        private void Refresh()
        {
            if (_content == null || _match == null) return;
            OverlayUi.Clear(_content);

            OverlayUi.Header(_content, LocalName + "  " + _match.ScoreOf(LocalId) + "  —  " + _match.ScoreOf(_rival) + "  " + Name(_rival));
            OverlayUi.Body(_content, _lastClash);

            if (_match.Phase == DuelPhase.InProgress)
            {
                UiTheme.Button(_content, "Clash!", Clash, 360, 56);
                UiTheme.Button(_content, "Forfeit", Forfeit, 360, 44);
            }
            else
            {
                OverlayUi.Header(_content, _match.Winner == LocalId ? "Victory!" : "Defeat.");
                UiTheme.Button(_content, "Leave arena", Close, 360, 50);
            }
        }
    }
}
