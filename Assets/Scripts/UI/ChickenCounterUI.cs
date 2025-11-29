using UnityEngine;
using TMPro;

namespace GallinasFelices.UI
{
    public class ChickenCounterUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI chickenCountText;

        [Header("Settings")]
        [SerializeField] private string prefix = "Gallinas: ";

        private void Start()
        {
            if (Core.FarmManager.Instance != null)
            {
                Core.FarmManager.Instance.OnChickenCountChanged += UpdateDisplay;
                UpdateDisplay(Core.FarmManager.Instance.CurrentChickenCount, GetMaxChickens());
            }
        }

        private void UpdateDisplay(int currentChickens, int maxChickens)
        {
            if (chickenCountText != null)
            {
                chickenCountText.text = $"{prefix}{currentChickens}/{maxChickens}";
            }
        }

        private int GetMaxChickens()
        {
            if (Core.FarmLimits.Instance != null)
            {
                return Core.FarmLimits.Instance.GetMaxChickens();
            }
            return 0;
        }

        private void OnDestroy()
        {
            if (Core.FarmManager.Instance != null)
            {
                Core.FarmManager.Instance.OnChickenCountChanged -= UpdateDisplay;
            }
        }
    }
}
