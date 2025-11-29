using UnityEngine;
using GallinasFelices.Data;
using GallinasFelices.Structures;

namespace GallinasFelices.Core
{
    public class FarmLimits : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private FarmLimitsSO limitsConfig;
        [SerializeField] private FarmManager farmManager;
        [SerializeField] private UITextsConfigSO uiTexts;

        public static FarmLimits Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            if (limitsConfig == null)
            {
                Debug.LogWarning("[FarmLimits] FarmLimitsSO not assigned!");
            }
        }

        public bool CanBuyChicken(out string reason)
        {
            reason = string.Empty;

            if (limitsConfig == null) return true;

            int currentChickens = GetCurrentChickens();
            int maxChickens = GetMaxChickens();

            if (currentChickens >= maxChickens)
            {
                if (uiTexts != null)
                {
                    reason = $"{uiTexts.needMoreNests} ({currentChickens}/{maxChickens})";
                }
                return false;
            }

            return true;
        }

        public int GetMaxChickens()
        {
            if (limitsConfig == null) return 0;
            
            Nest[] nests = FindObjectsOfType<Nest>();
            return nests.Length * limitsConfig.chickensPerNest;
        }

        public int GetCurrentChickens()
        {
            if (farmManager != null)
            {
                return farmManager.CurrentChickenCount;
            }
            
            return FindObjectsOfType<Chicken.Chicken>().Length;
        }
    }
}
