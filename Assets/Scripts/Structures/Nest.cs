using UnityEngine;
using UnityEngine.Events;
using GallinasFelices.Core;
using GallinasFelices.Data;
using GallinasFelices.Chicken;

namespace GallinasFelices.Structures
{
    public class Nest : MonoBehaviour, IInteractable
    {
        [Header("Nest Settings")]
        [SerializeField] private bool isOccupied = false;
        [SerializeField] private Transform eggSpawnPoint;
        [SerializeField] private GameObject eggPrefab;
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private UITextsConfigSO uiTexts;

        [Header("Events")]
        public UnityEvent<GameObject> OnEggSpawned;

        public bool IsOccupied => isOccupied;
        public Transform EggSpawnPoint => eggSpawnPoint;

        private GameObject currentEgg;
        private StructureDurability durability;

        private void Awake()
        {
            durability = GetComponent<StructureDurability>();
        }

        private void Start()
        {
            if (gameBalance == null)
            {
                Debug.LogWarning("[Nest] GameBalanceSO not assigned!");
                gameBalance = FindObjectOfType<GameBalanceSO>();
            }

            ChickenEggProducer.RegisterNest(this);
        }

        private void OnDestroy()
        {
            ChickenEggProducer.UnregisterNest(this);
        }

        public bool TryOccupy()
        {
            if (isOccupied)
            {
                return false;
            }

            if (durability != null && durability.IsBroken)
            {
                return false;
            }

            isOccupied = true;
            // Use durability when occupied (chicken enters)
            durability?.OnStructureUsed();
            return true;
        }

        public void Release()
        {
            isOccupied = false;
        }

        public void SpawnEgg()
        {
            if (currentEgg != null)
            {
                return;
            }

            Vector3 spawnPosition = eggSpawnPoint != null ? eggSpawnPoint.position : transform.position;

            if (eggPrefab != null)
            {
                currentEgg = Instantiate(eggPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                currentEgg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                currentEgg.transform.position = spawnPosition;
                currentEgg.transform.localScale = Vector3.one * 0.2f;
                currentEgg.name = "Egg";

                if (currentEgg.TryGetComponent<Collider>(out var collider))
                {
                    collider.isTrigger = true;
                }
            }

            if (currentEgg.TryGetComponent<Egg>(out var egg))
            {
                egg.SetNest(this);
                if (gameBalance != null) egg.GameBalance = gameBalance;
            }
            else
            {
                Egg eggComponent = currentEgg.AddComponent<Egg>();
                eggComponent.SetNest(this);
                if (gameBalance != null) eggComponent.GameBalance = gameBalance;
            }

            OnEggSpawned?.Invoke(currentEgg);
            
            EggCollector collector = FindObjectOfType<EggCollector>();
            if (collector != null && currentEgg != null)
            {
                Egg eggComponent = currentEgg.GetComponent<Egg>();
                if (eggComponent != null)
                {
                    collector.SubscribeToEgg(eggComponent);
                    Debug.Log($"[Nest] Egg spawned and subscribed: {currentEgg.name}");
                }
                else
                {
                    Debug.LogError("[Nest] Egg component not found on spawned egg!");
                }
            }
            else
            {
                if (collector == null)
                    Debug.LogError("[Nest] EggCollector not found in scene!");
            }
        }

        public void RemoveEgg()
        {
            if (currentEgg != null)
            {
                Destroy(currentEgg);
                currentEgg = null;
            }
        }

        public bool HasEgg()
        {
            return currentEgg != null;
        }

        // IInteractable Implementation

        public string GetTitle()
        {
            return uiTexts != null ? uiTexts.nestTitle : string.Empty;
        }

        public string GetMainInfo()
        {
            if (uiTexts == null) return string.Empty;
            string status = durability != null && durability.IsBroken ? uiTexts.brokenState : uiTexts.operativeState;
            return status;
        }

        public InteractionBar[] GetBars()
        {
            float durabilityPercent = durability != null ? durability.CurrentDurability : 100f;
            string durabilityLabel = uiTexts != null ? uiTexts.durabilityLabel : string.Empty;
            
            return new InteractionBar[]
            {
                new InteractionBar(durabilityLabel, durabilityPercent, 100f)
            };
        }

        public InteractionButton[] GetActions()
        {
            System.Collections.Generic.List<InteractionButton> actions = new System.Collections.Generic.List<InteractionButton>();

            if (durability != null && durability.CurrentDurability < 100f)
            {
                int repairCost = gameBalance != null ? gameBalance.nestRepairCost : 15;
                bool canRepair = EggCounter.Instance != null && EggCounter.Instance.CanAfford(repairCost);

                actions.Add(new InteractionButton(
                    $"Reparar ({repairCost} huevos)",
                    canRepair,
                    () => {
                        if (EggCounter.Instance != null && EggCounter.Instance.TrySpendEggs(repairCost))
                        {
                            durability.Repair();
                            Debug.Log($"[Nest] Repaired to 100% durability");
                        }
                    }
                ));
            }

            return actions.ToArray();
        }
    }
}
