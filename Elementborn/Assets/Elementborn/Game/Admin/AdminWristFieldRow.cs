using System;
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminWristFieldRow
    {
        public GameObject Root;
        public Text Label;
        public InputField Input;
        public Dropdown Dropdown;
        public Toggle Toggle;

        public void Hide()
        {
            if (Root != null)
            {
                Root.SetActive(false);
            }
        }

        public void Show(AdminActionFieldDefinition field)
        {
            if (Root != null)
            {
                Root.SetActive(true);
            }

            if (Label != null)
            {
                Label.text = field != null ? field.Label : "Field";
            }

            bool useDropdown = field != null && (
                field.FieldType == AdminFieldType.Dropdown ||
                field.FieldType == AdminFieldType.Capital ||
                field.FieldType == AdminFieldType.CapitalPressure);

            bool useToggle = field != null && field.FieldType == AdminFieldType.Toggle;
            bool useInput = field != null && !useDropdown && !useToggle;

            if (Input != null)
            {
                Input.gameObject.SetActive(useInput);
                Input.text = field != null ? field.DefaultValue : "";
            }

            if (Dropdown != null)
            {
                Dropdown.gameObject.SetActive(useDropdown);
                Dropdown.ClearOptions();

                var options = new System.Collections.Generic.List<string>();
                if (field != null)
                {
                    if (field.FieldType == AdminFieldType.Capital)
                    {
                        options.AddRange(System.Enum.GetNames(typeof(CapitalId)));
                    }
                    else if (field.FieldType == AdminFieldType.CapitalPressure)
                    {
                        options.AddRange(System.Enum.GetNames(typeof(CapitalPressureType)));
                    }
                    else if (field.Options != null && field.Options.Count > 0)
                    {
                        options.AddRange(field.Options);
                    }
                }

                if (options.Count == 0)
                {
                    options.Add(field != null ? field.DefaultValue : "");
                }

                Dropdown.AddOptions(options);
                int defaultIndex = Mathf.Max(0, options.IndexOf(field != null ? field.DefaultValue : ""));
                Dropdown.value = defaultIndex >= 0 ? defaultIndex : 0;
                Dropdown.RefreshShownValue();
            }

            if (Toggle != null)
            {
                Toggle.gameObject.SetActive(useToggle);
                Toggle.isOn = field != null && (field.DefaultValue == "true" || field.DefaultValue == "1");
            }
        }

        public string GetValue(AdminActionFieldDefinition field)
        {
            if (field == null)
            {
                return "";
            }

            if ((field.FieldType == AdminFieldType.Dropdown ||
                 field.FieldType == AdminFieldType.Capital ||
                 field.FieldType == AdminFieldType.CapitalPressure) &&
                Dropdown != null &&
                Dropdown.options != null &&
                Dropdown.options.Count > Dropdown.value)
            {
                return Dropdown.options[Dropdown.value].text;
            }

            if (field.FieldType == AdminFieldType.Toggle && Toggle != null)
            {
                return Toggle.isOn ? "true" : "false";
            }

            return Input != null ? Input.text : "";
        }
    }
}
