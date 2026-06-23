using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The conversation panel an NPC opens. Shows the NPC's line, then quest-aware choices: a Turn in button when
    /// they have a quest of yours ready, an Accept button (with the summary and objectives) for a quest you
    /// haven't started, and a read-only progress block for one already in flight. Falls back to just the line and
    /// a Goodbye. A singleton the bootstrap scene adds; NPCs call <see cref="Open"/>.
    /// </summary>
    public sealed class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        private Canvas _canvas;
        private Transform _content;
        private string _npcId;
        private string _npcName;
        private string _line;
        private bool _open;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            var p = OverlayUi.Panel("DialogueCanvas", "", 60, new Vector2(900, 560), Hide);
            _canvas = p.canvas; _content = p.content;
            Hide();
        }

        private void Start()
        {
            if (QuestController.Instance != null) QuestController.Instance.Changed += Rebuild;
        }

        private void OnDestroy()
        {
            if (QuestController.Instance != null) QuestController.Instance.Changed -= Rebuild;
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!_open) return;
            var kb = Keyboard.current;
            if (kb != null && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        public void Open(string npcId, string npcName, string line)
        {
            _npcId = npcId; _npcName = npcName; _line = line;
            _open = true;
            if (_canvas != null) _canvas.gameObject.SetActive(true);
            Rebuild();
        }

        private void Hide()
        {
            _open = false;
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        private void Rebuild()
        {
            if (!_open || _content == null) return;
            OverlayUi.Clear(_content);

            OverlayUi.Header(_content, _npcName);
            OverlayUi.Body(_content, _line, 20, new Color(0.85f, 0.88f, 0.93f, 1f));

            var log = QuestController.Instance != null ? QuestController.Instance.Log : null;
            if (log != null)
            {
                foreach (var s in log.TurnInsFor(_npcId))
                {
                    string id = s.Definition.Id;
                    OverlayUi.Header(_content, "Ready: " + s.Definition.Title, 20);
                    UiTheme.Button(_content, "Turn in  (reward: " + RewardText(s) + ")",
                        () => { if (QuestController.Instance != null) QuestController.Instance.TurnIn(id); Rebuild(); }, 540, 50);
                }

                foreach (var s in log.AvailableFrom(_npcId))
                {
                    string id = s.Definition.Id;
                    OverlayUi.Header(_content, s.Definition.Title, 20);
                    OverlayUi.Body(_content, s.Definition.Summary, 18, new Color(0.78f, 0.80f, 0.86f, 1f));
                    foreach (var o in s.Definition.Objectives)
                        OverlayUi.Body(_content, "•  " + o.Description, 16, new Color(0.70f, 0.72f, 0.78f, 1f));
                    UiTheme.Button(_content, "Accept",
                        () => { if (QuestController.Instance != null) QuestController.Instance.Start(id); Rebuild(); }, 220, 48);
                }

                foreach (var s in log.ActiveFrom(_npcId))
                {
                    OverlayUi.Header(_content, "In progress: " + s.Definition.Title, 20);
                    for (int i = 0; i < s.Definition.Objectives.Count; i++)
                    {
                        var o = s.Definition.Objectives[i];
                        OverlayUi.Body(_content, "•  " + o.Description + "   (" + s.Progress[i] + "/" + o.Required + ")",
                            16, new Color(0.70f, 0.72f, 0.78f, 1f));
                    }
                }
            }

            UiTheme.Button(_content, "Goodbye", Hide, 200, 46);
        }

        private static string RewardText(QuestState s) =>
            s.Definition.Reward.Amount > 0 ? (s.Definition.Reward.Amount + " " + s.Definition.Reward.Currency) : "—";
    }
}
