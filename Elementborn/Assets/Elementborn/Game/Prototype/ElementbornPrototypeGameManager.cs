using UnityEngine;

namespace Elementborn.Game
{
    public enum ElementbornPrototypeQuestState
    {
        NotStarted,
        TalkedToGuide,
        CollectedShard,
        Completed,
        OpenedElementGate,
        DefeatedHostile,
        GatheredEssence,
        OpenedRewardChest,
        ZoneComplete
    }

    public sealed class ElementbornPrototypeGameManager : MonoBehaviour
    {
        public const string SaveKeyQuestState = "Elementborn.Prototype.QuestState";
        public const string SaveKeyPlayerX = "Elementborn.Prototype.PlayerX";
        public const string SaveKeyPlayerY = "Elementborn.Prototype.PlayerY";
        public const string SaveKeyPlayerZ = "Elementborn.Prototype.PlayerZ";
        public const string SaveKeyElement = "Elementborn.Prototype.Element";
        public const string SaveKeyPathChoice = "Elementborn.Prototype.PathChoice";
        public const string SaveKeyEssence = "Elementborn.Prototype.Essence";
        public const string SaveKeyCoins = "Elementborn.Prototype.Coins";
        public const string SaveKeyLore = "Elementborn.Prototype.Lore";
        public const string SaveKeyChests = "Elementborn.Prototype.Chests";
        public const string SaveKeyZoneLevel = "Elementborn.Prototype.ZoneLevel";
        public const string SaveKeyRelic = "Elementborn.Prototype.Relic";

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

        [Header("Inventory / Progression")]
        public int elementalEssence;
        public int coins;
        public int loreDiscovered;
        public int chestsOpened;
        public int zoneLevel = 1;
        public int essenceNeededForReward = 3;
        public bool hasPrototypeRelic;

        [Header("UI")]
        public bool showHud = true;
        public bool showDebugHelp = true;
        public bool showJournal;
        public bool showMessageLog = true;
        public bool showCompass = true;
        public float hudMessageSeconds = 11f;
        public float dialogueSeconds = 16f;
        public int messageLogLines = 5;

        private string message = "Welcome to Elementborn.";
        private float messageUntil;
        private string dialogueSpeaker = "";
        private string dialogueText = "";
        private float dialogueUntil;
        private bool guideChoiceOpen;
        private readonly string[] messageHistory = new string[8];

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

            if (Input.GetKeyDown(KeyCode.J))
            {
                showJournal = !showJournal;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                showMessageLog = !showMessageLog;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                showCompass = !showCompass;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                DismissDialogue();
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
            elementalEssence = 0;
            coins = 0;
            loreDiscovered = 0;
            chestsOpened = 0;
            zoneLevel = 1;
            hasPrototypeRelic = false;
            ResetMessageHistory();
            ResetSceneRuntimeState(true);
            ApplySelectedElement();
            menuOpen = false;
            ShowDialogue("Ember Guide", "Welcome, channeler. This hub now has envoys, lore, shrines, resources, gates, and a reward chest. Speak with me to begin.");
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
            PlayerPrefs.DeleteKey(SaveKeyEssence);
            PlayerPrefs.DeleteKey(SaveKeyCoins);
            PlayerPrefs.DeleteKey(SaveKeyLore);
            PlayerPrefs.DeleteKey(SaveKeyChests);
            PlayerPrefs.DeleteKey(SaveKeyZoneLevel);
            PlayerPrefs.DeleteKey(SaveKeyRelic);
            PlayerPrefs.Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(SaveKeyQuestState, (int)questState);
            PlayerPrefs.SetInt(SaveKeyElement, (int)selectedElement);
            PlayerPrefs.SetInt(SaveKeyPathChoice, (int)pathChoice);
            PlayerPrefs.SetInt(SaveKeyEssence, elementalEssence);
            PlayerPrefs.SetInt(SaveKeyCoins, coins);
            PlayerPrefs.SetInt(SaveKeyLore, loreDiscovered);
            PlayerPrefs.SetInt(SaveKeyChests, chestsOpened);
            PlayerPrefs.SetInt(SaveKeyZoneLevel, zoneLevel);
            PlayerPrefs.SetInt(SaveKeyRelic, hasPrototypeRelic ? 1 : 0);

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

            elementalEssence = PlayerPrefs.GetInt(SaveKeyEssence, elementalEssence);
            coins = PlayerPrefs.GetInt(SaveKeyCoins, coins);
            loreDiscovered = PlayerPrefs.GetInt(SaveKeyLore, loreDiscovered);
            chestsOpened = PlayerPrefs.GetInt(SaveKeyChests, chestsOpened);
            zoneLevel = PlayerPrefs.GetInt(SaveKeyZoneLevel, zoneLevel);
            hasPrototypeRelic = PlayerPrefs.GetInt(SaveKeyRelic, hasPrototypeRelic ? 1 : 0) != 0;

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

                if (interactable.kind == ElementbornPrototypeInteractableKind.ResourceNode ||
                    interactable.kind == ElementbornPrototypeInteractableKind.LootChest)
                {
                    interactable.ResetInteractable();
                }

                interactable.ResolveGateController();
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

            ElementbornPrototypeElementGate[] gates =
                FindObjectsByType<ElementbornPrototypeElementGate>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < gates.Length; i++)
            {
                if (gates[i] != null)
                {
                    gates[i].CloseGate();
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
                case ElementbornPrototypeInteractableKind.ElementGate:
                    HandleElementGate(interactable);
                    break;
                case ElementbornPrototypeInteractableKind.ResourceNode:
                    HandleResourceNode(interactable);
                    break;
                case ElementbornPrototypeInteractableKind.HealingShrine:
                    HandleHealingShrine(interactable);
                    break;
                case ElementbornPrototypeInteractableKind.LootChest:
                    HandleLootChest(interactable);
                    break;
                case ElementbornPrototypeInteractableKind.LoreStone:
                    HandleLoreStone(interactable);
                    break;
                case ElementbornPrototypeInteractableKind.EnvoyNpc:
                    HandleEnvoyNpc(interactable);
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
                ShowDialogue("Ember Guide", stance + " The shard is east of here. Envoys around the hub will teach you about each path.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.CollectedShard)
            {
                ShowDialogue("Ember Guide", "Take the shard to the return pedestal. The gates will answer after the shard is returned.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.Completed)
            {
                ShowDialogue("Ember Guide", "Now attune to an element and open its matching gate. The hub is beginning to wake.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.OpenedElementGate)
            {
                ShowDialogue("Ember Guide", "The gate is open. Defeat the hostile to prove this route is safe.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.DefeatedHostile)
            {
                ShowDialogue("Ember Guide", "Good. Now gather " + essenceNeededForReward + " elemental essence from resource nodes, then open the reward chest.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.GatheredEssence)
            {
                ShowDialogue("Ember Guide", "You have enough essence. Open the Convergence Reward Chest near the center.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.OpenedRewardChest)
            {
                ShowDialogue("Ember Guide", "Read a lore stone to complete the zone and learn why the factions fight.");
                return;
            }

            ShowDialogue("Ember Guide", "Zone test complete. Next we can add real models, loot equipment, and faction consequences.");
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
                ShowMessage("Quest complete. Now open your matching elemental gate.");
                return;
            }

            if ((int)questState >= (int)ElementbornPrototypeQuestState.Completed)
            {
                ShowMessage("The pedestal glows softly. The shard is stable.");
                return;
            }

            ShowMessage("The pedestal waits for a shard.");
        }

        private void HandleElementGate(ElementbornPrototypeInteractable interactable)
        {
            interactable.ResolveGateController();

            if ((int)questState < (int)ElementbornPrototypeQuestState.Completed)
            {
                ShowMessage("The gates remain sealed until the shard is returned.");
                return;
            }

            if (interactable.gateController != null && interactable.gateController.isOpen)
            {
                ShowMessage("The " + ElementbornPrototypeVisualUtility.GetElementName(interactable.gateElement) + " Gate is already open.");
                return;
            }

            if (selectedElement != interactable.gateElement)
            {
                ShowMessage("This gate needs " + ElementbornPrototypeVisualUtility.GetElementName(interactable.gateElement) + " attunement. Switch elements with 1-4.");
                return;
            }

            if (interactable.gateController != null)
            {
                interactable.gateController.OpenGate();
            }

            if (questState == ElementbornPrototypeQuestState.Completed)
            {
                questState = ElementbornPrototypeQuestState.OpenedElementGate;
            }

            Save();
            ShowDialogue("Ember Guide", "The " + ElementbornPrototypeVisualUtility.GetElementName(interactable.gateElement) + " Gate opens. Defeat the hostile to secure the route.");
        }

        private void HandleResourceNode(ElementbornPrototypeInteractable interactable)
        {
            if (interactable.consumed)
            {
                ShowMessage(interactable.displayName + " is depleted.");
                return;
            }

            elementalEssence += Mathf.Max(1, interactable.amount);
            interactable.MarkConsumed();

            ShowMessage("Harvested " + interactable.displayName + ". Essence: " + elementalEssence + "/" + essenceNeededForReward + ".");

            if (questState == ElementbornPrototypeQuestState.DefeatedHostile &&
                elementalEssence >= essenceNeededForReward)
            {
                questState = ElementbornPrototypeQuestState.GatheredEssence;
                ShowDialogue("Ember Guide", "Enough essence gathered. Open the Convergence Reward Chest.");
            }

            Save();
        }

        private void HandleHealingShrine(ElementbornPrototypeInteractable interactable)
        {
            ElementbornPrototypePlayerStats stats = player != null ? player.GetComponent<ElementbornPrototypePlayerStats>() : null;
            if (stats != null)
            {
                stats.HealToFull();
            }

            ShowMessage("Rested at " + interactable.displayName + ". Health and stamina restored.");
        }

        private void HandleLootChest(ElementbornPrototypeInteractable interactable)
        {
            if (interactable.consumed)
            {
                ShowMessage(interactable.displayName + " is already open.");
                return;
            }

            if (questState == ElementbornPrototypeQuestState.DefeatedHostile &&
                elementalEssence < essenceNeededForReward)
            {
                ShowMessage("The chest needs " + essenceNeededForReward + " elemental essence. You have " + elementalEssence + ".");
                return;
            }

            interactable.MarkConsumed();
            coins += 25;
            chestsOpened++;

            if (interactable.displayName.Contains("Convergence"))
            {
                hasPrototypeRelic = true;
            }

            if (questState == ElementbornPrototypeQuestState.GatheredEssence ||
                questState == ElementbornPrototypeQuestState.DefeatedHostile)
            {
                questState = ElementbornPrototypeQuestState.OpenedRewardChest;
            }

            string rewardText = hasPrototypeRelic
                ? "You found 25 coins and equipped the Prototype Convergence Relic. Read a lore stone to complete the zone."
                : "You found 25 coins. Read a lore stone to complete the zone.";
            ShowDialogue(interactable.displayName, rewardText);
            Save();
        }

        private void HandleLoreStone(ElementbornPrototypeInteractable interactable)
        {
            loreDiscovered++;

            string text = string.IsNullOrWhiteSpace(interactable.customText)
                ? "The central city remembers every element, but not every faction agrees that power should be shared."
                : interactable.customText;

            if (questState == ElementbornPrototypeQuestState.OpenedRewardChest)
            {
                questState = ElementbornPrototypeQuestState.ZoneComplete;
                zoneLevel++;
                Save();
                ShowDialogue(interactable.displayName, text + " Zone complete. Prototype level increased to " + zoneLevel + ".");
                return;
            }

            ShowDialogue(interactable.displayName, text);
            Save();
        }

        private void HandleEnvoyNpc(ElementbornPrototypeInteractable interactable)
        {
            string text = string.IsNullOrWhiteSpace(interactable.customText)
                ? "The envoy studies your stance carefully."
                : interactable.customText;

            ShowDialogue(interactable.displayName, text);
        }

        public void NotifyHostileDefeated(ElementbornPrototypeHostileEnemy hostile)
        {
            if (questState == ElementbornPrototypeQuestState.OpenedElementGate)
            {
                questState = ElementbornPrototypeQuestState.DefeatedHostile;
                Save();
                ShowDialogue("Ember Guide", "Route secured. Now gather " + essenceNeededForReward + " elemental essence from glowing resource nodes.");
            }
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
                    return "Objective: Attune and open the matching elemental gate.";
                case ElementbornPrototypeQuestState.OpenedElementGate:
                    return "Objective: Defeat the hostile.";
                case ElementbornPrototypeQuestState.DefeatedHostile:
                    return "Objective: Gather elemental essence " + elementalEssence + "/" + essenceNeededForReward + ".";
                case ElementbornPrototypeQuestState.GatheredEssence:
                    return "Objective: Open the Convergence Reward Chest.";
                case ElementbornPrototypeQuestState.OpenedRewardChest:
                    return "Objective: Read a lore stone.";
                case ElementbornPrototypeQuestState.ZoneComplete:
                    return "Objective: Zone complete. Explore, test, or start new.";
                default:
                    return "Objective: Unknown.";
            }
        }

        public string GetJournalText()
        {
            string journal =
                "QUEST JOURNAL\n\n" +
                GetObjectiveText() + "\n\n" +
                "Progress:\n" +
                "- Element: " + ElementbornPrototypeVisualUtility.GetElementName(selectedElement) + "\n" +
                "- Stance: " + pathChoice + "\n" +
                "- Quest State: " + questState + "\n" +
                "- Essence: " + elementalEssence + "/" + essenceNeededForReward + "\n" +
                "- Coins: " + coins + "\n" +
                "- Lore Read: " + loreDiscovered + "\n" +
                "- Chests Opened: " + chestsOpened + "\n" +
                "- Zone Level: " + zoneLevel + "\n" +
                "- Relic: " + (hasPrototypeRelic ? "Prototype Convergence Relic" : "None") + "\n\n" +
                "Prototype asset boards are now placed around the hub as the first pass away from blocky placeholders.\n\n" +
                "Current loop:\n" +
                "1. Talk to Ember Guide\n" +
                "2. Choose Unifier or Dominion\n" +
                "3. Collect and return shard\n" +
                "4. Open matching elemental gate\n" +
                "5. Defeat hostile\n" +
                "6. Gather essence\n" +
                "7. Open reward chest\n" +
                "8. Read lore stone\n\n" +
                "Controls:\n" +
                "J = toggle journal, M = toggle message log, C = toggle compass, Enter = dismiss dialogue.";

            return journal;
        }

        public void ShowMessage(string text)
        {
            message = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            messageUntil = Time.time + Mathf.Max(2f, hudMessageSeconds);
            AddMessageToHistory(message);
        }

        public void ShowDialogue(string speaker, string text)
        {
            dialogueSpeaker = string.IsNullOrWhiteSpace(speaker) ? "Elementborn" : speaker;
            dialogueText = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            dialogueUntil = Time.time + Mathf.Max(4f, dialogueSeconds);
            ShowMessage(dialogueSpeaker + ": " + dialogueText);
        }

        public void DismissDialogue()
        {
            dialogueUntil = 0f;
            dialogueText = "";
            dialogueSpeaker = "";
        }

        private void AddMessageToHistory(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            for (int i = messageHistory.Length - 1; i > 0; i--)
            {
                messageHistory[i] = messageHistory[i - 1];
            }

            messageHistory[0] = text;
        }

        private void ResetMessageHistory()
        {
            for (int i = 0; i < messageHistory.Length; i++)
            {
                messageHistory[i] = "";
            }
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
                DrawCompass();
                DrawMessageLog();
                DrawJournal();
            }
        }

        private void DrawMenu()
        {
            const int width = 620;
            const int height = 450;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(rect, "Elementborn Prototype");

            GUI.Label(new Rect(rect.x + 24, rect.y + 42, width - 48, 42),
                "Choose an element, start fresh, or resume your saved prototype loop.");

            GUI.Label(new Rect(rect.x + 80, rect.y + 92, 460, 22), "Attunement: " + ElementbornPrototypeVisualUtility.GetElementName(selectedElement));

            if (GUI.Button(new Rect(rect.x + 80, rect.y + 120, 100, 30), "Fire")) SetElement(ElementbornPrototypeElementType.Fire);
            if (GUI.Button(new Rect(rect.x + 190, rect.y + 120, 100, 30), "Water")) SetElement(ElementbornPrototypeElementType.Water);
            if (GUI.Button(new Rect(rect.x + 300, rect.y + 120, 100, 30), "Earth")) SetElement(ElementbornPrototypeElementType.Earth);
            if (GUI.Button(new Rect(rect.x + 410, rect.y + 120, 100, 30), "Air")) SetElement(ElementbornPrototypeElementType.Air);

            if (GUI.Button(new Rect(rect.x + 195, rect.y + 172, 230, 34), "New Prototype")) StartNewPrototype();
            if (GUI.Button(new Rect(rect.x + 195, rect.y + 214, 230, 34), "Resume Saved")) ResumeSavedPrototype();
            if (GUI.Button(new Rect(rect.x + 195, rect.y + 256, 230, 34), "Save Current")) Save();

            if (GUI.Button(new Rect(rect.x + 195, rect.y + 298, 230, 34), "Clear Save"))
            {
                ClearSavedState();
                questState = ElementbornPrototypeQuestState.NotStarted;
                pathChoice = ElementbornPrototypePathChoice.None;
                elementalEssence = 0;
                coins = 0;
                loreDiscovered = 0;
                chestsOpened = 0;
                zoneLevel = 1;
                hasPrototypeRelic = false;
                ResetMessageHistory();
                ResetSceneRuntimeState(true);
                ShowMessage("Save cleared. Choose New Prototype.");
            }

            if (GUI.Button(new Rect(rect.x + 195, rect.y + 340, 230, 34), "Start Without Loading")) StartPrototype();

            GUI.Label(new Rect(rect.x + 24, rect.y + 394, width - 48, 42), "Esc menu | F5 save | F9 load | Q cast | 1-4 switch elements | J journal | M log | C compass | Enter dismiss.");
        }

        private void DrawHud()
        {
            ElementbornPrototypePlayerStats stats = player != null ? player.GetComponent<ElementbornPrototypePlayerStats>() : null;
            ElementbornPrototypeElementalAbility ability = player != null ? player.GetComponent<ElementbornPrototypeElementalAbility>() : null;

            Rect rect = new Rect(16, 16, 740, showDebugHelp ? 312 : 236);
            GUI.Box(rect, "Elementborn Prototype HUD");
            GUI.Label(new Rect(32, 42, 700, 24), GetObjectiveText());
            GUI.Label(new Rect(32, 68, 700, 24), "Attunement: " + ElementbornPrototypeVisualUtility.GetElementName(selectedElement) + " | Stance: " + pathChoice + " | Quest: " + questState);
            GUI.Label(new Rect(32, 94, 700, 24), "Essence: " + elementalEssence + "/" + essenceNeededForReward + " | Coins: " + coins + " | Lore: " + loreDiscovered + " | Chests: " + chestsOpened + " | Zone Level: " + zoneLevel + " | Relic: " + (hasPrototypeRelic ? "Equipped" : "None"));

            if (stats != null)
            {
                GUI.Label(new Rect(32, 124, 280, 22), "Health: " + Mathf.CeilToInt(stats.currentHealth) + "/" + Mathf.CeilToInt(stats.maxHealth));
                GUI.HorizontalScrollbar(new Rect(126, 126, 190, 20), 0f, stats.Health01, 0f, 1f);

                GUI.Label(new Rect(350, 124, 280, 22), "Stamina: " + Mathf.CeilToInt(stats.currentStamina) + "/" + Mathf.CeilToInt(stats.maxStamina));
                GUI.HorizontalScrollbar(new Rect(460, 126, 190, 20), 0f, stats.Stamina01, 0f, 1f);
            }

            if (ability != null)
            {
                GUI.Label(new Rect(32, 152, 680, 22), "Ability: Q elemental bolt | Cooldown: " + ability.CooldownRemaining.ToString("0.0") + "s");
                GUI.HorizontalScrollbar(new Rect(194, 154, 190, 20), 0f, 1f - ability.Cooldown01, 0f, 1f);
            }

            GUI.Label(new Rect(32, 180, 690, 22), "Effects: Fire burns | Water slows | Earth stuns/knocks | Air pushes");

            if (Time.time < messageUntil && !string.IsNullOrWhiteSpace(message))
            {
                GUI.Box(new Rect(28, 206, 700, 54), "Latest Message");
                GUI.Label(new Rect(42, 230, 670, 26), message);
            }

            if (showDebugHelp)
            {
                GUI.Label(new Rect(32, 268, 690, 20), "J = journal | M = message log | C = compass | Enter = dismiss dialogue | F1 = toggle help");
                GUI.Label(new Rect(32, 288, 690, 20), "Move: WASD / Arrows | Sprint: Shift | Jump: Space | Interact: E | Cast: Q | Elements: 1-4");
            }
        }

        private void DrawDialogue()
        {
            if (Time.time >= dialogueUntil || string.IsNullOrWhiteSpace(dialogueText))
            {
                return;
            }

            const int width = 920;
            const int height = 164;
            Rect rect = new Rect((Screen.width - width) / 2f, Screen.height - height - 34f, width, height);
            GUI.Box(rect, dialogueSpeaker);
            GUI.Label(new Rect(rect.x + 28, rect.y + 38, width - 56, 92), dialogueText);
            GUI.Label(new Rect(rect.x + 28, rect.y + 132, width - 56, 22), "Enter = dismiss | J = journal | M = message log");
        }

        private void DrawGuideChoice()
        {
            if (!guideChoiceOpen)
            {
                return;
            }

            const int width = 540;
            const int height = 128;
            Rect rect = new Rect((Screen.width - width) / 2f, Screen.height - 320f, width, height);
            GUI.Box(rect, "Choose your first stance");

            if (GUI.Button(new Rect(rect.x + 42, rect.y + 54, 210, 44), "Unifier"))
            {
                ChoosePath(ElementbornPrototypePathChoice.Unifier);
            }

            if (GUI.Button(new Rect(rect.x + 288, rect.y + 54, 210, 44), "Dominion"))
            {
                ChoosePath(ElementbornPrototypePathChoice.Dominion);
            }
        }


        private void DrawCompass()
        {
            if (!showCompass || player == null)
            {
                return;
            }

            ElementbornPrototypeInteractable target = FindCurrentObjectiveTarget();
            Rect rect = new Rect((Screen.width - 430f) / 2f, 16f, 430f, 82f);
            GUI.Box(rect, "Objective Compass");

            if (target == null)
            {
                GUI.Label(new Rect(rect.x + 18, rect.y + 34, rect.width - 36, 22), "Explore the hub or open the journal with J.");
                return;
            }

            Vector3 offset = target.transform.position - player.transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            string direction = GetDirectionText(offset);
            GUI.Label(new Rect(rect.x + 18, rect.y + 34, rect.width - 36, 22), target.displayName + " — " + direction + " — " + distance.ToString("0.0") + "m");
            GUI.Label(new Rect(rect.x + 18, rect.y + 56, rect.width - 36, 20), "Marker: " + target.GetPrompt());
        }

        private ElementbornPrototypeInteractable FindCurrentObjectiveTarget()
        {
            ElementbornPrototypeInteractableKind desiredKind = ElementbornPrototypeInteractableKind.Generic;

            switch (questState)
            {
                case ElementbornPrototypeQuestState.NotStarted:
                    desiredKind = ElementbornPrototypeInteractableKind.GuideNpc;
                    break;
                case ElementbornPrototypeQuestState.TalkedToGuide:
                    desiredKind = ElementbornPrototypeInteractableKind.ShardResource;
                    break;
                case ElementbornPrototypeQuestState.CollectedShard:
                    desiredKind = ElementbornPrototypeInteractableKind.ReturnPoint;
                    break;
                case ElementbornPrototypeQuestState.Completed:
                    return FindMatchingGate();
                case ElementbornPrototypeQuestState.OpenedElementGate:
                    return FindNearestNamedInteractable("Training Hostile");
                case ElementbornPrototypeQuestState.DefeatedHostile:
                    desiredKind = ElementbornPrototypeInteractableKind.ResourceNode;
                    break;
                case ElementbornPrototypeQuestState.GatheredEssence:
                    desiredKind = ElementbornPrototypeInteractableKind.LootChest;
                    break;
                case ElementbornPrototypeQuestState.OpenedRewardChest:
                    desiredKind = ElementbornPrototypeInteractableKind.LoreStone;
                    break;
                default:
                    return null;
            }

            return FindNearestInteractableOfKind(desiredKind);
        }

        private ElementbornPrototypeInteractable FindMatchingGate()
        {
            ElementbornPrototypeInteractable[] interactables =
                FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable != null &&
                    interactable.kind == ElementbornPrototypeInteractableKind.ElementGate &&
                    interactable.gateElement == selectedElement)
                {
                    return interactable;
                }
            }

            return FindNearestInteractableOfKind(ElementbornPrototypeInteractableKind.ElementGate);
        }

        private ElementbornPrototypeInteractable FindNearestNamedInteractable(string containsName)
        {
            ElementbornPrototypeInteractable best = null;
            float bestDistance = float.MaxValue;

            ElementbornPrototypeInteractable[] interactables =
                FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null || !interactable.name.Contains(containsName))
                {
                    continue;
                }

                float distance = player != null ? Vector3.Distance(player.transform.position, interactable.transform.position) : 0f;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
                }
            }

            return best;
        }

        private ElementbornPrototypeInteractable FindNearestInteractableOfKind(ElementbornPrototypeInteractableKind kind)
        {
            ElementbornPrototypeInteractable best = null;
            float bestDistance = float.MaxValue;

            ElementbornPrototypeInteractable[] interactables =
                FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null || interactable.kind != kind || interactable.consumed)
                {
                    continue;
                }

                float distance = player != null ? Vector3.Distance(player.transform.position, interactable.transform.position) : 0f;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
                }
            }

            return best;
        }

        private string GetDirectionText(Vector3 offset)
        {
            if (offset.sqrMagnitude < 0.01f)
            {
                return "here";
            }

            Vector3 forward = playCamera != null ? playCamera.transform.forward : Vector3.forward;
            Vector3 right = playCamera != null ? playCamera.transform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 dir = offset.normalized;
            float f = Vector3.Dot(forward, dir);
            float r = Vector3.Dot(right, dir);

            if (f > 0.7f) return "ahead";
            if (f < -0.7f) return "behind";
            if (r > 0.7f) return "right";
            if (r < -0.7f) return "left";
            if (f >= 0f && r >= 0f) return "ahead-right";
            if (f >= 0f && r < 0f) return "ahead-left";
            if (f < 0f && r >= 0f) return "behind-right";
            return "behind-left";
        }

        private void DrawMessageLog()
        {
            if (!showMessageLog)
            {
                return;
            }

            int lines = Mathf.Clamp(messageLogLines, 1, messageHistory.Length);
            Rect rect = new Rect(Screen.width - 430f, 16f, 410f, 42f + lines * 24f);
            GUI.Box(rect, "Recent Messages");

            for (int i = 0; i < lines; i++)
            {
                if (!string.IsNullOrWhiteSpace(messageHistory[i]))
                {
                    GUI.Label(new Rect(rect.x + 16, rect.y + 34 + i * 24, rect.width - 32, 22), messageHistory[i]);
                }
            }
        }

        private void DrawJournal()
        {
            if (!showJournal)
            {
                return;
            }

            const int width = 520;
            const int height = 520;
            Rect rect = new Rect(Screen.width - width - 24f, (Screen.height - height) / 2f, width, height);
            GUI.Box(rect, "Journal");
            GUI.Label(new Rect(rect.x + 24, rect.y + 36, width - 48, height - 62), GetJournalText());
        }
    }
}
