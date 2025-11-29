using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using GallinasFelices.Core;
using GallinasFelices.Data;
using GallinasFelices.Structures;

namespace GallinasFelices.Chicken
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Chicken : MonoBehaviour, Core.IInteractable
    {
        [Header("Identity")]
        [SerializeField] private string chickenName = "Chicken";
        [SerializeField] private ChickenPersonalitySO personality;
        [SerializeField] private ChickenConfigSO chickenConfig;
        [SerializeField] private UITextsConfigSO uiTexts;

        [Header("References")]
        [SerializeField] private Transform visualRoot;

        [Header("Events")]
        public UnityEvent<ChickenState> OnStateChanged;
        public UnityEvent OnEggLaid;

        public ChickenState CurrentState { get; private set; }
        public ChickenNeeds Needs { get; private set; }
        public ChickenHappiness Happiness { get; private set; }
        public ChickenPersonalitySO Personality => personality;
        public ChickenConfigSO ChickenConfig => chickenConfig;
        public string ChickenName => chickenName;
        public Transform VisualRoot => visualRoot;

        private NavMeshAgent agent;
        private float stateTimer;
        private Vector3 targetPosition;
        private Transform currentTarget;
        private float baseSpeed;
        private float pauseCooldown;
        private bool isPaused;
        private float speedChangeTimer;
        private float currentSpeedMultiplier = 1f;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            Needs = new ChickenNeeds();
            Happiness = new ChickenHappiness();

            if (personality != null)
            {
                agent.speed *= personality.walkSpeedMultiplier;
                agent.avoidancePriority = personality.avoidancePriority;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            }

            baseSpeed = agent.speed;
        }

        private void Start()
        {
            if (chickenConfig != null)
            {
                chickenName = chickenConfig.GetRandomName();
            }

            ChangeState(ChickenState.Idle);

            if (TimeController.Instance != null)
            {
                TimeController.Instance.OnTimeOfDayChanged.AddListener(OnTimeOfDayChanged);
            }
        }

        private void Update()
        {
            UpdateNeeds();
            Happiness.UpdateHappiness(Time.deltaTime, 0.1f);
            UpdateContinuousSpeedVariation();
            UpdateStateMachine();
        }

        private void UpdateNeeds()
        {
            if (CurrentState != ChickenState.Sleeping)
            {
                Needs.IncreaseHunger(Time.deltaTime * 1f);
                Needs.IncreaseThirst(Time.deltaTime * 1.2f);
                Needs.DecreaseEnergy(Time.deltaTime * 0.8f);
            }

            Happiness.SetHasFood(!Needs.IsHungry());
            Happiness.SetHasWater(!Needs.IsThirsty());
            Happiness.SetIsRested(!Needs.IsTired());
        }

        private void UpdateStateMachine()
        {
            stateTimer -= Time.deltaTime;

            switch (CurrentState)
            {
                case ChickenState.Idle:
                    HandleIdleState();
                    break;

                case ChickenState.Walking:
                    HandleWalkingState();
                    break;

                case ChickenState.Eating:
                    HandleEatingState();
                    break;

                case ChickenState.Drinking:
                    HandleDrinkingState();
                    break;

                case ChickenState.Sleeping:
                    HandleSleepingState();
                    break;

                case ChickenState.GoingToNest:
                    HandleGoingToNestState();
                    break;

                case ChickenState.LayingEgg:
                    HandleLayingEggState();
                    break;

                case ChickenState.Exploring:
                    HandleExploringState();
                    break;

                case ChickenState.GoingToEat:
                    HandleGoingToEatState();
                    break;

                case ChickenState.GoingToDrink:
                    HandleGoingToDrinkState();
                    break;

                case ChickenState.GoingToSleep:
                    HandleGoingToSleepState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            if (TimeController.Instance != null && TimeController.Instance.IsNightTime())
            {
                if (CurrentState != ChickenState.Sleeping)
                {
                    GoToSleep();
                }
                return;
            }

            if (stateTimer <= 0f)
            {
                if (ShouldHandleNeeds())
                {
                    return;
                }

                if (Random.value < 0.7f)
                {
                    StartWandering();
                }
                else
                {
                    if (personality != null)
                    {
                        float idleTime = Random.Range(personality.minIdleTime, personality.maxIdleTime);
                        idleTime *= personality.idleTimeMultiplier;
                        stateTimer = idleTime;
                    }
                    else
                    {
                        stateTimer = Random.Range(1f, 4f);
                    }
                }
            }
        }

        private void HandleWalkingState()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (personality != null)
                {
                    float waitTime = Random.Range(personality.minWaitAfterWalk, personality.maxWaitAfterWalk);
                    waitTime *= personality.idleTimeMultiplier;
                    stateTimer = waitTime;
                }
                else
                {
                    stateTimer = Random.Range(2f, 6f);
                }
                
                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleEatingState()
        {
            if (stateTimer <= 0f)
            {
                Needs.Feed(50f);
                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleDrinkingState()
        {
            if (stateTimer <= 0f)
            {
                Needs.GiveWater(50f);
                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleSleepingState()
        {
            Needs.RestoreEnergy(Time.deltaTime * 20f);

            if (TimeController.Instance != null && !TimeController.Instance.IsNightTime())
            {
                if (Needs.Energy >= 80f)
                {
                    ChangeState(ChickenState.Idle);
                }
            }
        }

        private void HandleLayingEggState()
        {
            if (stateTimer <= 0f)
            {
                OnEggLaid?.Invoke();
                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleExploringState()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleGoingToNestState()
        {
            if (agent.pathPending)
            {
                return;
            }

            float arrivalThreshold = agent.stoppingDistance + 0.5f;
            
            if (agent.remainingDistance <= arrivalThreshold)
            {
                ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                if (producer != null)
                {
                    producer.OnReachedNest();
                }
            }
        }

        private void HandleGoingToEatState()
        {
            UpdateRandomPause();

            if (agent.pathPending)
            {
                return;
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                ChangeState(ChickenState.Eating);
                
                if (personality != null)
                {
                    stateTimer = Random.Range(personality.minEatingDuration, personality.maxEatingDuration);
                }
                else
                {
                    stateTimer = Random.Range(4f, 8f);
                }
            }
        }

        private void HandleGoingToDrinkState()
        {
            UpdateRandomPause();

            if (agent.pathPending)
            {
                return;
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                ChangeState(ChickenState.Drinking);
                
                if (personality != null)
                {
                    stateTimer = Random.Range(personality.minDrinkingDuration, personality.maxDrinkingDuration);
                }
                else
                {
                    stateTimer = Random.Range(2f, 4f);
                }
            }
        }

        private void HandleGoingToSleepState()
        {
            UpdateRandomPause();

            if (agent.pathPending)
            {
                return;
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                ChangeState(ChickenState.Sleeping);
                agent.isStopped = true;
            }
        }

        private void UpdateRandomPause()
        {
            if (personality == null || !IsNavigationState(CurrentState))
            {
                return;
            }

            pauseCooldown -= Time.deltaTime;

            if (!isPaused && pauseCooldown <= 0f)
            {
                float pauseChance = personality.pauseFrequency * 2f;
                
                if (Random.value < pauseChance)
                {
                    isPaused = true;
                    pauseCooldown = Random.Range(0.8f, 2f);
                    agent.speed = 0f;
                }
                else
                {
                    pauseCooldown = Random.Range(1f, 3f);
                }
            }
            else if (isPaused && pauseCooldown <= 0f)
            {
                isPaused = false;
                pauseCooldown = Random.Range(2f, 5f);
                agent.speed = baseSpeed * currentSpeedMultiplier;
            }
        }

        private void UpdateContinuousSpeedVariation()
        {
            if (!IsNavigationState(CurrentState) || personality == null || isPaused)
            {
                return;
            }

            speedChangeTimer -= Time.deltaTime;

            if (speedChangeTimer <= 0f)
            {
                float variationStrength = personality.speedVariation * 0.5f;
                float targetMultiplier = Random.Range(1f - variationStrength, 1f + variationStrength);
                
                if (Random.value < 0.1f)
                {
                    targetMultiplier *= Random.Range(1.3f, 1.6f);
                }

                currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetMultiplier, 0.3f);
                agent.speed = baseSpeed * currentSpeedMultiplier;

                speedChangeTimer = Random.Range(0.5f, 2f);
            }
        }

        private bool ShouldHandleNeeds()
        {
            if (CurrentState == ChickenState.GoingToNest || 
                CurrentState == ChickenState.GoingToEat || 
                CurrentState == ChickenState.GoingToDrink || 
                CurrentState == ChickenState.GoingToSleep)
            {
                return false;
            }

            if (ShouldSleep()) return true;
            if (ShouldNap()) return true;
            if (ShouldEat()) return true;
            if (ShouldDrink()) return true;

            return false;
        }

        private bool ShouldSleep()
        {
            if (TimeController.Instance != null && TimeController.Instance.IsNightTime())
            {
                GoToSleep();
                return true;
            }
            return false;
        }

        private bool ShouldNap()
        {
            if (TimeController.Instance != null && !TimeController.Instance.IsNightTime() && Needs.Energy < 50f)
            {
                TryNap();
                return true;
            }
            return false;
        }

        private bool ShouldEat()
        {
            if (Needs.IsHungry())
            {
                TryEat();
                return true;
            }
            return false;
        }

        private bool ShouldDrink()
        {
            if (Needs.IsThirsty())
            {
                TryDrink();
                return true;
            }
            return false;
        }

        private void ApplyRandomSpeed()
        {
            if (personality == null)
            {
                return;
            }

            float variation = Random.Range(-personality.speedVariation, personality.speedVariation);
            float newSpeed = baseSpeed * (1f + variation);
            agent.speed = newSpeed;
        }

        private void StartWandering()
        {
            float radius = personality != null ? personality.wanderRadius : 10f;
            
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
                agent.SetDestination(targetPosition);
                ChangeState(ChickenState.Walking);
            }
        }

        public void TryEat()
        {
            Structures.Feeder feeder = FindAvailableFeeder();
            if (feeder != null)
            {
                agent.SetDestination(feeder.GetFeedingPosition());
                ChangeState(ChickenState.GoingToEat);
            }
            else
            {
                stateTimer = Random.Range(2f, 5f);
                ChangeState(ChickenState.Idle);
            }
        }

        private Structures.Feeder FindAvailableFeeder()
        {
            Structures.Feeder[] feeders = FindObjectsOfType<Structures.Feeder>();
            Structures.Feeder closestFeeder = null;
            float closestDistance = float.MaxValue;

            foreach (var feeder in feeders)
            {
                if (feeder != null && !feeder.IsEmpty && !feeder.IsFull)
                {
                    StructureDurability durability = feeder.GetComponent<StructureDurability>();
                    if (durability != null && durability.IsBroken)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(transform.position, feeder.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestFeeder = feeder;
                    }
                }
            }

            return closestFeeder;
        }

        public void TryDrink()
        {
            Structures.WaterTrough waterTrough = FindAvailableWaterTrough();
            if (waterTrough != null)
            {
                agent.SetDestination(waterTrough.GetDrinkingPosition());
                ChangeState(ChickenState.GoingToDrink);
            }
            else
            {
                stateTimer = Random.Range(2f, 5f);
                ChangeState(ChickenState.Idle);
            }
        }

        private Structures.WaterTrough FindAvailableWaterTrough()
        {
            Structures.WaterTrough[] waterTroughs = FindObjectsOfType<Structures.WaterTrough>();
            Structures.WaterTrough closestTrough = null;
            float closestDistance = float.MaxValue;

            foreach (var trough in waterTroughs)
            {
                if (trough != null && !trough.IsEmpty && !trough.IsFull)
                {
                    StructureDurability durability = trough.GetComponent<StructureDurability>();
                    if (durability != null && durability.IsBroken)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(transform.position, trough.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTrough = trough;
                    }
                }
            }

            return closestTrough;
        }

        public void GoToSleep()
        {
            Structures.Coop coop = FindAvailableCoop();
            if (coop != null)
            {
                Transform sleepingSpot = coop.AssignSpot(this);
                if (sleepingSpot != null)
                {
                    agent.SetDestination(sleepingSpot.position);
                    ChangeState(ChickenState.GoingToSleep);
                    return;
                }
            }

            ChangeState(ChickenState.Sleeping);
            agent.isStopped = true;
        }

        private Structures.Coop FindAvailableCoop()
        {
            Structures.Coop[] coops = FindObjectsOfType<Structures.Coop>();
            Structures.Coop closestCoop = null;
            float closestDistance = float.MaxValue;

            foreach (var coop in coops)
            {
                if (coop != null && coop.HasAvailableSpot())
                {
                    StructureDurability durability = coop.GetComponent<StructureDurability>();
                    if (durability != null && durability.IsBroken)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(transform.position, coop.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCoop = coop;
                    }
                }
            }

            return closestCoop;
        }

        public void TryNap()
        {
            ChangeState(ChickenState.Sleeping);
            agent.isStopped = true;
        }

        public void StartLayingEgg()
        {
            ChangeState(ChickenState.LayingEgg);
            
            if (personality != null)
            {
                stateTimer = Random.Range(personality.minLayingEggDuration, personality.maxLayingEggDuration);
            }
            else
            {
                stateTimer = Random.Range(4f, 7f);
            }
        }

        public void ChangeState(ChickenState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            if (CurrentState == ChickenState.Sleeping && newState != ChickenState.Sleeping)
            {
                Structures.Coop coop = FindObjectOfType<Structures.Coop>();
                if (coop != null)
                {
                    coop.ReleaseSpot(this);
                }
            }

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            if (newState == ChickenState.Sleeping)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }

            if (IsNavigationState(newState))
            {
                ApplyRandomSpeed();
                pauseCooldown = Random.Range(2f, 5f);
                isPaused = false;
                speedChangeTimer = Random.Range(0.5f, 1.5f);
            }
        }

        private bool IsNavigationState(ChickenState state)
        {
            return state == ChickenState.Walking ||
                   state == ChickenState.GoingToEat ||
                   state == ChickenState.GoingToDrink ||
                   state == ChickenState.GoingToSleep ||
                   state == ChickenState.GoingToNest;
        }

        private void OnTimeOfDayChanged(TimeOfDay timeOfDay)
        {
            if (timeOfDay == TimeOfDay.Night)
            {
                GoToSleep();
            }
        }

        public void SetPersonality(ChickenPersonalitySO newPersonality)
        {
            personality = newPersonality;
            if (agent != null && personality != null)
            {
                agent.speed *= personality.walkSpeedMultiplier;
            }
        }

        public void SetConfig(ChickenConfigSO newConfig)
        {
            chickenConfig = newConfig;
        }

        public void SetVisualRoot(Transform visual)
        {
            visualRoot = visual;
        }

        private void OnDestroy()
        {
            if (TimeController.Instance != null)
            {
                TimeController.Instance.OnTimeOfDayChanged.RemoveListener(OnTimeOfDayChanged);
            }
        }

        private void OnDrawGizmosSelected()
        {
            float radius = personality != null ? personality.wanderRadius : 10f;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);

            if (Application.isPlaying && agent != null && agent.hasPath)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, agent.destination);
            }
        }

        public string GetTitle()
        {
            return chickenName;
        }

        public string GetMainInfo()
        {
            string stateText = CurrentState.ToString();
            if (chickenConfig != null)
            {
                stateText = chickenConfig.GetStateText(CurrentState);
            }

            string info = stateText;

            if (Needs.IsHungry() && uiTexts != null)
            {
                info += $"\n{uiTexts.hungerWarning}";
            }
            if (Needs.IsThirsty() && uiTexts != null)
            {
                info += $"\n{uiTexts.thirstWarning}";
            }
            
            return info;
        }

        public Core.InteractionBar[] GetBars()
        {
            ChickenLifespan lifespan = GetComponent<ChickenLifespan>();
            float age = lifespan != null ? lifespan.CurrentAge : 0f;
            float maxAge = lifespan != null ? lifespan.MaxLifespan : 30f;
            float remainingLife = maxAge - age;

            string ageLabel = chickenConfig != null ? chickenConfig.ageLabel : 
                              (uiTexts != null ? uiTexts.lifeLabel : string.Empty);
            string happinessLabel = chickenConfig != null ? chickenConfig.happinessLabel : 
                                    (uiTexts != null ? uiTexts.happinessLabel : string.Empty);

            return new Core.InteractionBar[]
            {
                new Core.InteractionBar(ageLabel, remainingLife, maxAge),
                new Core.InteractionBar(happinessLabel, Happiness.CurrentHappiness, 100f)
            };
        }

        public Core.InteractionButton[] GetActions()
        {
            return new Core.InteractionButton[0];
        }
    }
}
