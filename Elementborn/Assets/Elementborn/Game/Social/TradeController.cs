using UnityEngine;
using Elementborn.Core;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>Drives a player-to-player trade on top of the pure, tamper-safe <see cref="TradeSession"/>: it shows
    /// both sides of the table, lets you stake items and currency from your real holdings (never more than you own),
    /// mirrors the lock/confirm handshake, and performs the atomic swap the instant both sides confirm — removing
    /// what you staked and granting what they offered. Offers are keyed as item ids, or <c>coin:Currency</c> for
    /// money. Offline a demo partner stakes a small reward and mirrors your lock/confirm so the whole flow is
    /// playable solo; the networked build feeds the partner's real actions into the same session.</summary>
    public sealed class TradeController : MonoBehaviour
    {
        public static TradeController Instance { get; private set; }

        private const string CoinPrefix = "coin:";

        private TradeSession _session;
        private string _partner;
        private Transform _content;
        private GameObject _panel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private string LocalId => SocialHub.Instance != null ? SocialHub.Instance.CurrentUser.Id : "local";

        public void OpenWith(string friendId)
        {
            _partner = friendId;
            _session = new TradeSession(LocalId, friendId);

            // Offline demo partner stakes a token reward; networked build receives their real offer instead.
            _session.SetOffer(friendId, "river_pearl", 1);
            _session.SetOffer(friendId, CoinPrefix + Currency.Silver, 15);

            if (_panel == null)
            {
                var panel = OverlayUi.Panel("TradeCanvas", "TRADE", 215, new Vector2(620, 720), Cancel);
                _panel = panel.canvas.gameObject;
                _content = panel.content;
            }
            Refresh();
        }

        public void Cancel()
        {
            _session?.Cancel();
            if (_panel != null) Destroy(_panel);
            _panel = null; _content = null; _session = null;
        }

        // ---- local actions (the demo partner mirrors lock/confirm) ----
        private void OfferItemDelta(string id, int delta)
        {
            int held = Inv != null ? Inv.Items.Count(id) : 0;
            int now = Mathf.Clamp(_session.OfferA.Lines.TryGetValue(id, out var v) ? v + delta : delta, 0, held);
            _session.SetOffer(LocalId, id, now);
            Refresh();
        }

        private void OfferCoinDelta(Currency c, int delta)
        {
            string key = CoinPrefix + c;
            int held = Inv != null ? Inv.Wallet.CountOf(c) : 0;
            int now = Mathf.Clamp(_session.OfferA.Lines.TryGetValue(key, out var v) ? v + delta : delta, 0, held);
            _session.SetOffer(LocalId, key, now);
            Refresh();
        }

        private void LockMine()
        {
            _session.Lock(LocalId);
            _session.Lock(_partner); // demo partner agrees to what's on the table
            Refresh();
        }

        private void ConfirmMine()
        {
            _session.Confirm(LocalId);
            _session.Confirm(_partner); // demo partner confirms
            if (_session.Phase == TradePhase.Completed) Complete();
            else Refresh();
        }

        private void Complete()
        {
            var inv = Inv;
            if (inv != null)
            {
                foreach (var kv in _session.OfferA.Lines) Give(inv, kv.Key, kv.Value);    // what you staked leaves
                foreach (var kv in _session.OfferB.Lines) Receive(inv, kv.Key, kv.Value); // what they offered arrives
            }
            GameHud.Instance?.Toast("Trade complete with " + Name(_partner) + ".");
            if (_panel != null) Destroy(_panel);
            _panel = null; _content = null; _session = null;
        }

        private static void Give(PlayerInventory inv, string key, int amount)
        {
            if (key.StartsWith(CoinPrefix)) { if (TryCoin(key, out var c)) inv.AddCurrency(c, -Mathf.Min(amount, inv.Wallet.CountOf(c))); }
            else inv.Items.Remove(key, amount);
        }

        private static void Receive(PlayerInventory inv, string key, int amount)
        {
            if (key.StartsWith(CoinPrefix)) { if (TryCoin(key, out var c)) inv.AddCurrency(c, amount); }
            else inv.AddItem(key, amount);
        }

        private static bool TryCoin(string key, out Currency c) =>
            System.Enum.TryParse(key.Substring(CoinPrefix.Length), out c);

        private static PlayerInventory Inv => PlayerInventory.Instance;

        private string Name(string id)
        {
            var hub = SocialHub.Instance;
            if (hub != null && hub.Directory.TryGet(id, out var u)) return u.Name;
            return id;
        }

        private static string Label(string key)
        {
            if (key.StartsWith(CoinPrefix)) return key.Substring(CoinPrefix.Length);
            var def = ItemCatalog.Get(key);
            return def != null ? def.Name : key;
        }

        // ---- UI ----
        private void Refresh()
        {
            if (_content == null || _session == null) return;
            OverlayUi.Clear(_content);
            string me = LocalId;
            bool locked = _session.IsLocked(me);

            // their offer (read-only)
            OverlayUi.Header(_content, Name(_partner) + " offers");
            if (_session.OfferB.IsEmpty) OverlayUi.Body(_content, "(nothing yet)", 18);
            else foreach (var kv in _session.OfferB.Lines) OverlayUi.Body(_content, "• " + kv.Value + "x " + Label(kv.Key));

            // your offer
            OverlayUi.Header(_content, "You offer" + (locked ? "  [locked]" : ""));
            if (_session.OfferA.IsEmpty) OverlayUi.Body(_content, "(nothing yet)", 18);
            else foreach (var kv in _session.OfferA.Lines) OverlayUi.Body(_content, "• " + kv.Value + "x " + Label(kv.Key));

            if (!locked)
            {
                OverlayUi.Header(_content, "Stake from your bag");
                var inv = Inv;
                if (inv != null)
                {
                    foreach (var e in inv.Items.Entries())
                    {
                        string id = e.Key;
                        UiTheme.Button(_content, "+ " + Label(id) + " (have " + e.Value + ")", () => OfferItemDelta(id, 1), 300, 42);
                    }
                    if (inv.Wallet.CountOf(Currency.Silver) > 0)
                        UiTheme.Button(_content, "+ 10 Silver", () => OfferCoinDelta(Currency.Silver, 10), 300, 42);
                }
                UiTheme.Button(_content, "Clear my offer", ClearMine, 300, 42);
            }

            // handshake controls
            if (_session.Phase == TradePhase.Locked)
            {
                bool youConfirmed = _session.IsConfirmed(me);
                UiTheme.Button(_content, youConfirmed ? "Confirmed — waiting" : "Confirm trade", ConfirmMine, 360, 50);
                UiTheme.Button(_content, "Unlock", () => { _session.Unlock(me); _session.Unlock(_partner); Refresh(); }, 360, 44);
            }
            else
            {
                UiTheme.Button(_content, locked ? "Locked — waiting" : "Lock my offer", LockMine, 360, 50);
            }
            UiTheme.Button(_content, "Cancel trade", Cancel, 360, 44);
        }

        private void ClearMine()
        {
            if (_session == null) return;
            foreach (var kv in new System.Collections.Generic.List<string>(_session.OfferA.Lines.Keys))
                _session.SetOffer(LocalId, kv, 0);
            Refresh();
        }
    }
}
