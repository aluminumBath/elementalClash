using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// A code-built, toggled social overlay (default key J) wiring the existing social controllers onto screen:
    /// notifications, friends + invites, session chat, a feedback form, and — for admins / session-admins — a
    /// moderation panel. It auto-ensures a <see cref="SocialHub"/> and the six controllers exist, so dropping
    /// this one component on any GameObject is enough. Built through <see cref="UiTheme"/>; lists show the most
    /// recent entries (no scrollbar needed). The Moderation tab appears only when the current user can moderate
    /// (call <c>SocialHub.GrantSelfAdmin()</c> to try it locally).
    /// </summary>
    public sealed class SocialMenuController : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.J;

        private enum Tab { Notifications, Friends, Chat, Feedback, Moderation }

        private Canvas _canvas;
        private Transform _content;
        private Button _moderationTab;
        private bool _open;
        private Tab _tab = Tab.Notifications;
        private bool _feedbackBug = true;

        private Transform _chatHistory;
        private UiInput _chatInput;

        private NotificationController _notes;
        private FriendController _friends;
        private InviteController _invites;
        private ChatController _chat;
        private FeedbackController _feedback;
        private ModerationController _moderation;

        // ---- lifecycle ------------------------------------------------------------------------------------

        private void Awake()
        {
            EnsureSocial();
            Build();
            Hide();
        }

        private void OnEnable()
        {
            if (_notes != null) _notes.Changed += Refresh;
            if (_friends != null) _friends.Changed += Refresh;
            if (_invites != null) _invites.Changed += Refresh;
            if (_moderation != null) _moderation.Changed += Refresh;
            if (_chat != null) _chat.MessageReceived += OnMessage;
        }

        private void OnDisable()
        {
            if (_notes != null) _notes.Changed -= Refresh;
            if (_friends != null) _friends.Changed -= Refresh;
            if (_invites != null) _invites.Changed -= Refresh;
            if (_moderation != null) _moderation.Changed -= Refresh;
            if (_chat != null) _chat.MessageReceived -= OnMessage;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        private void EnsureSocial()
        {
            if (SocialHub.Instance == null && FindObjectOfType<SocialHub>() == null)
                new GameObject("SocialHub").AddComponent<SocialHub>();
            _notes = Ensure<NotificationController>();
            _friends = Ensure<FriendController>();
            _invites = Ensure<InviteController>();
            _chat = Ensure<ChatController>();
            _feedback = Ensure<FeedbackController>();
            _moderation = Ensure<ModerationController>();
        }

        private T Ensure<T>() where T : MonoBehaviour
        {
            var existing = FindObjectOfType<T>();
            return existing != null ? existing : gameObject.AddComponent<T>();
        }

        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; if (_canvas != null) _canvas.gameObject.SetActive(true); BuildTab(); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Refresh()
        {
            if (_open && (_tab == Tab.Notifications || _tab == Tab.Friends)) BuildTab();
        }

        private void OnMessage(ChatMessage m)
        {
            if (_open && _tab == Tab.Chat) RefreshChatHistory();
        }

        // ---- shell ----------------------------------------------------------------------------------------

        private void Build()
        {
            _canvas = UiTheme.Canvas("SocialCanvas", sortOrder: 50);
            _canvas.gameObject.AddComponent<VrCanvasAdapter>();

            var root = new GameObject("SocialRoot", typeof(RectTransform));
            root.transform.SetParent(_canvas.transform, false);
            var rr = UiTheme.Rect(root);
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(1120, 720);
            rr.anchoredPosition = Vector2.zero;
            root.AddComponent<Image>().color = UiTheme.PanelColor;

            var title = UiTheme.Label(root.transform, "Social", 34, UiTheme.TextColor, TextAnchor.MiddleLeft);
            var tr = title.Rect;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(-220, 56); tr.anchoredPosition = new Vector2(30, -16);

            var close = UiTheme.Button(root.transform, "Close (Esc)", Hide, 160, 44);
            var cr = UiTheme.Rect(close.gameObject);
            cr.anchorMin = cr.anchorMax = new Vector2(1f, 1f); cr.pivot = new Vector2(1f, 1f);
            cr.anchoredPosition = new Vector2(-20, -16);

            var rail = new GameObject("Tabs", typeof(RectTransform));
            rail.transform.SetParent(root.transform, false);
            var railR = UiTheme.Rect(rail);
            railR.anchorMin = new Vector2(0f, 0f); railR.anchorMax = new Vector2(0f, 1f); railR.pivot = new Vector2(0f, 1f);
            railR.sizeDelta = new Vector2(240, -100); railR.anchoredPosition = new Vector2(20, -90);
            var vlg = rail.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8; vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false; vlg.childAlignment = TextAnchor.UpperLeft;
            AddTabButton(rail.transform, "Notifications", Tab.Notifications);
            AddTabButton(rail.transform, "Friends", Tab.Friends);
            AddTabButton(rail.transform, "Chat", Tab.Chat);
            AddTabButton(rail.transform, "Feedback", Tab.Feedback);
            _moderationTab = AddTabButton(rail.transform, "Moderation", Tab.Moderation);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(root.transform, false);
            var contR = UiTheme.Rect(content);
            contR.anchorMin = Vector2.zero; contR.anchorMax = Vector2.one;
            contR.offsetMin = new Vector2(280, 20); contR.offsetMax = new Vector2(-20, -90);
            _content = content.transform;

            BuildTab();
        }

        private Button AddTabButton(Transform parent, string label, Tab tab)
        {
            var b = UiTheme.Button(parent, label, () => { _tab = tab; BuildTab(); }, 220, 48);
            var le = b.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 48; le.preferredHeight = 48;
            return b;
        }

        private void BuildTab()
        {
            if (_content == null) return;
            for (int i = _content.childCount - 1; i >= 0; i--) DestroyImmediate(_content.GetChild(i).gameObject);
            _chatHistory = null; _chatInput = null;

            bool canMod = SocialHub.Instance != null && SocialHub.Instance.CurrentUser.CanModerateSessions;
            if (_moderationTab != null) _moderationTab.gameObject.SetActive(canMod);
            if (_tab == Tab.Moderation && !canMod) _tab = Tab.Notifications;

            switch (_tab)
            {
                case Tab.Notifications: BuildNotifications(); break;
                case Tab.Friends: BuildFriends(); break;
                case Tab.Chat: BuildChat(); break;
                case Tab.Feedback: BuildFeedback(); break;
                case Tab.Moderation: BuildModeration(); break;
            }
        }

        // ---- panels ---------------------------------------------------------------------------------------

        private void BuildNotifications()
        {
            var panel = Section();
            int unread = _notes != null ? _notes.UnreadCount : 0;
            Header(panel, unread > 0 ? "Notifications  (" + unread + " unread)" : "Notifications");
            Fix(UiTheme.Button(panel, "Mark all read", MarkAllRead, 220, 44), 220);

            var list = List(panel);
            var inbox = _notes != null ? _notes.Inbox : null;
            if (inbox == null || inbox.Count == 0) { Header(list, "No notifications yet.", 18); return; }

            int shown = 0;
            for (int i = inbox.Count - 1; i >= 0 && shown < 12; i--, shown++)
            {
                var n = inbox[i];
                var row = Row(list, 64);
                NameLabel(row, n.Title + " — " + n.Body, n.Read ? Dim : UiTheme.TextColor, 18);
                if (!n.Read)
                {
                    string id = n.Id;
                    Fix(UiTheme.Button(row, "Read", () => _notes.MarkRead(id), 110, 44), 110);
                }
            }
        }

        private void MarkAllRead()
        {
            if (_notes == null) return;
            var inbox = _notes.Inbox;
            for (int i = 0; i < inbox.Count; i++) if (!inbox[i].Read) _notes.MarkRead(inbox[i].Id);
        }

        private void BuildFriends()
        {
            var panel = Section();
            Header(panel, "Friends");

            var addRow = Row(panel, 50);
            var addInput = UiTheme.Input(addRow, "User id to add…", 48);
            Flex(addInput);
            Fix(UiTheme.Button(addRow, "Send request", () =>
            {
                if (_friends != null && !string.IsNullOrWhiteSpace(addInput.text))
                {
                    var st = _friends.SendRequest(addInput.text.Trim());
                    GameHud.Instance?.Toast("Friend request: " + st);
                }
            }, 180, 48), 180);

            Header(panel, "Requests to you", 20);
            var reqList = List(panel);
            var reqs = _friends != null ? _friends.IncomingRequests : null;
            if (reqs == null || reqs.Count == 0) Header(reqList, "None", 16);
            else for (int i = 0; i < reqs.Count; i++)
            {
                string id = reqs[i];
                var row = Row(reqList, 56);
                NameLabel(row, Name(id));
                Fix(UiTheme.Button(row, "Accept", () => _friends.Accept(id), 110, 40), 110);
                Fix(UiTheme.Button(row, "Decline", () => _friends.Decline(id), 110, 40), 110);
            }

            Header(panel, "Your friends", 20);
            var friendList = List(panel);
            var ids = _friends != null ? _friends.FriendIds : null;
            if (ids == null || ids.Count == 0) Header(friendList, "No friends yet", 16);
            else for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                var row = Row(friendList, 56);
                NameLabel(row, Name(id));
                Fix(UiTheme.Button(row, "Invite", () => { _invites?.InviteFriend(id); GameHud.Instance?.Toast("Invited " + Name(id)); }, 110, 40), 110);
                Fix(UiTheme.Button(row, "Remove", () => _friends.Unfriend(id), 110, 40), 110);
            }

            Header(panel, "Invites to you", 20);
            var invList = List(panel);
            var invs = _invites != null ? _invites.Pending : null;
            if (invs == null || invs.Count == 0) Header(invList, "None", 16);
            else for (int i = 0; i < invs.Count; i++)
            {
                var inv = invs[i];
                string id = inv.Id;
                var row = Row(invList, 56);
                NameLabel(row, "From " + Name(inv.FromUserId));
                Fix(UiTheme.Button(row, "Join", () => _invites.Accept(id), 110, 40), 110);
                Fix(UiTheme.Button(row, "Decline", () => _invites.Decline(id), 110, 40), 110);
            }
        }

        private void BuildChat()
        {
            var panel = Section();
            Header(panel, "Session chat");
            Header(panel, "Channel " + (SocialHub.Instance != null ? Short(SocialHub.Instance.CurrentSessionId) : "—"), 16);

            var host = new GameObject("History", typeof(RectTransform));
            host.transform.SetParent(panel, false);
            var le = host.AddComponent<LayoutElement>(); le.flexibleHeight = 1; le.minHeight = 360;
            var v = host.AddComponent<VerticalLayoutGroup>();
            v.spacing = 4; v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false; v.childAlignment = TextAnchor.LowerLeft;
            _chatHistory = host.transform;
            RefreshChatHistory();

            var row = Row(panel, 50);
            _chatInput = UiTheme.Input(row, "Message…", 48);
            Flex(_chatInput);
            Fix(UiTheme.Button(row, "Send", SendChat, 140, 48), 140);
        }

        private void SendChat()
        {
            if (_chat == null || _chatInput == null || string.IsNullOrWhiteSpace(_chatInput.text)) return;
            _chat.SendToSession(_chatInput.text);
            _chatInput.text = "";
            RefreshChatHistory();
        }

        private void RefreshChatHistory()
        {
            if (_chatHistory == null || _chat == null) return;
            for (int i = _chatHistory.childCount - 1; i >= 0; i--) DestroyImmediate(_chatHistory.GetChild(i).gameObject);
            var msgs = _chat.SessionHistory();
            int start = Mathf.Max(0, msgs.Count - 14);
            for (int i = start; i < msgs.Count; i++)
            {
                var m = msgs[i];
                var l = UiTheme.Label(_chatHistory, Name(m.SenderId) + ":  " + m.Text, 20, UiTheme.TextColor, TextAnchor.MiddleLeft);
                var le = l.Graphic.gameObject.AddComponent<LayoutElement>(); le.minHeight = 26; le.preferredHeight = 26;
            }
        }

        private void BuildFeedback()
        {
            var panel = Section();
            Header(panel, "Send feedback");
            _feedbackBug = true;
            UiTheme.Toggle(panel, "Report as bug  (off = suggestion)", true, v => _feedbackBug = v);
            var titleIn = UiTheme.Input(panel, "Title…", 48);
            var bodyIn = UiTheme.Input(panel, "Describe it…", 130, true);
            UiTheme.Button(panel, "Submit", () =>
            {
                if (_feedback == null || string.IsNullOrWhiteSpace(titleIn.text)) { GameHud.Instance?.Toast("Add a title first"); return; }
                if (_feedbackBug) _feedback.SubmitBug(titleIn.text.Trim(), bodyIn.text);
                else _feedback.SubmitSuggestion(titleIn.text.Trim(), bodyIn.text);
                titleIn.text = ""; bodyIn.text = "";
                GameHud.Instance?.Toast("Feedback sent");
            }, 220, 52);
        }

        private void BuildModeration()
        {
            var panel = Section();
            bool admin = SocialHub.Instance != null && SocialHub.Instance.CurrentUser.IsAdmin;
            Header(panel, "Moderation");
            Header(panel, "Session " + (SocialHub.Instance != null ? Short(SocialHub.Instance.CurrentSessionId) : "—") + (admin ? "   · admin" : "   · session admin"), 16);

            var targetIn = UiTheme.Input(panel, "Target user id…", 48);
            var reasonIn = UiTheme.Input(panel, "Reason (optional)…", 48);

            var sRow = Row(panel, 52);
            Fix(UiTheme.Button(sRow, "Ban from session", () => Mod(targetIn.text, reasonIn.text, ban: true, global: false), 230, 48), 230);
            Fix(UiTheme.Button(sRow, "Allow in session", () => Mod(targetIn.text, reasonIn.text, ban: false, global: false), 230, 48), 230);

            if (admin)
            {
                var gRow = Row(panel, 52);
                Fix(UiTheme.Button(gRow, "Ban globally", () => Mod(targetIn.text, reasonIn.text, ban: true, global: true), 230, 48), 230);
                Fix(UiTheme.Button(gRow, "Lift global ban", () => Mod(targetIn.text, reasonIn.text, ban: false, global: true), 230, 48), 230);
            }
        }

        private void Mod(string target, string reason, bool ban, bool global)
        {
            if (_moderation == null || string.IsNullOrWhiteSpace(target)) { GameHud.Instance?.Toast("Enter a target id"); return; }
            target = target.Trim();
            bool ok = global
                ? (ban ? _moderation.BanGlobally(target, reason) : _moderation.LiftGlobalBan(target))
                : (ban ? _moderation.BanFromSession(target, reason) : _moderation.AllowInSession(target));
            GameHud.Instance?.Toast(ok ? "Done" : "Not permitted / no change");
        }

        // ---- small builders -------------------------------------------------------------------------------

        private static readonly Color Dim = new Color(0.62f, 0.64f, 0.70f, 1f);

        private Transform Section()
        {
            var go = new GameObject("Panel", typeof(RectTransform));
            go.transform.SetParent(_content, false);
            var r = UiTheme.Rect(go);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = 10; v.padding = new RectOffset(4, 4, 4, 4);
            v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false; v.childAlignment = TextAnchor.UpperLeft;
            return go.transform;
        }

        private Transform List(Transform parent)
        {
            var go = new GameObject("List", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>(); le.flexibleHeight = 1; le.minHeight = 120;
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = 6; v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false; v.childAlignment = TextAnchor.UpperLeft;
            return go.transform;
        }

        private Transform Row(Transform parent, float height)
        {
            var go = new GameObject("Row", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
            var le = go.AddComponent<LayoutElement>(); le.minHeight = height; le.preferredHeight = height;
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 8; h.padding = new RectOffset(10, 10, 4, 4);
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = false; h.childForceExpandHeight = true; h.childAlignment = TextAnchor.MiddleLeft;
            return go.transform;
        }

        private UiLabel Header(Transform parent, string text, int size = 26)
        {
            var l = UiTheme.Label(parent, text, size, UiTheme.TextColor, TextAnchor.MiddleLeft);
            var le = l.Graphic.gameObject.AddComponent<LayoutElement>();
            le.minHeight = size + 10; le.preferredHeight = size + 10;
            return l;
        }

        private void NameLabel(Transform row, string text, Color? color = null, int size = 20)
        {
            var l = UiTheme.Label(row, text, size, color ?? UiTheme.TextColor, TextAnchor.MiddleLeft);
            l.Graphic.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        }

        private void Fix(Button b, int width)
        {
            var le = b.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width; le.preferredWidth = width; le.flexibleWidth = 0;
        }

        private void Flex(UiInput input)
        {
            var le = input.Rect.GetComponent<LayoutElement>();
            if (le != null) le.flexibleWidth = 1;
        }

        private string Name(string userId)
        {
            var hub = SocialHub.Instance;
            if (hub != null && hub.Directory.TryGet(userId, out var u) && !string.IsNullOrEmpty(u.DisplayName)) return u.DisplayName;
            return userId;
        }

        private static string Short(string s) => string.IsNullOrEmpty(s) ? "—" : (s.Length <= 6 ? s : s.Substring(0, 6));
    }
}
