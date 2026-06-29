using UnityEngine;

namespace Elementborn.Game
{
    public enum ElementbornPrototypeQuestState
    {
        NotStarted,
        TalkedToGuide,
        CollectedShard,
        Completed
    }

    /// <summary>
    /// A real, self-contained prototype loop:
    /// Start menu -> talk to NPC -> collect shard -> return shard -> save/load.
    /// This is intentionally simple and does not depend on unfinished production systems.
    /// </summary>
    public sealed class ElementbornPrototypeGameManager : MonoBehaviour
    {
        public const string SaveKeyQuestState = "Elementborn.Prototype.QuestState";
        public const string SaveKeyPlayerX = "Elementborn.Prototype.PlayerX";
        public const string SaveKeyPlayerY = "Elementborn.Prototype.PlayerY";
        public const string SaveKeyPlayerZ = "Elementborn.Prototype.PlayerZ";

        public static ElementbornPrototypeGameManager Instance { get; private set; }

        [Header("Runtime References")]
        public ElementbornPrototypePlayerController player;
        public Camera playCamera;

        [Header("Prototype State")]
        public bool menuOpen = true;
        public ElementbornPrototypeQuestState questState = ElementbornPrototypeQuestState.NotStarted;

        [Header("UI")]
        public bool showHud = true;
        public bool showDebugHelp = true;

        private string message = "Welcome to Elementborn.";
        private float messageUntil;

        public bool HasStarted => !menuOpen;

        private void Awake()
        {
            Instance = this;
            ResolveReferences();
            LoadIfPresent(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            ResolveReferences();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuOpen = !menuOpen;
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadIfPresent(true);
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                showDebugHelp = !showDebugHelp;
            }
        }

        public void StartPrototype()
        {
            menuOpen = false;
            ShowMessage("Prototype started. Talk to Ember Guide with E.");
        }

        public void ResetPrototype()
        {
            questState = ElementbornPrototypeQuestState.NotStarted;
            PlayerPrefs.DeleteKey(SaveKeyQuestState);
            PlayerPrefs.DeleteKey(SaveKeyPlayerX);
            PlayerPrefs.DeleteKey(SaveKeyPlayerY);
            PlayerPrefs.DeleteKey(SaveKeyPlayerZ);
            PlayerPrefs.Save();

            if (player != null)
            {
                player.Teleport(new Vector3(0f, 1f, -8f));
            }

            menuOpen = false;
            ShowMessage("Prototype reset.");
        }

        public void Save()
        {
            PlayerPrefs.SetInt(SaveKeyQuestState, (int)questState);

            if (player != null)
            {
                Vector3 position = player.transform.position;
                PlayerPrefs.SetFloat(SaveKeyPlayerX, position.x);
                PlayerPrefs.SetFloat(SaveKeyPlayerY, position.y);
                PlayerPrefs.SetFloat(SaveKeyPlayerZ, position.z);
            }

            PlayerPrefs.Save();
            ShowMessage("Saved prototype state.");
        }

        public void LoadIfPresent(bool showMessage)
        {
            if (PlayerPrefs.HasKey(SaveKeyQuestState))
            {
                questState = (ElementbornPrototypeQuestState)PlayerPrefs.GetInt(SaveKeyQuestState, 0);
            }

            if (player != null &&
                PlayerPrefs.HasKey(SaveKeyPlayerX) &&
                PlayerPrefs.HasKey(SaveKeyPlayerY) &&
                PlayerPrefs.HasKey(SaveKeyPlayerZ))
            {
                Vector3 position = new Vector3(
                    PlayerPrefs.GetFloat(SaveKeyPlayerX),
                    PlayerPrefs.GetFloat(SaveKeyPlayerY),
                    PlayerPrefs.GetFloat(SaveKeyPlayerZ));

                player.Teleport(position);
            }

            if (showMessage)
            {
                ShowMessage("Loaded prototype state.");
            }
        }

        public void HandleInteraction(ElementbornPrototypeInteractable interactable)
        {
            if (interactable == null)
            {
                return;
            }

            switch (interactable.kind)
            {
                case ElementbornPrototypeInteractableKind.GuideNpc:
                    HandleGuideNpc();
                    break;

                case ElementbornPrototypeInteractableKind.ShardResource:
                    HandleShardResource(interactable);
                    break;

                case ElementbornPrototypeInteractableKind.ReturnPoint:
                    HandleReturnPoint();
                    break;

                default:
                    ShowMessage(interactable.displayName);
                    break;
            }
        }

        private void HandleGuideNpc()
        {
            if (questState == ElementbornPrototypeQuestState.NotStarted)
            {
                questState = ElementbornPrototypeQuestState.TalkedToGuide;
                ShowMessage("Ember Guide: Please recover the glowing shard near the basalt stones.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.TalkedToGuide)
            {
                ShowMessage("Ember Guide: The shard is east of here. Look for the glowing crystal.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                ShowMessage("Ember Guide: Take the shard to the return pedestal.");
                return;
            }

            ShowMessage("Ember Guide: The prototype loop is complete. Nice!");
        }

        private void HandleShardResource(ElementbornPrototypeInteractable interactable)
        {
            if (questState == ElementbornPrototypeQuestState.NotStarted)
            {
                ShowMessage("The shard hums, but you should talk to the guide first.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.TalkedToGuide)
            {
                questState = ElementbornPrototypeQuestState.CollectedShard;
                interactable.gameObject.SetActive(false);
                ShowMessage("Collected the glowing shard. Return it to the pedestal.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                ShowMessage("You already have the shard. Return it to the pedestal.");
                return;
            }

            ShowMessage("The shard objective is already complete.");
        }

        private void HandleReturnPoint()
        {
            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                questState = ElementbornPrototypeQuestState.Completed;
                Save();
                ShowMessage("Quest complete! Prototype loop saved.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.Completed)
            {
                ShowMessage("The pedestal glows softly. Quest already complete.");
                return;
            }

            ShowMessage("The pedestal waits for a shard.");
        }

        public string GetObjectiveText()
        {
            switch (questState)
            {
                case ElementbornPrototypeQuestState.NotStarted:
                    return "Objective: Talk to Ember Guide.";
                case ElementbornPrototypeQuestState.TalkedToGuide:
                    return "Objective: Collect the glowing shard.";
                case ElementbornPrototypeQuestState.CollectedShard:
                    return "Objective: Return the shard to the pedestal.";
                case ElementbornPrototypeQuestState.Completed:
                    return "Objective: Complete. Explore the prototype area.";
                default:
                    return "Objective: Unknown.";
            }
        }

        public void ShowMessage(string text)
        {
            message = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            messageUntil = Time.time + 5f;
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<ElementbornPrototypePlayerController>();
            }

            if (playCamera == null)
            {
                playCamera = Camera.main;
            }
        }

        private void OnGUI()
        {
            if (menuOpen)
            {
                DrawMenu();
                return;
            }

            if (showHud)
            {
                DrawHud();
            }
        }

        private void DrawMenu()
        {
            const int width = 520;
            const int height = 310;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(rect, "Elementborn Prototype");

            GUI.Label(new Rect(rect.x + 24, rect.y + 44, width - 48, 42),
                "A small playable loop for testing movement, camera, interaction, quest state, and save/load.");

            if (GUI.Button(new Rect(rect.x + 145, rect.y + 98, 230, 34), "Start / Resume"))
            {
                StartPrototype();
            }

            if (GUI.Button(new Rect(rect.x + 145, rect.y + 140, 230, 34), "Save"))
            {
                Save();
            }

            if (GUI.Button(new Rect(rect.x + 145, rect.y + 182, 230, 34), "Load"))
            {
                LoadIfPresent(true);
                menuOpen = false;
            }

            if (GUI.Button(new Rect(rect.x + 145, rect.y + 224, 230, 34), "Reset Prototype"))
            {
                ResetPrototype();
            }

            GUI.Label(new Rect(rect.x + 24, rect.y + 270, width - 48, 22), "Esc toggles menu. F5 saves. F9 loads. F1 toggles debug help.");
        }

        private void DrawHud()
        {
            Rect rect = new Rect(16, 16, 520, showDebugHelp ? 132 : 82);
            GUI.Box(rect, "Elementborn Prototype HUD");
            GUI.Label(new Rect(28, 42, 496, 20), GetObjectiveText());

            if (Time.time < messageUntil && !string.IsNullOrWhiteSpace(message))
            {
                GUI.Label(new Rect(28, 64, 496, 22), message);
            }

            if (showDebugHelp)
            {
                GUI.Label(new Rect(28, 90, 496, 20), "Move: WASD / Arrows | Sprint: Shift | Jump: Space | Interact: E");
                GUI.Label(new Rect(28, 110, 496, 20), "Menu: Esc | Save: F5 | Load: F9 | Toggle help: F1");
            }
        }
    }
}
