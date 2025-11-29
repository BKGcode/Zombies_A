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
            if (chicken.CurrentState == ChickenState.Sleeping)
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
                    TryProduceEgg();
                }
                return;
            }

            if (chicken.CurrentState == ChickenState.GoingToNest)
            {
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

            Nest nest = FindAvailableNest();
            if (nest != null)
            {
                waitingForNest = false;
                float distance = Vector3.Distance(transform.position, nest.transform.position);
                Debug.Log($"[{chicken.ChickenName}] Found available nest at {distance:F2}m. Going to lay egg.");
                
                currentNest = nest;
                currentNest.TryOccupy();
                
                if (agent != null)
                {
                    agent.SetDestination(currentNest.transform.position);
                    chicken.ChangeState(ChickenState.GoingToNest);
                }
            }
            else
            {
                if (!waitingForNest)
                {
                    Debug.Log($"[{chicken.ChickenName}] No available nests. Will retry every {retryDelay}s while continuing normal behavior.");
                }
                
                waitingForNest = true;
                retryTimer = Random.Range(retryDelay * 0.8f, retryDelay * 1.2f);
            }
        }

        public void OnReachedNest()
        {
            Debug.Log($"[{chicken.ChickenName}] Reached nest! Starting to lay egg.");
            chicken.StartLayingEgg();
            OnProductionStarted?.Invoke();
            chicken.OnStateChanged.AddListener(OnChickenStateChanged);
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
            
            Nest[] allNests = FindObjectsOfType<Nest>();
            foreach (var nest in allNests)
            {
                if (nest != null)
                {
                    Gizmos.DrawLine(transform.position, nest.transform.position);
                }
            }
        }
    }
}
