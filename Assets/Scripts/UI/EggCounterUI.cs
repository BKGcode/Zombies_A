using UnityEngine;
using TMPro;

namespace GallinasFelices.UI
{
    public class EggCounterUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI eggCountText;

        [Header("Settings")]
        [SerializeField] private string prefix = "Eggs: ";

        private void Start()
        {
            if (Core.EggCounter.Instance != null)
            {
                Core.EggCounter.Instance.OnEggCountChanged += UpdateDisplay;
                UpdateDisplay(Core.EggCounter.Instance.TotalEggs);
            }
        }

        private void UpdateDisplay(int eggCount)
        {
            if (eggCountText != null)
            {
                eggCountText.text = $"{prefix}{eggCount}";
            }
        }

        private void OnDestroy()
        {
            if (Core.EggCounter.Instance != null)
            {
                Core.EggCounter.Instance.OnEggCountChanged -= UpdateDisplay;
            }
        }
    }
}
