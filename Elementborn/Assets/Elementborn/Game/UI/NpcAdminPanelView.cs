using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class NpcAdminPanelView : MonoBehaviour
    {
        [SerializeField] private InputField searchInput;
        [SerializeField] private InputField regionInput;
        [SerializeField] private InputField elementInput;
        [SerializeField] private Text resultsText;
        [SerializeField] private bool refreshOnEnable = true;

        private void OnEnable()
        {
            if (refreshOnEnable)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            var filter = new NpcAdminFilter
            {
                SearchText = searchInput != null ? searchInput.text : "",
                Region = regionInput != null ? regionInput.text : "",
                Element = elementInput != null ? elementInput.text : "",
                IncludeUnknownRole = true
            };

            string summary = NpcAdminRegistry.Ensure().BuildSummary(filter);
            if (resultsText != null)
            {
                resultsText.text = summary;
            }
            else
            {
                Debug.Log(summary);
            }
        }

        public void ClearSearch()
        {
            if (searchInput != null) searchInput.text = "";
            if (regionInput != null) regionInput.text = "";
            if (elementInput != null) elementInput.text = "";
            Refresh();
        }

        public void ShowRoyalFamily()
        {
            ShowRole(NpcWorldRole.RoyalFamily);
        }

        public void ShowVillains()
        {
            ShowRole(NpcWorldRole.Villain);
        }

        public void ShowPirates()
        {
            ShowRole(NpcWorldRole.Pirate);
        }

        private void ShowRole(NpcWorldRole role)
        {
            var filter = new NpcAdminFilter { Role = role, IncludeUnknownRole = true };
            string summary = NpcAdminRegistry.Ensure().BuildSummary(filter);
            if (resultsText != null)
            {
                resultsText.text = summary;
            }
            else
            {
                Debug.Log(summary);
            }
        }
    }
}
