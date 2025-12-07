using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using GallinasFelices.Core;
using GallinasFelices.Data;
using GallinasFelices.Structures;

namespace GallinasFelices.Chicken
{
    public class ChickenEggProducer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Chicken chicken;
        [SerializeField] private GameBalanceSO gameBalance;

        [Header("Production Settings")]
        [SerializeField] private float productionTimer = 0f;

        [Header("Nest Finding")]
        [SerializeField] private float retryDelay = 2f;

        [Header("Events")]
        public UnityEvent OnProductionStarted;
        public UnityEvent OnEggProduced;

        private Nest currentNest;
        private NavMeshAgent agent;
        private float retryTimer = 0f;
        private bool waitingForNest = false;

        private static System.Collections.Generic.List<Nest> cachedNests = new System.Collections.Generic.List<Nest>();

        private void Awake()
        {
            if (chicken == null)
            {
                chicken = GetComponent<Chicken>();
            }

            agent = GetComponent<NavMeshAgent>();
            ResetProductionTimer();
        }

        private void Start()
        {
            RefreshNestCache();
        }

        private static void RefreshNestCache()
        {
            if (cachedNests.Count == 0)
            {
                cachedNests.AddRange(FindObjectsOfType<Nest>());
            }
        }

        public static void RegisterNest(Nest nest)
        {
            if (!cachedNests.Contains(nest))
            {
                cachedNests.Add(nest);
            }
        }

        public static void UnregisterNest(Nest nest)
        {
            cachedNests.Remove(nest);
        }

        private void Update()
        {
            if (chicken.CurrentState == ChickenState.Sleeping ||
                chicken.CurrentState == ChickenState.GoingToNest ||
                chicken.CurrentState == ChickenState.LayingEgg ||
                chicken.CurrentState == ChickenState.GoingToSleep)
            {
                return;
            }

            if (TimeController.Instance != null && TimeController.Instance.IsNightTime())
            {
                return;
            }

            if (waitingForNest)
            {
                retryTimer -= Time.deltaTime;
                if (retryTimer <= 0f)
                {
                    waitingForNest = false;
                    TryProduceEgg();
                }
                return;
            }

            UpdateProduction();
        }

        private void UpdateProduction()
        {
            productionTimer -= Time.deltaTime * GetProductionMultiplier();

            if (productionTimer <= 0f)
            {
                TryProduceEgg();
            }
        }

        private void TryProduceEgg()
        {
            if (chicken.CurrentState == ChickenState.LayingEgg || chicken.CurrentState == ChickenState.GoingToNest)
            {
                return;
            }
            
            if (chicken.Needs.Hunger >= 80f || chicken.Needs.Thirst >= 80f)
            {
                waitingForNest = true;
                retryTimer = Random.Range(retryDelay * 0.8f, retryDelay * 1.2f);
                return;
            }

            Nest nest = FindAvailableNest();
            if (nest != null)
            {
                waitingForNest = false;
                float distance = Vector3.Distance(transform.position, nest.transform.position);
                chicken.GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("EggProduction", $"Found nest at {distance:F2}m. Going to lay egg", HappyChickens.Debug.EventSeverity.Info);
                
                currentNest = nest;
                currentNest.TryOccupy();
                
                chicken.ChangeState(ChickenState.GoingToNest);
                
                if (agent != null)
                {
                    agent.SetDestination(currentNest.transform.position);
                }
            }
            else
            {
                if (!waitingForNest)
                {
                    chicken.GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("EggProduction", $"No available nests. Will retry every {retryDelay}s", HappyChickens.Debug.EventSeverity.Warning);
                }
                
                waitingForNest = true;
                retryTimer = Random.Range(retryDelay * 0.8f, retryDelay * 1.2f);
            }
        }

        public void OnReachedNest()
        {
            chicken.GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("EggProduction", "Reached nest. Starting to lay egg", HappyChickens.Debug.EventSeverity.Info);
            
            chicken.OnStateChanged.RemoveListener(OnChickenStateChanged);
            chicken.OnStateChanged.AddListener(OnChickenStateChanged);
            
            chicken.ChangeState(ChickenState.LayingEgg);
            
            float duration = chicken.Personality != null
                ? Random.Range(chicken.Personality.minLayingEggDuration, chicken.Personality.maxLayingEggDuration)
                : Random.Range(4f, 7f);
            chicken.SetStateTimer(duration);
            
            OnProductionStarted?.Invoke();
        }

        public void CancelEggProduction()
        {
            if (currentNest != null)
            {
                chicken.GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("EggProduction", "Egg production cancelled. Releasing nest", HappyChickens.Debug.EventSeverity.Warning);
                currentNest.Release();
                currentNest = null;
            }
            
            waitingForNest = false;
            ResetProductionTimer();
        }

        private void OnChickenStateChanged(ChickenState newState)
        {
            if (newState != ChickenState.LayingEgg && currentNest != null)
            {
                currentNest.SpawnEgg();
                currentNest.Release();
                currentNest = null;

                OnEggProduced?.Invoke();
                ResetProductionTimer();

                chicken.OnStateChanged.RemoveListener(OnChickenStateChanged);
            }
        }

        private Nest FindAvailableNest()
        {
            if (cachedNests.Count == 0)
            {
                RefreshNestCache();
            }

            System.Collections.Generic.List<Nest> availableNests = new System.Collections.Generic.List<Nest>();

            foreach (var nest in cachedNests)
            {
                if (nest != null && !nest.IsOccupied && !nest.HasEgg())
                {
                    availableNests.Add(nest);
                }
            }

            if (availableNests.Count == 0)
            {
                return null;
            }

            availableNests.Sort((a, b) => 
            {
                float distA = Vector3.Distance(transform.position, a.transform.position);
                float distB = Vector3.Distance(transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });

            return availableNests[0];
        }

        private float GetProductionMultiplier()
        {
            float multiplier = 1f;

            if (chicken.Happiness != null)
            {
                multiplier *= chicken.Happiness.GetProductionMultiplier();
            }

            return multiplier;
        }

        private void ResetProductionTimer()
        {
            waitingForNest = false;
            
            if (chicken != null && chicken.Personality != null)
            {
                productionTimer = Random.Range(chicken.Personality.minEggProductionTime, chicken.Personality.maxEggProductionTime);
            }
            else
            {
                productionTimer = Random.Range(15f, 20f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            
            foreach (var nest in cachedNests)
            {
                if (nest != null)
                {
                    Gizmos.DrawLine(transform.position, nest.transform.position);
                }
            }
        }
    }
}
