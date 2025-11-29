using UnityEngine;
using GallinasFelices.Data;

namespace GallinasFelices.Core
{
    public class FarmManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private Transform spawnPoint;

        [Header("Visual Prefabs")]
        [Tooltip("Prefabs visuales de gallinas (PFB_*) que se instancian como hijos")]
        [SerializeField] private GameObject[] chickenVisualPrefabs;

        [Header("Starting Setup")]
        [SerializeField] private int startingChickens = 3;
        [SerializeField] private ChickenPersonalitySO[] availablePersonalities;
        [SerializeField] private ChickenConfigSO[] availableConfigs;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 5f;

        public int CurrentChickenCount { get; private set; }
        private int totalChickensSpawned = 0;
        
        public event System.Action<int, int> OnChickenCountChanged; // (current, max)

        public static FarmManager Instance { get; private set; }

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
        }

        private void Start()
        {
            SpawnInitialChickens();
        }

        private void SpawnInitialChickens()
        {
            for (int i = 0; i < startingChickens; i++)
            {
                SpawnChicken();
            }
        }

        public void SpawnChicken()
        {
            if (chickenPrefab == null)
            {
                Debug.LogWarning("[FarmManager] Chicken prefab not assigned!");
                return;
            }

            Vector3 spawnPosition = GetSpawnPosition();
            GameObject chickenObject = Instantiate(chickenPrefab, spawnPosition, Quaternion.identity);

            totalChickensSpawned++;
            chickenObject.name = $"Chicken_{totalChickensSpawned:D3}";

            ConfigureChicken(chickenObject);

            CurrentChickenCount++;
            OnChickenCountChanged?.Invoke(CurrentChickenCount, GetMaxChickens());
        }

        private void ConfigureChicken(GameObject chickenObject)
        {
            AttachRandomVisual(chickenObject);
            SetRandomPersonality(chickenObject);
            SetRandomConfig(chickenObject);
            SubscribeToEvents(chickenObject);
            
            #if UNITY_EDITOR
            if (!chickenObject.TryGetComponent<HappyChickens.Debug.ChickenDebugger>(out _))
            {
                chickenObject.AddComponent<HappyChickens.Debug.ChickenDebugger>();
            }
            #endif
        }

        private void AttachRandomVisual(GameObject chickenObject)
        {
            if (chickenVisualPrefabs == null || chickenVisualPrefabs.Length == 0)
            {
                Debug.LogWarning("[FarmManager] No visual prefabs assigned!");
                return;
            }

            GameObject randomVisual = chickenVisualPrefabs[Random.Range(0, chickenVisualPrefabs.Length)];
            GameObject visualInstance = Instantiate(randomVisual, chickenObject.transform);
            visualInstance.transform.localPosition = new Vector3(0f, -1f, 0f);
            visualInstance.transform.localRotation = Quaternion.identity;

            if (chickenObject.TryGetComponent<Chicken.Chicken>(out var chicken))
            {
                chicken.SetVisualRoot(visualInstance.transform);
            }

            if (chickenObject.TryGetComponent<Chicken.ChickenIdleFidget>(out var fidget))
            {
                fidget.SetRotationTarget(visualInstance.transform);
            }
        }

        private void SetRandomPersonality(GameObject chickenObject)
        {
            if (!chickenObject.TryGetComponent<Chicken.Chicken>(out var chicken))
            {
                return;
            }

            if (availablePersonalities == null || availablePersonalities.Length == 0)
            {
                Debug.LogWarning("[FarmManager] No personalities available!");
                return;
            }

            ChickenPersonalitySO randomPersonality = availablePersonalities[Random.Range(0, availablePersonalities.Length)];
            chicken.SetPersonality(randomPersonality);
        }

        private void SetRandomConfig(GameObject chickenObject)
        {
            if (!chickenObject.TryGetComponent<Chicken.Chicken>(out var chicken))
            {
                return;
            }

            if (availableConfigs == null || availableConfigs.Length == 0)
            {
                Debug.LogWarning("[FarmManager] No configs available!");
                return;
            }

            ChickenConfigSO randomConfig = availableConfigs[Random.Range(0, availableConfigs.Length)];
            chicken.SetConfig(randomConfig);
        }

        private void SubscribeToEvents(GameObject chickenObject)
        {
            if (chickenObject.TryGetComponent<Chicken.ChickenLifespan>(out var lifespan))
            {
                lifespan.OnChickenDied += OnChickenDied;
            }
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition = spawnPoint != null ? spawnPoint.position : transform.position;
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return new Vector3(basePosition.x + randomOffset.x, basePosition.y, basePosition.z + randomOffset.y);
        }

        public void BuyChicken(int cost)
        {
            if (chickenPrefab == null)
            {
                Debug.LogError("[FarmManager] chickenPrefab is NULL! Cannot spawn chicken.");
                return;
            }
            
            if (EggCounter.Instance != null && EggCounter.Instance.TrySpendEggs(cost))
            {
                SpawnChicken();
            }
            else
            {
                Debug.LogWarning("[FarmManager] Not enough eggs to buy chicken!");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, spawnRadius);
        }

        public void OnChickenDied(Chicken.Chicken chicken)
        {
            CurrentChickenCount--;
            OnChickenCountChanged?.Invoke(CurrentChickenCount, GetMaxChickens());
            if (chicken.TryGetComponent<Chicken.ChickenLifespan>(out var lifespan))
            {
                lifespan.OnChickenDied -= OnChickenDied;
            }
        }

        private int GetMaxChickens()
        {
            if (FarmLimits.Instance != null)
            {
                return FarmLimits.Instance.GetMaxChickens();
            }
            return 0;
        }
    }
}
