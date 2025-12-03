using UnityEngine;
using GallinasFelices.Chicken;

namespace GallinasFelices.Structures
{
    public class WaterTrough : ConsumableStructure
    {
        [Header("Water Trough Settings")]
        [SerializeField] private Transform drinkingSpot;
        [SerializeField] private Data.WaterTroughConfigSO currentConfig;
        [SerializeField] private MeshRenderer meshRenderer;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Chicken.Chicken>(out var chicken))
            {
                chicken.GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Drinking", $"Entered water trough trigger. State:{chicken.CurrentState} Capacity:{CurrentCapacity:F0}", HappyChickens.Debug.EventSeverity.Info);
                
                if (chicken.CurrentState == ChickenState.Drinking && !IsEmpty)
                {
                    if (TryStartUsing())
                    {
                        TryConsume(0); // Amount is handled by GameBalanceSO in base class
                        chicken.OnStateChanged.AddListener((newState) => OnChickenStateChanged(chicken, newState));
                    }
                    else
                    {
                        // Structure is full, chicken should abort drinking
                        chicken.ChangeState(ChickenState.Idle);
                    }
                }
            }
        }

        private void OnChickenStateChanged(Chicken.Chicken chicken, ChickenState newState)
        {
            if (newState != ChickenState.Drinking)
            {
                StopUsing();
                chicken.OnStateChanged.RemoveListener((state) => OnChickenStateChanged(chicken, state));
            }
        }

        public Vector3 GetDrinkingPosition()
        {
            if (drinkingSpot != null)
            {
                return drinkingSpot.position;
            }

            return transform.position;
        }

        public bool IsInDrinkingRange(Vector3 position)
        {
            float range = gameBalance != null ? gameBalance.drinkingRange : 2f;
            return Vector3.Distance(GetDrinkingPosition(), position) <= range;
        }

        private void OnDrawGizmosSelected()
        {
            if (gameBalance != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(GetDrinkingPosition(), gameBalance.drinkingRange);
            }
        }

        public override float MaxCapacity
        {
            get
            {
                if (currentConfig != null) return currentConfig.capacity;
                return 100f; // Default fallback
            }
        }

        protected override string GetStructureTitle()
        {
            return uiTexts != null ? uiTexts.waterTroughTitle : "Bebedero";
        }

        protected override int GetRefillCost()
        {
            return gameBalance != null ? gameBalance.waterTroughRefillCost : 10;
        }

        protected override int GetRepairCost()
        {
            return gameBalance != null ? gameBalance.waterTroughRepairCost : 20;
        }

        protected override int GetMaxSimultaneousUsers()
        {
            if (currentConfig != null) return currentConfig.simultaneousUsers;
            return 5; // Default fallback
        }

        public bool CanUpgrade()
        {
            return currentConfig != null && currentConfig.CanUpgrade;
        }

        public int GetUpgradeCost()
        {
            return currentConfig != null ? currentConfig.upgradeCost : 0;
        }

        public void Upgrade()
        {
            if (currentConfig != null && currentConfig.nextLevel != null)
            {
                currentConfig = currentConfig.nextLevel;
                UpdateVisual();
                Debug.Log($"[WaterTrough] Upgraded to Level {currentConfig.level}: {currentConfig.levelName}");
            }
        }

        private void UpdateVisual()
        {
            if (currentConfig != null && currentConfig.material != null && meshRenderer != null)
            {
                meshRenderer.material = currentConfig.material;
            }
        }

        protected override void AddUpgradeAction(System.Collections.Generic.List<Core.InteractionButton> actions)
        {
            if (!CanUpgrade()) return;

            int cost = GetUpgradeCost();
            bool canAfford = Core.EggCounter.Instance != null && Core.EggCounter.Instance.CanAfford(cost);

            actions.Add(new Core.InteractionButton(
                $"Mejorar ({cost} huevos)",
                canAfford,
                Upgrade
            ));
        }
    }
}
