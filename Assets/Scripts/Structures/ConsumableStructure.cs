using UnityEngine;
using UnityEngine.Events;
using GallinasFelices.Core;
using GallinasFelices.Data;

namespace GallinasFelices.Structures
{
    public abstract class ConsumableStructure : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] protected GameBalanceSO gameBalance;
        [SerializeField] protected UITextsConfigSO uiTexts;

        [Header("State")]
        [SerializeField] protected float currentCapacity = 100f;
        [SerializeField] protected int currentUsers = 0;

        [Header("Events")]
        public UnityEvent<float> OnCapacityChanged;
        public UnityEvent OnEmpty;
        public UnityEvent OnRefilled;

        public float CurrentCapacity => currentCapacity;
        public int CurrentUsers => currentUsers;
        
        public abstract float MaxCapacity { get; }
        protected abstract int GetMaxSimultaneousUsers();

        public bool IsEmpty => currentCapacity <= 0f;
        public bool IsFull => currentUsers >= GetMaxSimultaneousUsers();
        public float FillPercentage => MaxCapacity > 0 ? currentCapacity / MaxCapacity : 0f;

        private StructureDurability durability;

        protected virtual void Awake()
        {
            durability = GetComponent<StructureDurability>();
        }

        protected virtual void Start()
        {
            if (gameBalance == null)
            {
                Debug.LogWarning($"[{gameObject.name}] GameBalanceSO not assigned!");
                // Try to find it
                gameBalance = FindObjectOfType<GameBalanceSO>();
            }

            OnCapacityChanged?.Invoke(FillPercentage);
        }

        public virtual bool TryConsume(float amount)
        {
            if (IsEmpty)
            {
                return false;
            }

            if (durability != null && durability.IsBroken)
            {
                return false;
            }

            // Use SO consumption amount if available, otherwise fallback to parameter
            float actualConsumption = gameBalance != null ? gameBalance.consumptionPerUse : amount;

            currentCapacity = Mathf.Max(0f, currentCapacity - actualConsumption);
            OnCapacityChanged?.Invoke(FillPercentage);

            durability?.OnStructureUsed();

            if (IsEmpty)
            {
                OnEmpty?.Invoke();
            }

            return true;
        }

        public virtual void Refill(float amount)
        {
            bool wasEmpty = IsEmpty;
            currentCapacity = Mathf.Min(MaxCapacity, currentCapacity + amount);
            OnCapacityChanged?.Invoke(FillPercentage);

            if (wasEmpty && !IsEmpty)
            {
                OnRefilled?.Invoke();
            }
        }

        public virtual void RefillToMax()
        {
            Refill(MaxCapacity);
        }

        public virtual void Upgrade(float capacityIncrease)
        {
            // Deprecated/Needs refactor for SO based capacity
            // For now, we just invoke change
            OnCapacityChanged?.Invoke(FillPercentage);
        }

        public virtual bool TryStartUsing()
        {
            if (IsFull)
            {
                return false;
            }

            if (durability != null && durability.IsBroken)
            {
                return false;
            }

            if (IsEmpty)
            {
                return false;
            }

            currentUsers++;
            return true;
        }

        public virtual void StopUsing()
        {
            if (currentUsers > 0)
            {
                currentUsers--;
            }
        }

        public virtual string GetTitle()
        {
            return uiTexts != null ? GetStructureTitle() : string.Empty;
        }

        protected abstract string GetStructureTitle();

        public virtual string GetMainInfo()
        {
            if (uiTexts == null) return string.Empty;
            StructureDurability durabilityComp = GetComponent<StructureDurability>();
            string status = durabilityComp != null && durabilityComp.IsBroken ? uiTexts.brokenState : uiTexts.operativeState;

            return status;
        }

        public virtual Core.InteractionBar[] GetBars()
        {
            StructureDurability durabilityComp = GetComponent<StructureDurability>();
            float durability = durabilityComp != null ? durabilityComp.CurrentDurability : 100f;

            string capacityLabel = uiTexts != null ? uiTexts.capacityLabel : string.Empty;
            string durabilityLabel = uiTexts != null ? uiTexts.durabilityLabel : string.Empty;

            return new Core.InteractionBar[]
            {
                new Core.InteractionBar(capacityLabel, currentCapacity, MaxCapacity),
                new Core.InteractionBar(durabilityLabel, durability, 100f)
            };
        }

        public virtual InteractionButton[] GetActions()
        {
            System.Collections.Generic.List<InteractionButton> actions = new System.Collections.Generic.List<InteractionButton>();

            if (gameBalance == null) return actions.ToArray();

            int refillCost = GetRefillCost();

            bool canRefill = currentCapacity < MaxCapacity && 
                           EggCounter.Instance != null && 
                           EggCounter.Instance.CanAfford(refillCost);

            actions.Add(new InteractionButton(
                $"Rellenar ({refillCost} huevos)",
                canRefill,
                () => {
                    if (EggCounter.Instance != null && EggCounter.Instance.TrySpendEggs(refillCost))
                    {
                        RefillToMax();
                    }
                }
            ));

            StructureDurability durabilityComp = GetComponent<StructureDurability>();
            if (durabilityComp != null)
            {
                int repairCost = GetRepairCost();

                bool canRepair = durabilityComp.CurrentDurability < 100f && 
                               EggCounter.Instance != null && 
                               EggCounter.Instance.CanAfford(repairCost);

                actions.Add(new InteractionButton(
                    $"Reparar ({repairCost} huevos)",
                    canRepair,
                    () => {
                        if (EggCounter.Instance != null && EggCounter.Instance.TrySpendEggs(repairCost))
                        {
                            durabilityComp.Repair();
                        }
                    }
                ));
            }

            AddUpgradeAction(actions);

            return actions.ToArray();
        }

        protected virtual void AddUpgradeAction(System.Collections.Generic.List<InteractionButton> actions)
        {
        }

        protected abstract int GetRefillCost();
        protected abstract int GetRepairCost();
    }
}
