using System;
using UnityEngine;
using GallinasFelices.Data;

namespace GallinasFelices.Structures
{
    public class StructureDurability : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private StructureType structureType;

        [Header("State")]
        [SerializeField] private float currentDurability = 100f;
        [SerializeField] private bool isBroken = false;

        public event Action<float> OnDurabilityChanged;
        public event Action OnStructureBroken;
        public event Action OnStructureRepaired;

        public float CurrentDurability => currentDurability;
        public bool IsBroken => isBroken;

        public enum StructureType
        {
            Feeder,
            WaterTrough,
            Nest,
            Coop
        }

        private void Start()
        {
            if (gameBalance == null)
            {
                Debug.LogError($"GameBalanceSO not assigned in StructureDurability on {gameObject.name}");
            }
        }

        private void Update()
        {
            if (isBroken || gameBalance == null) return;

            // Time decay
            float decayAmount = gameBalance.structureTimeDecayRate * Time.deltaTime / 60f; // Rate is per minute
            DecreaseDurability(decayAmount);
        }

        public void OnStructureUsed()
        {
            if (isBroken || gameBalance == null) return;

            DecreaseDurability(gameBalance.structureUseDecayAmount);
        }

        private void DecreaseDurability(float amount)
        {
            currentDurability -= amount;
            currentDurability = Mathf.Clamp(currentDurability, 0f, 100f);
            
            OnDurabilityChanged?.Invoke(currentDurability);

            if (currentDurability <= 0f && !isBroken)
            {
                BreakStructure();
            }
        }

        private void BreakStructure()
        {
            isBroken = true;
            OnStructureBroken?.Invoke();
            Debug.Log($"{gameObject.name} is broken!");
        }

        public void Repair()
        {
            currentDurability = 100f;
            isBroken = false;
            OnDurabilityChanged?.Invoke(currentDurability);
            OnStructureRepaired?.Invoke();
            Debug.Log($"{gameObject.name} repaired!");
        }

        public int GetRepairCost()
        {
            if (gameBalance == null) return 0;

            switch (structureType)
            {
                case StructureType.Feeder: return gameBalance.feederRepairCost;
                case StructureType.WaterTrough: return gameBalance.waterTroughRepairCost;
                case StructureType.Nest: return gameBalance.nestRepairCost;
                case StructureType.Coop: return gameBalance.coopRepairCost;
                default: return 0;
            }
        }
    }
}
