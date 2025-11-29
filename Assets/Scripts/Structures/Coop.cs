using System.Collections.Generic;
using UnityEngine;
using GallinasFelices.Core;
using GallinasFelices.Data;

namespace GallinasFelices.Structures
{
    public class Coop : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private UITextsConfigSO uiTexts;
        [SerializeField] private List<Transform> sleepingSpots = new List<Transform>();
        [SerializeField] private CoopConfigSO currentConfig;
        [SerializeField] private MeshRenderer meshRenderer;

        private Dictionary<Chicken.Chicken, Transform> assignedSpots = new Dictionary<Chicken.Chicken, Transform>();

        private void Start()
        {
            if (gameBalance == null)
            {
                Debug.LogWarning("[Coop] GameBalanceSO not assigned!");
            }
        }

        public bool HasAvailableSpot()
        {
            int maxCapacity = GetMaxCapacity();
            return assignedSpots.Count < maxCapacity && assignedSpots.Count < sleepingSpots.Count;
        }

        private int GetMaxCapacity()
        {
            if (currentConfig != null) return currentConfig.sleepingSpots;
            return 15; // Default fallback
        }

        public Transform AssignSpot(Chicken.Chicken chicken)
        {
            if (assignedSpots.ContainsKey(chicken))
            {
                return assignedSpots[chicken];
            }

            if (!HasAvailableSpot())
            {
                return null;
            }

            foreach (var spot in sleepingSpots)
            {
                if (!assignedSpots.ContainsValue(spot))
                {
                    assignedSpots[chicken] = spot;
                    return spot;
                }
            }

            return null;
        }

        public void ReleaseSpot(Chicken.Chicken chicken)
        {
            if (assignedSpots.ContainsKey(chicken))
            {
                assignedSpots.Remove(chicken);
            }
        }

        public bool IsInCoopRange(Vector3 position)
        {
            if (gameBalance == null) return false;
            return Vector3.Distance(transform.position, position) <= gameBalance.coopRange;
        }

        private void OnDrawGizmosSelected()
        {
            if (gameBalance != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, gameBalance.coopRange);
            }

            if (sleepingSpots != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var spot in sleepingSpots)
                {
                    if (spot != null)
                    {
                        Gizmos.DrawWireSphere(spot.position, 0.3f);
                    }
                }
            }
        }
        public string GetTitle()
        {
            return uiTexts != null ? uiTexts.coopTitle : string.Empty;
        }

        public string GetMainInfo()
        {
            if (uiTexts == null) return string.Empty;
            StructureDurability durability = GetComponent<StructureDurability>();
            string status = durability != null && durability.IsBroken ? uiTexts.brokenState : uiTexts.operativeState;
            return status;
        }

        public InteractionBar[] GetBars()
        {
            StructureDurability durability = GetComponent<StructureDurability>();
            float durabilityVal = durability != null ? durability.CurrentDurability : 100f;
            
            int occupied = assignedSpots.Count;
            int total = GetMaxCapacity();

            string occupancyLabel = uiTexts != null ? uiTexts.occupancyLabel : string.Empty;
            string durabilityLabel = uiTexts != null ? uiTexts.durabilityLabel : string.Empty;

            return new InteractionBar[]
            {
                new InteractionBar(occupancyLabel, occupied, total),
                new InteractionBar(durabilityLabel, durabilityVal, 100f)
            };
        }

        public InteractionButton[] GetActions()
        {
            System.Collections.Generic.List<InteractionButton> actions = new System.Collections.Generic.List<InteractionButton>();

            StructureDurability durability = GetComponent<StructureDurability>();
            if (durability != null && durability.CurrentDurability < 100f)
            {
                int repairCost = gameBalance != null ? gameBalance.coopRepairCost : 50;
                bool canRepair = EggCounter.Instance != null && EggCounter.Instance.CanAfford(repairCost);

                actions.Add(new InteractionButton(
                    $"Reparar ({repairCost} huevos)",
                    canRepair,
                    () => {
                        if (EggCounter.Instance != null && EggCounter.Instance.TrySpendEggs(repairCost))
                        {
                            durability.Repair();
                        }
                    }
                ));
            }

            // Upgrade button
            if (CanUpgrade())
            {
                int upgradeCost = GetUpgradeCost();
                bool canUpgrade = EggCounter.Instance != null && EggCounter.Instance.CanAfford(upgradeCost);

                actions.Add(new InteractionButton(
                    $"Mejorar ({upgradeCost} huevos)",
                    canUpgrade,
                    () => {
                        if (EggCounter.Instance != null && EggCounter.Instance.TrySpendEggs(upgradeCost))
                        {
                            Upgrade();
                        }
                    }
                ));
            }

            return actions.ToArray();
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
                Debug.Log($"[Coop] Upgraded to Level {currentConfig.level}: {currentConfig.levelName}");
            }
        }

        private void UpdateVisual()
        {
            if (currentConfig != null && currentConfig.material != null && meshRenderer != null)
            {
                meshRenderer.material = currentConfig.material;
            }
        }
    }
}
