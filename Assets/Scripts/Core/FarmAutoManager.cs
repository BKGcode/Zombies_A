using UnityEngine;
using GallinasFelices.Data;
using GallinasFelices.Structures;

namespace GallinasFelices.Core
{
    public class FarmAutoManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private float checkInterval = 5f;
        [SerializeField] private bool enableAutoRepair = true;
        [SerializeField] private bool enableAutoRefill = true;

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= checkInterval)
            {
                timer = 0;
                PerformAutoManagement();
            }
        }

        private void PerformAutoManagement()
        {
            if (gameBalance == null || EggCounter.Instance == null) return;

            if (enableAutoRepair)
            {
                CheckAndRepairStructures();
            }

            if (enableAutoRefill)
            {
                CheckAndRefillStructures();
            }
        }

        private void CheckAndRepairStructures()
        {
            StructureDurability[] structures = FindObjectsOfType<StructureDurability>();

            foreach (var structure in structures)
            {
                if (structure.CurrentDurability < gameBalance.autoRepairThreshold)
                {
                    TryRepair(structure);
                }
            }
        }

        private void TryRepair(StructureDurability structure)
        {
            int cost = structure.GetRepairCost();
            
            if (cost > 0 && EggCounter.Instance.CanAfford(cost))
            {
                EggCounter.Instance.TrySpendEggs(cost);
                structure.Repair();
                Debug.Log($"[FarmAutoManager] Auto-repaired {structure.name}");
            }
        }

        private void CheckAndRefillStructures()
        {
            ConsumableStructure[] consumables = FindObjectsOfType<ConsumableStructure>();

            foreach (var consumable in consumables)
            {
                // FillPercentage is 0-1, threshold is likely 0-100 or 0-1. 
                // GameBalanceSO tooltip says "Percentage decay...", usually 0-100 in this project context?
                // Let's check GameBalanceSO.cs again. 
                // "public float autoRefillThreshold = 20f;" -> This implies 0-100.
                // FillPercentage is 0-1. So we need to multiply by 100.
                if (consumable.FillPercentage * 100f < gameBalance.autoRefillThreshold)
                {
                    TryRefill(consumable);
                }
            }
        }

        private void TryRefill(ConsumableStructure consumable)
        {
            int cost = 0;
            if (consumable is Feeder) cost = gameBalance.feederRefillCost;
            else if (consumable is WaterTrough) cost = gameBalance.waterTroughRefillCost;
            
            if (cost > 0 && EggCounter.Instance.CanAfford(cost))
            {
                EggCounter.Instance.TrySpendEggs(cost);
                consumable.RefillToMax();
                Debug.Log($"[FarmAutoManager] Auto-refilled {consumable.name}");
            }
        }
    }
}
