using UnityEngine;
using TMPro;
using GallinasFelices.Data;

namespace GallinasFelices.UI
{
    public class BuyChickenButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Core.FarmManager farmManager;
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private UnityEngine.UI.Button button;

        private void Start()
        {
            UpdateCostDisplay();
            
            if (button == null)
            {
                button = GetComponent<UnityEngine.UI.Button>();
            }
            
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogError("[BuyChickenButton] Button component not found!");
            }

            if (gameBalance == null)
            {
                Debug.LogWarning("[BuyChickenButton] GameBalanceSO not assigned!");
            }
        }

        public void OnButtonClicked()
        {
            if (farmManager == null)
            {
                Debug.LogError("[BuyChickenButton] FarmManager reference is NULL!");
                return;
            }

            if (gameBalance == null)
            {
                Debug.LogError("[BuyChickenButton] GameBalanceSO reference is NULL!");
                return;
            }

            // Check Limits
            if (Core.FarmLimits.Instance != null)
            {
                if (!Core.FarmLimits.Instance.CanBuyChicken(out string reason))
                {
                    Debug.Log($"[BuyChickenButton] Cannot buy chicken: {reason}");
                    // Optional: Show UI feedback here
                    return;
                }
            }

            int cost = gameBalance.chickenBaseCost;
            
            if (Core.EggCounter.Instance != null && Core.EggCounter.Instance.CanAfford(cost))
            {
                farmManager.BuyChicken(cost);
            }
            else
            {
                Debug.Log("[BuyChickenButton] Not enough eggs!");
            }
        }

        private void UpdateCostDisplay()
        {
            if (costText == null) return;
            
            if (gameBalance != null)
            {
                costText.text = gameBalance.chickenBaseCost.ToString();
            }
            else
            {
                costText.text = string.Empty;
            }
        }
    }
}
