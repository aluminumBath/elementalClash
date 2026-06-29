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
    /// Self-contained prototype loop:
    /// New prototype -> choose element -> talk to NPC -> collect shard -> return shard -> test ability -> save/load.
    ///
    /// v82:
    /// - No longer auto-loads an old completed PlayerPrefs save by default.
    /// - Adds explicit New Prototype / Resume Saved paths.
    /// - Resets shard/dummy scene state when starting fresh.
    /// </summary>
    public sealed class ElementbornPrototypeGameManager : MonoBehaviour
    {
        public const string SaveKeyQuestState = "Elementborn.Prototype.QuestState";
        public const string SaveKeyPlayerX = "Elementborn.Prototype.PlayerX";
        public const string SaveKeyPlayerY = "Elementborn.Prototype.PlayerY";
        public const string SaveKeyPlayerZ = "Elementborn.Prototype.PlayerZ";
        public const string SaveKeyElement = "Elementborn.Prototype.Element";

        public static ElementbornPrototypeGameManager Instance { get; private set; }

        [Header("Runtime References")]
        public ElementbornPrototypePlayerController player;
        public Camera playCamera;

        [Header("Prototype State")]
        public bool menuOpen = true;
        public bool loadSavedStateOnAwake = false;
        public ElementbornPrototypeQuestState questState = ElementbornPrototypeQuestState.NotStarted;
        public ElementbornPrototypeElementType selectedElement = ElementbornPrototypeElementType.Fire;

        [Header("UI")]
        public bool showHud = true;
        public bool showDebugHelp = true;

        private string message = "Welcome to Elementborn.";
        private float messageUntil;
        private string dialogueSpeaker = "";
        private string dialogueText = "";
        private float dialogueUntil;

        public bool HasStarted => !menuOpen;

        private void Awake()
        {
            Instance = this;
            ResolveReferences();

            if (loadSavedStateOnAwake)
            {
                LoadIfPresent(false);
            }
            else
            {
                ResetSceneRuntimeState(false);
                ApplySelectedElement();
                ShowMessage("Choose New Prototype or Resume Saved.");
            }
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
            ApplySelectedElement();
            ShowMessage("Prototype started. Talk to Ember Guide with E.");
        }

        public void StartNewPrototype()
        {
            ClearSavedState();
            questState = ElementbornPrototypeQuestState.NotStarted;
            ResetSceneRuntimeState(true);
            ApplySelectedElement();
            menuOpen = false;
            ShowDialogue("Ember Guide", "Welcome, channeler. Speak with me, then recover the glowing shard.");
        }

        public void ResumeSavedPrototype()
        {
            LoadIfPresent(true);
            menuOpen = false;
        }

        public void SetElement(ElementbornPrototypeElementType element)
        {
            selectedElement = element;
            ApplySelectedElement();
            ShowMessage("Attuned to " + ElementbornPrototypeVisualUtility.GetElementName(element) + ". Press Q to cast.");
        }

        private void ApplySelectedElement()
        {
            ElementbornPrototypeElementalAbility ability = player != null ? player.GetComponent<ElementbornPrototypeElementalAbility>() : null;
            if (ability != null)
            {
                ability.currentElement = selectedElement;
            }

            ElementbornPrototypePlayerVisual visual = player != null ? player.GetComponent<ElementbornPrototypePlayerVisual>() : null;
            if (visual != null)
            {
                visual.ApplyElement(selectedElement);
            }
        }

        public void ResetPrototype()
        {
            StartNewPrototype();
        }

        public void ClearSavedState()
        {
            PlayerPrefs.DeleteKey(SaveKeyQuestState);
            PlayerPrefs.DeleteKey(SaveKeyPlayerX);
            PlayerPrefs.DeleteKey(SaveKeyPlayerY);
            PlayerPrefs.DeleteKey(SaveKeyPlayerZ);
            PlayerPrefs.DeleteKey(SaveKeyElement);
            PlayerPrefs.Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(SaveKeyQuestState, (int)questState);
            PlayerPrefs.SetInt(SaveKeyElement, (int)selectedElement);

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
            else
            {
                questState = ElementbornPrototypeQuestState.NotStarted;
            }

            if (PlayerPrefs.HasKey(SaveKeyElement))
            {
                selectedElement = (ElementbornPrototypeElementType)PlayerPrefs.GetInt(SaveKeyElement, 0);
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

            ResetSceneRuntimeState(false);
            ApplySceneForQuestState();
            ApplySelectedElement();

            if (showMessage)
            {
                ShowMessage("Loaded prototype state: " + questState + ".");
            }
        }

        public void ResetSceneRuntimeState(bool resetPlayer)
        {
            ResolveReferences();

            if (resetPlayer && player != null)
            {
                player.Teleport(new Vector3(0f, 1f, -8f));
            }

            ElementbornPrototypeInteractable[] interactables =
                FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null)
                {
                    continue;
                }

                if (interactable.kind == ElementbornPrototypeInteractableKind.ShardResource)
                {
                    interactable.gameObject.SetActive(true);
                }

                interactable.EnsureInteractionRadius();
            }

            ElementbornPrototypeDummyEnemy[] dummies =
                FindObjectsByType<ElementbornPrototypeDummyEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < dummies.Length; i++)
            {
                if (dummies[i] != null)
                {
                    dummies[i].ResetDummy();
                }
            }

            dialogueText = "";
            dialogueSpeaker = "";
            dialogueUntil = 0f;
        }

        private void ApplySceneForQuestState()
        {
            ElementbornPrototypeInteractable[] interactables =
                FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null)
                {
                    continue;
                }

                if (interactable.kind == ElementbornPrototypeInteractableKind.ShardResource)
                {
                    bool shouldShowShard =
                        questState == ElementbornPrototypeQuestState.NotStarted ||
                        questState == ElementbornPrototypeQuestState.TalkedToGuide;

                    interactable.gameObject.SetActive(shouldShowShard);
                }
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
                ShowDialogue("Ember Guide", "You feel the world answering your element. Recover the glowing shard near the basalt stones, then try your first channeling bolt with Q.");
                ApplySceneForQuestState();
                return;
            }

            if (questState == ElementbornPrototypeQuestState.TalkedToGuide)
            {
                ShowDialogue("Ember Guide", "The shard is east of here. The training dummy is beyond it if you want to test your attunement.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                ShowDialogue("Ember Guide", "Take the shard to the return pedestal, then cast at the training dummy.");
                return;
            }

            ShowDialogue("Ember Guide", "The prototype loop is complete. Start a New Prototype from the menu to replay it, or keep testing elemental bolts on the dummy.");
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

            ShowMessage("The shard objective is already complete. Start a New Prototype from the menu to replay.");
        }

        private void HandleReturnPoint()
        {
            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                questState = ElementbornPrototypeQuestState.Completed;
                ApplySceneForQuestState();
                Save();
                ShowMessage("Quest complete! Prototype loop saved. Press Q to test your elemental bolt on the dummy.");
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
                    return "Objective: Test your elemental bolt on the training dummy.";
                default:
                    return "Objective: Unknown.";
            }
        }

        public void ShowMessage(string text)
        {
            message = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            messageUntil = Time.time + 5f;
        }

        public void ShowDialogue(string speaker, string text)
        {
            dialogueSpeaker = string.IsNullOrWhiteSpace(speaker) ? "Elementborn" : speaker;
            dialogueText = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            dialogueUntil = Time.time + 7f;
            ShowMessage(dialogueSpeaker + ": " + dialogueText);
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
                DrawDialogue();
            }
        }

        private void DrawMenu()
        {
            const int width = 600;
            const int height = 430;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(rect, "Elementborn Prototype");

            GUI.Label(new Rect(rect.x + 24, rect.y + 42, width - 48, 42),
                "Choose an element, start fresh, or resume your saved prototype loop.");

            GUI.Label(new Rect(rect.x + 80, rect.y + 92, 440, 22), "Attunement: " + ElementbornPrototypeVisualUtility.GetElementName(selectedElement));

            if (GUI.Button(new Rect(rect.x + 80, rect.y + 120, 100, 30), "Fire"))
            {
                SetElement(ElementbornPrototypeElementType.Fire);
            }

            if (GUI.Button(new Rect(rect.x + 190, rect.y + 120, 100, 30), "Water"))
            {
                SetElement(ElementbornPrototypeElementType.Water);
            }

            if (GUI.Button(new Rect(rect.x + 300, rect.y + 120, 100, 30), "Earth"))
            {
                SetElement(ElementbornPrototypeElementType.Earth);
            }

            if (GUI.Button(new Rect(rect.x + 410, rect.y + 120, 100, 30), "Air"))
            {
                SetElement(ElementbornPrototypeElementType.Air);
            }

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 172, 230, 34), "New Prototype"))
            {
                StartNewPrototype();
            }

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 214, 230, 34), "Resume Saved"))
            {
                ResumeSavedPrototype();
            }

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 256, 230, 34), "Save Current"))
            {
                Save();
            }

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 298, 230, 34), "Clear Save"))
            {
                ClearSavedState();
                questState = ElementbornPrototypeQuestState.NotStarted;
                ResetSceneRuntimeState(true);
                ShowMessage("Save cleared. Choose New Prototype.");
            }

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 340, 230, 34), "Start Without Loading"))
            {
                StartPrototype();
            }

            GUI.Label(new Rect(rect.x + 24, rect.y + 392, width - 48, 22), "Esc menu | F5 save | F9 load | Q cast | 1-4 switch elements.");
        }

        private void DrawHud()
        {
            Rect rect = new Rect(16, 16, 600, showDebugHelp ? 174 : 114);
            GUI.Box(rect, "Elementborn Prototype HUD");
            GUI.Label(new Rect(28, 42, 560, 20), GetObjectiveText());
            GUI.Label(new Rect(28, 64, 560, 20), "Attunement: " + ElementbornPrototypeVisualUtility.GetElementName(selectedElement) + " | Ability: Q");

            if (Time.time < messageUntil && !string.IsNullOrWhiteSpace(message))
            {
                GUI.Label(new Rect(28, 86, 560, 34), message);
            }

            if (showDebugHelp)
            {
                GUI.Label(new Rect(28, 126, 560, 20), "Move: WASD / Arrows | Sprint: Shift | Jump: Space | Interact: E | Cast: Q");
                GUI.Label(new Rect(28, 146, 560, 20), "Elements: 1 Fire, 2 Water, 3 Earth, 4 Air | Menu: Esc | Save: F5 | Load: F9");
            }
        }

        private void DrawDialogue()
        {
            if (Time.time >= dialogueUntil || string.IsNullOrWhiteSpace(dialogueText))
            {
                return;
            }

            const int width = 760;
            const int height = 104;
            Rect rect = new Rect((Screen.width - width) / 2f, Screen.height - height - 36f, width, height);
            GUI.Box(rect, dialogueSpeaker);
            GUI.Label(new Rect(rect.x + 24, rect.y + 36, width - 48, 54), dialogueText);
        }
    }
}
