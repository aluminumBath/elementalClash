using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class AdminWristPanelView : MonoBehaviour
    {
        [Header("Dropdown form")]
        [SerializeField] private Dropdown categoryDropdown;
        [SerializeField] private Dropdown actionDropdown;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button applyButton;
        [SerializeField] private List<AdminWristFieldRow> fieldRows = new List<AdminWristFieldRow>();

        [Header("Cheat / raw command section")]
        [SerializeField] private Dropdown cheatDropdown;
        [SerializeField] private Button applyCheatButton;
        [SerializeField] private InputField rawCommandInput;
        [SerializeField] private Button runRawCommandButton;

        private readonly List<AdminActionCategory> categories = new List<AdminActionCategory>();
        private readonly List<AdminActionDefinition> visibleActions = new List<AdminActionDefinition>();
        private AdminActionDefinition currentAction;

        private void Awake()
        {
            Wire();
        }

        private void OnEnable()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            AdminActionCatalog.Ensure().RebuildDefaults();
            RebuildCategories();
            RebuildCheats();
            RefreshActions();
        }

        public void ApplyCurrentAction()
        {
            if (currentAction == null)
            {
                SetStatus("No action selected.", false);
                return;
            }

            AdminActionRequest request = new AdminActionRequest { ActionId = currentAction.ActionId };
            for (int i = 0; i < currentAction.Fields.Count && i < fieldRows.Count; i++)
            {
                AdminActionFieldDefinition field = currentAction.Fields[i];
                request.Values[field.FieldId] = fieldRows[i].GetValue(field);
            }

            AdminActionResult result = AdminActionExecutor.Ensure().Execute(request);
            SetStatus(result.Message, result.Success);
            RefreshDashboard();
        }

        public void ApplySelectedCheat()
        {
            string cheat = "";
            if (cheatDropdown != null && cheatDropdown.options.Count > cheatDropdown.value)
            {
                cheat = cheatDropdown.options[cheatDropdown.value].text;
            }

            AdminActionRequest request = new AdminActionRequest { ActionId = "cheat.apply" };
            request.Values["cheat"] = cheat;
            AdminActionResult result = AdminActionExecutor.Ensure().Execute(request);
            SetStatus(result.Message, result.Success);
            RefreshDashboard();
        }

        public void RunRawCommand()
        {
            string command = rawCommandInput != null ? rawCommandInput.text : "";
            AdminActionRequest request = new AdminActionRequest { ActionId = "raw.command" };
            request.Values["command"] = command;
            AdminActionResult result = AdminActionExecutor.Ensure().Execute(request);
            SetStatus(result.Message, result.Success);
            RefreshDashboard();
        }

        private void Wire()
        {
            if (categoryDropdown != null)
            {
                categoryDropdown.onValueChanged.RemoveListener(OnCategoryChanged);
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            }

            if (actionDropdown != null)
            {
                actionDropdown.onValueChanged.RemoveListener(OnActionChanged);
                actionDropdown.onValueChanged.AddListener(OnActionChanged);
            }

            if (applyButton != null)
            {
                applyButton.onClick.RemoveListener(ApplyCurrentAction);
                applyButton.onClick.AddListener(ApplyCurrentAction);
            }

            if (applyCheatButton != null)
            {
                applyCheatButton.onClick.RemoveListener(ApplySelectedCheat);
                applyCheatButton.onClick.AddListener(ApplySelectedCheat);
            }

            if (runRawCommandButton != null)
            {
                runRawCommandButton.onClick.RemoveListener(RunRawCommand);
                runRawCommandButton.onClick.AddListener(RunRawCommand);
            }
        }

        private void RebuildCategories()
        {
            categories.Clear();
            if (categoryDropdown == null)
            {
                return;
            }

            categoryDropdown.ClearOptions();
            var labels = new List<string>();
            foreach (AdminActionCategory category in System.Enum.GetValues(typeof(AdminActionCategory)))
            {
                categories.Add(category);
                labels.Add(category.ToString());
            }

            categoryDropdown.AddOptions(labels);
            categoryDropdown.value = 0;
            categoryDropdown.RefreshShownValue();
        }

        private void RebuildCheats()
        {
            if (cheatDropdown == null)
            {
                return;
            }

            cheatDropdown.ClearOptions();
            cheatDropdown.AddOptions(new List<string>
            {
                "stabilize_fire_capital",
                "chaos_fire_capital",
                "start_fire_intro",
                "spawn_wave",
                "save_slot_zero",
                "load_slot_zero",
                "admit_demo_creature",
                "resolve_wolf_pack",
                "pulse_volcano",
                "calm_volcano"
            });
            cheatDropdown.value = 0;
            cheatDropdown.RefreshShownValue();
        }

        private void OnCategoryChanged(int _)
        {
            RefreshActions();
        }

        private void OnActionChanged(int index)
        {
            if (index >= 0 && index < visibleActions.Count)
            {
                SetCurrentAction(visibleActions[index]);
            }
        }

        private void RefreshActions()
        {
            visibleActions.Clear();
            if (actionDropdown == null || categoryDropdown == null)
            {
                return;
            }

            AdminActionCategory category = categories.Count > categoryDropdown.value
                ? categories[categoryDropdown.value]
                : AdminActionCategory.General;

            visibleActions.AddRange(AdminActionCatalog.Ensure().GetActionsForCategory(category));

            actionDropdown.ClearOptions();
            var labels = new List<string>();
            foreach (AdminActionDefinition action in visibleActions)
            {
                labels.Add(action.DisplayName);
            }

            if (labels.Count == 0)
            {
                labels.Add("(No actions)");
            }

            actionDropdown.AddOptions(labels);
            actionDropdown.value = 0;
            actionDropdown.RefreshShownValue();

            SetCurrentAction(visibleActions.Count > 0 ? visibleActions[0] : null);
        }

        private void SetCurrentAction(AdminActionDefinition action)
        {
            currentAction = action;

            if (descriptionText != null)
            {
                descriptionText.text = action != null ? action.Description : "No action available.";
            }

            for (int i = 0; i < fieldRows.Count; i++)
            {
                if (action != null && i < action.Fields.Count)
                {
                    fieldRows[i].Show(action.Fields[i]);
                }
                else
                {
                    fieldRows[i].Hide();
                }
            }
        }

        private void SetStatus(string message, bool success)
        {
            if (statusText != null)
            {
                statusText.text = (success ? "✓ " : "⚠ ") + message;
            }

            Debug.Log((success ? "Admin Wrist UI: " : "Admin Wrist UI warning: ") + message);
        }

        private void RefreshDashboard()
        {
            StorySystemsDebugDashboard dashboard = ElementbornFindUtility.FindFirst<StorySystemsDebugDashboard>();
            if (dashboard != null)
            {
                dashboard.Refresh();
            }
        }
    }
}
