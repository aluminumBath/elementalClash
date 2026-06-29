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

    public sealed class ElementbornPrototypeGameManager : MonoBehaviour
    {
        public const string SaveKeyQuestState = "Elementborn.Prototype.QuestState";
        public const string SaveKeyPlayerX = "Elementborn.Prototype.PlayerX";
        public const string SaveKeyPlayerY = "Elementborn.Prototype.PlayerY";
        public const string SaveKeyPlayerZ = "Elementborn.Prototype.PlayerZ";
        public const string SaveKeyElement = "Elementborn.Prototype.Element";
        public const string SaveKeyPathChoice = "Elementborn.Prototype.PathChoice";

        public static ElementbornPrototypeGameManager Instance { get; private set; }

        [Header("Runtime References")]
        public ElementbornPrototypePlayerController player;
        public Camera playCamera;

        [Header("Prototype State")]
        public bool menuOpen = true;
        public bool loadSavedStateOnAwake = false;
        public ElementbornPrototypeQuestState questState = ElementbornPrototypeQuestState.NotStarted;
        public ElementbornPrototypeElementType selectedElement = ElementbornPrototypeElementType.Fire;
        public ElementbornPrototypePathChoice pathChoice = ElementbornPrototypePathChoice.None;

        [Header("UI")]
        public bool showHud = true;
        public bool showDebugHelp = true;

        private string message = "Welcome to Elementborn.";
        private float messageUntil;
        private string dialogueSpeaker = "";
        private string dialogueText = "";
        private float dialogueUntil;
        private bool guideChoiceOpen;

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
                guideChoiceOpen = false;
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
            pathChoice = ElementbornPrototypePathChoice.None;
            ResetSceneRuntimeState(true);
            ApplySelectedElement();
            menuOpen = false;
            ShowDialogue("Ember Guide", "Welcome, channeler. Speak with me, then choose how you want to use your element.");
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
            PlayerPrefs.DeleteKey(SaveKeyPathChoice);
            PlayerPrefs.Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(SaveKeyQuestState, (int)questState);
            PlayerPrefs.SetInt(SaveKeyElement, (int)selectedElement);
            PlayerPrefs.SetInt(SaveKeyPathChoice, (int)pathChoice);

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
            questState = PlayerPrefs.HasKey(SaveKeyQuestState)
                ? (ElementbornPrototypeQuestState)PlayerPrefs.GetInt(SaveKeyQuestState, 0)
                : ElementbornPrototypeQuestState.NotStarted;

            if (PlayerPrefs.HasKey(SaveKeyElement))
            {
                selectedElement = (ElementbornPrototypeElementType)PlayerPrefs.GetInt(SaveKeyElement, 0);
            }

            if (PlayerPrefs.HasKey(SaveKeyPathChoice))
            {
                pathChoice = (ElementbornPrototypePathChoice)PlayerPrefs.GetInt(SaveKeyPathChoice, 0);
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

                ElementbornPrototypePlayerStats stats = player.GetComponent<ElementbornPrototypePlayerStats>();
                if (stats != null)
                {
                    stats.HealToFull();
                }
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

            ElementbornPrototypeHostileEnemy[] hostiles =
                FindObjectsByType<ElementbornPrototypeHostileEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < hostiles.Length; i++)
            {
                if (hostiles[i] != null)
                {
                    hostiles[i].ResetEnemy();
                }
            }

            dialogueText = "";
            dialogueSpeaker = "";
            dialogueUntil = 0f;
            guideChoiceOpen = false;
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
                guideChoiceOpen = true;
                ShowDialogue("Ember Guide", "Your element can unify people or dominate them. Choose how this prototype remembers your first stance.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.TalkedToGuide)
            {
                string stance = pathChoice == ElementbornPrototypePathChoice.Dominion
                    ? "Power without restraint invites fear. Prove you can control it."
                    : "Unity requires action, not slogans. Recover the shard and return it safely.";
                ShowDialogue("Ember Guide", stance + " The shard is east of here.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                ShowDialogue("Ember Guide", "Take the shard to the return pedestal, then test your channeling on the dummy or hostile.");
                return;
            }

            ShowDialogue("Ember Guide", "The prototype loop is complete. The next step is making the factions and choices matter more.");
        }

        public void ChoosePath(ElementbornPrototypePathChoice choice)
        {
            pathChoice = choice;
            questState = ElementbornPrototypeQuestState.TalkedToGuide;
            guideChoiceOpen = false;

            if (choice == ElementbornPrototypePathChoice.Dominion)
            {
                ShowDialogue("Ember Guide", "Dominion chosen. Power can protect, but it can also corrupt. Recover the shard and prove discipline.");
            }
            else
            {
                ShowDialogue("Ember Guide", "Unifier chosen. Bring the shard back as a promise that elements can coexist.");
            }

            ApplySceneForQuestState();
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
                ShowMessage("Quest complete! Prototype loop saved. Press Q to test your elemental bolt.");
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
            if (guideChoiceOpen)
            {
                return "Objective: Choose Unifier or Dominion.";
            }

            switch (questState)
            {
                case ElementbornPrototypeQuestState.NotStarted:
                    return "Objective: Talk to Ember Guide.";
                case ElementbornPrototypeQuestState.TalkedToGuide:
                    return "Objective: Collect the glowing shard.";
                case ElementbornPrototypeQuestState.CollectedShard:
                    return "Objective: Return the shard to the pedestal.";
                case ElementbornPrototypeQuestState.Completed:
                    return "Objective: Test your elemental bolt on the dummy/hostile.";
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
            dialogueUntil = Time.time + 8f;
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
                DrawGuideChoice();
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

            if (GUI.Button(new Rect(rect.x + 80, rect.y + 120, 100, 30), "Fire")) SetElement(ElementbornPrototypeElementType.Fire);
            if (GUI.Button(new Rect(rect.x + 190, rect.y + 120, 100, 30), "Water")) SetElement(ElementbornPrototypeElementType.Water);
            if (GUI.Button(new Rect(rect.x + 300, rect.y + 120, 100, 30), "Earth")) SetElement(ElementbornPrototypeElementType.Earth);
            if (GUI.Button(new Rect(rect.x + 410, rect.y + 120, 100, 30), "Air")) SetElement(ElementbornPrototypeElementType.Air);

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 172, 230, 34), "New Prototype")) StartNewPrototype();
            if (GUI.Button(new Rect(rect.x + 185, rect.y + 214, 230, 34), "Resume Saved")) ResumeSavedPrototype();
            if (GUI.Button(new Rect(rect.x + 185, rect.y + 256, 230, 34), "Save Current")) Save();

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 298, 230, 34), "Clear Save"))
            {
                ClearSavedState();
                questState = ElementbornPrototypeQuestState.NotStarted;
                pathChoice = ElementbornPrototypePathChoice.None;
                ResetSceneRuntimeState(true);
                ShowMessage("Save cleared. Choose New Prototype.");
            }

            if (GUI.Button(new Rect(rect.x + 185, rect.y + 340, 230, 34), "Start Without Loading")) StartPrototype();

            GUI.Label(new Rect(rect.x + 24, rect.y + 392, width - 48, 22), "Esc menu | F5 save | F9 load | Q cast | 1-4 switch elements.");
        }

        private void DrawHud()
        {
            ElementbornPrototypePlayerStats stats = player != null ? player.GetComponent<ElementbornPrototypePlayerStats>() : null;
            ElementbornPrototypeElementalAbility ability = player != null ? player.GetComponent<ElementbornPrototypeElementalAbility>() : null;

            Rect rect = new Rect(16, 16, 640, showDebugHelp ? 214 : 154);
            GUI.Box(rect, "Elementborn Prototype HUD");
            GUI.Label(new Rect(28, 42, 600, 20), GetObjectiveText());
            GUI.Label(new Rect(28, 64, 600, 20), "Attunement: " + ElementbornPrototypeVisualUtility.GetElementName(selectedElement) + " | Stance: " + pathChoice);

            if (stats != null)
            {
                GUI.Label(new Rect(28, 88, 280, 20), "Health: " + Mathf.CeilToInt(stats.currentHealth) + "/" + Mathf.CeilToInt(stats.maxHealth));
                GUI.HorizontalScrollbar(new Rect(120, 90, 170, 18), 0f, stats.Health01, 0f, 1f);

                GUI.Label(new Rect(320, 88, 280, 20), "Stamina: " + Mathf.CeilToInt(stats.currentStamina) + "/" + Mathf.CeilToInt(stats.maxStamina));
                GUI.HorizontalScrollbar(new Rect(425, 90, 170, 18), 0f, stats.Stamina01, 0f, 1f);
            }

            if (ability != null)
            {
                GUI.Label(new Rect(28, 112, 560, 20), "Ability: Q elemental bolt | Cooldown: " + ability.CooldownRemaining.ToString("0.0") + "s");
                GUI.HorizontalScrollbar(new Rect(180, 114, 170, 18), 0f, 1f - ability.Cooldown01, 0f, 1f);
            }

            if (Time.time < messageUntil && !string.IsNullOrWhiteSpace(message))
            {
                GUI.Label(new Rect(28, 136, 590, 34), message);
            }

            if (showDebugHelp)
            {
                GUI.Label(new Rect(28, 176, 600, 20), "Move: WASD / Arrows | Sprint uses stamina: Shift | Jump costs stamina: Space | Interact: E | Cast: Q");
                GUI.Label(new Rect(28, 196, 600, 20), "Elements: 1 Fire, 2 Water, 3 Earth, 4 Air | Menu: Esc | Save: F5 | Load: F9");
            }
        }

        private void DrawDialogue()
        {
            if (Time.time >= dialogueUntil || string.IsNullOrWhiteSpace(dialogueText))
            {
                return;
            }

            const int width = 790;
            const int height = 114;
            Rect rect = new Rect((Screen.width - width) / 2f, Screen.height - height - 36f, width, height);
            GUI.Box(rect, dialogueSpeaker);
            GUI.Label(new Rect(rect.x + 24, rect.y + 36, width - 48, 64), dialogueText);
        }

        private void DrawGuideChoice()
        {
            if (!guideChoiceOpen)
            {
                return;
            }

            const int width = 520;
            const int height = 118;
            Rect rect = new Rect((Screen.width - width) / 2f, Screen.height - 260f, width, height);
            GUI.Box(rect, "Choose your first stance");

            if (GUI.Button(new Rect(rect.x + 38, rect.y + 48, 200, 40), "Unifier"))
            {
                ChoosePath(ElementbornPrototypePathChoice.Unifier);
            }

            if (GUI.Button(new Rect(rect.x + 282, rect.y + 48, 200, 40), "Dominion"))
            {
                ChoosePath(ElementbornPrototypePathChoice.Dominion);
            }
        }
    }
}
