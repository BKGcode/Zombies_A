using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GallinasFelices.Core;
using GallinasFelices.Data;

namespace GallinasFelices.Structures
{
    public class Coop : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private UITextsConfigSO uiTexts;
        [SerializeField] private Transform entryPoint;
        [SerializeField] private CoopConfigSO currentConfig;
        [SerializeField] private MeshRenderer meshRenderer;

        private HashSet<Chicken.Chicken> chickensInside = new HashSet<Chicken.Chicken>();

        private void Start()
        {
            if (gameBalance == null)
            {
                Debug.LogWarning("[Coop] GameBalanceSO not assigned!");
            }

            if (entryPoint == null)
            {
                Debug.LogWarning("[Coop] Entry point not assigned! Using coop position as fallback.");
            }
        }

        public bool HasAvailableSpot()
        {
            return chickensInside.Count < GetMaxCapacity();
        }

        private int GetMaxCapacity()
        {
            if (currentConfig != null) return currentConfig.sleepingSpots;
            return 15;
        }

        public Transform GetEntryPoint()
        {
            return entryPoint != null ? entryPoint : transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            Chicken.Chicken chicken = other.GetComponent<Chicken.Chicken>();
            if (chicken != null && !chickensInside.Contains(chicken))
            {
                chickensInside.Add(chicken);
                
                if (chicken.CurrentState == Chicken.ChickenState.GoingToSleep)
                {
                    HideChickenWithAnimation(chicken);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Chicken.Chicken chicken = other.GetComponent<Chicken.Chicken>();
            if (chicken != null && chickensInside.Contains(chicken))
            {
                chickensInside.Remove(chicken);
                chicken.ShowVisual();
            }
        }

        private void HideChickenWithAnimation(Chicken.Chicken chicken)
        {
            Transform visualRoot = chicken.GetVisualRoot();
            if (visualRoot != null && visualRoot.gameObject.activeSelf)
            {
                visualRoot.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => visualRoot.gameObject.SetActive(false));
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

            if (entryPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(entryPoint.position, 0.5f);
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
            
            int occupied = chickensInside.Count;
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

        private void OnDestroy()
        {
            if (chickensInside.Count > 0)
            {
                ScatterChickens();
            }
        }

        private void ScatterChickens()
        {
            float scatterRadius = gameBalance != null ? gameBalance.coopDestroyedScatterRadius : 8f;
            float scatterForce = gameBalance != null ? gameBalance.coopDestroyedScatterForce : 1f;

            List<Chicken.Chicken> chickensToScatter = new List<Chicken.Chicken>(chickensInside);

            foreach (var chicken in chickensToScatter)
            {
                if (chicken != null)
                {
                    chicken.ShowVisual();

                    Vector2 randomCircle = Random.insideUnitCircle * scatterRadius * scatterForce;
                    Vector3 scatterOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
                    Vector3 targetPosition = transform.position + scatterOffset;

                    if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, scatterRadius, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        chicken.transform.position = hit.position;
                    }
                    else
                    {
                        chicken.transform.position = targetPosition;
                    }

                    chicken.ChangeState(Chicken.ChickenState.Idle);
                }
            }

            chickensInside.Clear();
        }
    }
}
