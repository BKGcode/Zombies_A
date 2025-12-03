using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
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
        [SerializeField] private GameBalanceSO gameBalance;
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
        public bool IsSleepingOutside { get; private set; }

        public Transform GetVisualRoot()
        {
            return visualRoot;
        }

        public void ShowVisual()
        {
            if (visualRoot != null && !visualRoot.gameObject.activeSelf)
            {
                visualRoot.gameObject.SetActive(true);
                visualRoot.localScale = Vector3.zero;
                visualRoot.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
        }

        private NavMeshAgent agent;
        private float stateTimer;
        private Vector3 targetPosition;
        private Transform currentTarget;
        private float baseSpeed;
        private float pauseCooldown;
        private bool isPaused;
        private float speedChangeTimer;
        private float currentSpeedMultiplier = 1f;
        private float pathDeviationTimer;
        private Vector3 currentPathDeviation;
        private Vector3 finalDestination;
        private bool hasIntermediateWaypoint;
        private Vector3 intermediateWaypoint;
        private float navigationTimeout = 0f;
        private const float MAX_NAVIGATION_TIME = 30f;

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
                agent.angularSpeed = personality.rotationSpeed;
                agent.updateRotation = !personality.stopWhileRotating;
            }
            else
            {
                agent.angularSpeed = 540f;
                agent.updateRotation = false;
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

        private void OnDestroy()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }
            
            if (TimeController.Instance != null)
            {
                TimeController.Instance.OnTimeOfDayChanged.RemoveListener(OnTimeOfDayChanged);
            }
        }

        private void Update()
        {
            UpdateNeeds();
            Happiness.UpdateHappiness(Time.deltaTime, 0.1f);
            UpdateContinuousSpeedVariation();
            HandleManualRotation();
            UpdateStateMachine();
        }

        private void HandleManualRotation()
        {
            if (personality == null || !personality.stopWhileRotating || !IsNavigationState(CurrentState))
            {
                return;
            }

            if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
            {
                Vector3 direction = (agent.steeringTarget - transform.position).normalized;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                {
                    float angle = Vector3.Angle(transform.forward, direction);

                    if (angle > personality.minAngleToStopRotating)
                    {
                        agent.isStopped = true;

                        float rotationSpeed = personality.rotationSpeed * Time.deltaTime;
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

                        if (angle < 5f)
                        {
                            agent.isStopped = false;
                        }
                    }
                    else if (agent.isStopped && angle < 5f)
                    {
                        agent.isStopped = false;
                    }
                }
            }
        }

        private void UpdateNeeds()
        {
            if (CurrentState != ChickenState.Sleeping)
            {
                Needs.IncreaseHunger(Time.deltaTime * 1f);
                Needs.IncreaseThirst(Time.deltaTime * 1.2f);
                Needs.DecreaseEnergy(Time.deltaTime * 0.8f);
            }
            else
            {
                Needs.IncreaseHunger(Time.deltaTime * 0.1f);
                Needs.IncreaseThirst(Time.deltaTime * 0.12f);
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
            // CRITICAL: Check needs BEFORE night time to prevent chickens sleeping with 100% hunger/thirst
            if (stateTimer <= 0f)
            {
                if (ShouldHandleNeeds())
                {
                    return;
                }
            }

            // Only go to sleep at night if needs are not critical
            if (TimeController.Instance != null && TimeController.Instance.IsNightTime())
            {
                if (CurrentState != ChickenState.Sleeping)
                {
                    Debug.Log($"[{ChickenName}] NIGHT TIME - Going to sleep. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%");
                    GoToSleep();
                }
                return;
            }

            if (stateTimer <= 0f)
            {
                Debug.Log($"[{ChickenName}] IDLE TIMER EXPIRED - H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}% E:{Needs.Energy:F0}%");
                
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
            UpdatePathDeviation();

            bool hasArrived = !agent.pathPending && agent.remainingDistance <= 0.8f;

            if (hasArrived)
            {
                if (hasIntermediateWaypoint)
                {
                    hasIntermediateWaypoint = false;
                    agent.SetDestination(finalDestination);
                    Debug.Log($"[{ChickenName}] WAYPOINT REACHED - Going to final destination");
                    return;
                }

                Debug.Log($"[{ChickenName}] WALKING COMPLETED - Switching to idle");

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
            // Simple pecking animation while eating
            if (stateTimer > 0.5f && visualRoot != null)
            {
                float peckInterval = 0.5f;
                if (stateTimer % peckInterval < Time.deltaTime)
                {
                    // Quick peck down and up
                    visualRoot.DOLocalMoveY(-0.1f, 0.1f).SetRelative(true).SetEase(Ease.InQuad)
                        .OnComplete(() => {
                            visualRoot.DOLocalMoveY(0.1f, 0.1f).SetRelative(true).SetEase(Ease.OutQuad);
                        });
                }
            }
            
            if (stateTimer <= 0f)
            {
                Needs.Feed(50f);
                agent.isStopped = false;
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Eating", $"Finished eating. Hunger: {Needs.Hunger:F0}%", HappyChickens.Debug.EventSeverity.Info);
                Debug.Log($"[{ChickenName}] FINISHED EATING - Hunger now: {Needs.Hunger:F0}%, agent resumed");
                ChangeState(ChickenState.Idle);
                stateTimer = 1f; // Give time to decide next action
            }
        }

        private void HandleDrinkingState()
        {
            // Simple drinking animation
            if (stateTimer > 0.5f && visualRoot != null)
            {
                float drinkInterval = 0.6f;
                if (stateTimer % drinkInterval < Time.deltaTime)
                {
                    // Quick head dip
                    visualRoot.DOLocalMoveY(-0.08f, 0.15f).SetRelative(true).SetEase(Ease.InOutQuad)
                        .OnComplete(() => {
                            visualRoot.DOLocalMoveY(0.08f, 0.15f).SetRelative(true).SetEase(Ease.InOutQuad);
                        });
                }
            }
            
            if (stateTimer <= 0f)
            {
                Needs.GiveWater(50f);
                agent.isStopped = false;
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Drinking", $"Finished drinking. Thirst: {Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Info);
                Debug.Log($"[{ChickenName}] FINISHED DRINKING - Thirst now: {Needs.Thirst:F0}%, agent resumed");
                ChangeState(ChickenState.Idle);
                stateTimer = 1f; // Give time to decide next action
            }
        }

        private void HandleSleepingState()
        {
            // CRITICAL: Wake up immediately if needs become critical while sleeping
            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Sleep", $"Waking up due to critical needs! H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Critical);
                ShowVisual();
                ChangeState(ChickenState.Idle);
                stateTimer = 0.5f; // Short timer to trigger need handling
                return;
            }

            float nightDurationSeconds = 360f;
            if (TimeController.Instance != null && gameBalance != null)
            {
                float nightHours = 24f / (gameBalance.dayNightRatio + 1f);
                nightDurationSeconds = nightHours * gameBalance.secondsPerGameHour;
            }

            float energyPerSecond = 100f / nightDurationSeconds;
            Needs.RestoreEnergy(Time.deltaTime * energyPerSecond);

            if (TimeController.Instance != null && !TimeController.Instance.IsNightTime())
            {
                if (Needs.Energy >= 100f)
                {
                    ShowVisual();
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
            UpdatePathDeviation();

            bool hasArrived = !agent.pathPending && agent.remainingDistance <= 0.8f;

            if (hasArrived)
            {
                if (hasIntermediateWaypoint)
                {
                    hasIntermediateWaypoint = false;
                    agent.SetDestination(finalDestination);
                    return;
                }

                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleGoingToNestState()
        {
            if (agent.pathPending)
            {
                return;
            }

            navigationTimeout += Time.deltaTime;
            if (navigationTimeout > MAX_NAVIGATION_TIME)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Navigation", $"Timeout going to nest. Pos:{transform.position} Dest:{agent.destination} Dist:{agent.remainingDistance:F2}m", HappyChickens.Debug.EventSeverity.Warning);
                
                ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                if (producer != null)
                {
                    producer.CancelEggProduction();
                }
                
                navigationTimeout = 0f;
                ChangeState(ChickenState.Idle);
                return;
            }

            float arrivalDistance = 0.8f;
            bool hasArrived = !agent.pathPending && agent.remainingDistance <= arrivalDistance;

            if (hasArrived)
            {
                if (hasIntermediateWaypoint)
                {
                    hasIntermediateWaypoint = false;
                    agent.SetDestination(finalDestination);
                    return;
                }

                navigationTimeout = 0f;
                ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                if (producer != null)
                {
                    GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("EggProduction", $"Arrived at nest. Distance: {agent.remainingDistance:F2}m", HappyChickens.Debug.EventSeverity.Info);
                    producer.OnReachedNest();
                }
            }
        }

        private void HandleGoingToEatState()
        {
            if (agent.pathPending)
            {
                return;
            }

            navigationTimeout += Time.deltaTime;
            if (navigationTimeout > MAX_NAVIGATION_TIME)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Navigation", $"Timeout going to eat. Pos:{transform.position} Dest:{agent.destination} Dist:{agent.remainingDistance:F2}m", HappyChickens.Debug.EventSeverity.Warning);
                Debug.LogWarning($"[{ChickenName}] TIMEOUT GOING TO EAT! Distance left: {agent.remainingDistance:F2}m");
                navigationTimeout = 0f;
                ChangeState(ChickenState.Idle);
                return;
            }

            bool hasArrived = !agent.pathPending && agent.remainingDistance <= 0.8f;

            if (hasArrived)
            {
                if (hasIntermediateWaypoint)
                {
                    hasIntermediateWaypoint = false;
                    agent.SetDestination(finalDestination);
                    return;
                }

                navigationTimeout = 0f;
                agent.isStopped = true;
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Eating", $"Arrived at feeder. Starting to eat", HappyChickens.Debug.EventSeverity.Info);
                Debug.Log($"[{ChickenName}] EATING - Agent stopped, timer set");
                ChangeState(ChickenState.Eating);
                
                if (personality != null)
                {
                    stateTimer = Random.Range(personality.minEatingDuration, personality.maxEatingDuration);
                }
                else
                {
                    stateTimer = Random.Range(4f, 8f);
                }
                
                Debug.Log($"[{ChickenName}] EATING duration: {stateTimer:F1}s");
            }
        }

        private void HandleGoingToDrinkState()
        {
            if (agent.pathPending)
            {
                return;
            }

            navigationTimeout += Time.deltaTime;
            if (navigationTimeout > MAX_NAVIGATION_TIME)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Navigation", $"Timeout going to drink. Pos:{transform.position} Dest:{agent.destination} Dist:{agent.remainingDistance:F2}m", HappyChickens.Debug.EventSeverity.Warning);
                navigationTimeout = 0f;
                ChangeState(ChickenState.Idle);
                return;
            }

            bool hasArrived = !agent.pathPending && agent.remainingDistance <= 0.8f;

            if (hasArrived)
            {
                if (hasIntermediateWaypoint)
                {
                    hasIntermediateWaypoint = false;
                    agent.SetDestination(finalDestination);
                    return;
                }

                navigationTimeout = 0f;
                agent.isStopped = true;
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Drinking", $"Arrived at water trough. Starting to drink", HappyChickens.Debug.EventSeverity.Info);
                Debug.Log($"[{ChickenName}] DRINKING - Agent stopped, timer set");
                ChangeState(ChickenState.Drinking);
                
                if (personality != null)
                {
                    stateTimer = Random.Range(personality.minDrinkingDuration, personality.maxDrinkingDuration);
                }
                else
                {
                    stateTimer = Random.Range(2f, 4f);
                }
                
                Debug.Log($"[{ChickenName}] DRINKING duration: {stateTimer:F1}s");
            }
        }

        private void HandleGoingToSleepState()
        {
            UpdateRandomPause();
            UpdatePathDeviation();

            if (agent.pathPending)
            {
                return;
            }

            navigationTimeout += Time.deltaTime;
            if (navigationTimeout > MAX_NAVIGATION_TIME)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Navigation", $"Timeout going to sleep. Pos:{transform.position} Dest:{agent.destination} Dist:{agent.remainingDistance:F2}m", HappyChickens.Debug.EventSeverity.Warning);
                navigationTimeout = 0f;
                
                // CRITICAL: Don't sleep if needs are critical
                if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
                {
                    GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Sleep", $"Blocked timeout-sleep due to critical needs. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                    ChangeState(ChickenState.Idle);
                    stateTimer = 2f;
                    return;
                }
                
                IsSleepingOutside = true;
                ChangeState(ChickenState.Sleeping);
                agent.isStopped = true;
                return;
            }

            float arrivalDistance = 0.8f;
            bool hasArrived = !agent.pathPending && agent.remainingDistance <= arrivalDistance;

            if (hasArrived)
            {
                if (hasIntermediateWaypoint)
                {
                    hasIntermediateWaypoint = false;
                    agent.SetDestination(finalDestination);
                    return;
                }

                navigationTimeout = 0f;
                
                // CRITICAL: Check needs one last time before sleeping
                if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
                {
                    GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Sleep", $"Cancelled sleep at coop due to critical needs. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                    ChangeState(ChickenState.Idle);
                    stateTimer = 1f;
                    return;
                }
                
                ChangeState(ChickenState.Sleeping);
                agent.isStopped = true;
            }
        }

        private void UpdateRandomPause()
        {
            return;
            
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

                float lerpFactor = personality != null ? (1f - personality.speedChangeAbruptness) * 0.5f : 0.3f;
                currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetMultiplier, lerpFactor);
                agent.speed = baseSpeed * currentSpeedMultiplier;

                speedChangeTimer = Random.Range(0.5f, 2f);
            }
        }

        private void UpdatePathDeviation()
        {
            if (personality == null || personality.pathDeviationFrequency <= 0f || isPaused)
            {
                return;
            }

            if (CurrentState == ChickenState.GoingToEat || 
                CurrentState == ChickenState.GoingToDrink || 
                CurrentState == ChickenState.GoingToNest)
            {
                return;
            }

            pathDeviationTimer -= Time.deltaTime;

            if (pathDeviationTimer <= 0f)
            {
                if (Random.value < personality.pathDeviationFrequency)
                {
                    Vector3 lateralOffset = Quaternion.Euler(0, Random.Range(-90f, 90f), 0) * transform.forward;
                    lateralOffset *= Random.Range(0.5f, personality.maxPathDeviation);
                    lateralOffset.y = 0f;

                    Vector3 deviatedPosition = transform.position + lateralOffset;

                    if (NavMesh.SamplePosition(deviatedPosition, out NavMeshHit hit, personality.maxPathDeviation + 1f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                }

                pathDeviationTimer = Random.Range(1f, 3f);
            }
        }

        private bool ShouldHandleNeeds()
        {
            bool isGoingToNest = CurrentState == ChickenState.GoingToNest;
            
            if (!isGoingToNest && 
                (CurrentState == ChickenState.GoingToEat || 
                CurrentState == ChickenState.GoingToDrink || 
                CurrentState == ChickenState.GoingToSleep))
            {
                return false;
            }

            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                if (Needs.Hunger >= 80f && ShouldEat()) return true;
                if (Needs.Thirst >= 80f && ShouldDrink()) return true;
            }

            if (ShouldSleep()) return true;
            if (ShouldNap()) return true;
            
            if (isGoingToNest)
            {
                if (Needs.Hunger >= 90f || Needs.Thirst >= 90f)
                {
                    ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                    if (producer != null)
                    {
                        GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("EggProduction", $"Critical needs detected. Cancelling egg laying. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                        producer.CancelEggProduction();
                    }
                    
                    if (Needs.Hunger >= 90f && ShouldEat()) return true;
                    if (Needs.Thirst >= 90f && ShouldDrink()) return true;
                }
                return false;
            }
            
            if (ShouldEat()) return true;
            if (ShouldDrink()) return true;

            return false;
        }

        private bool ShouldSleep()
        {
            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Sleep", $"Skipping sleep due to critical needs. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Info);
                return false;
            }
            
            if (TimeController.Instance != null && TimeController.Instance.IsNightTime())
            {
                GoToSleep();
                return true;
            }
            return false;
        }

        private bool ShouldNap()
        {
            if (TimeController.Instance != null && !TimeController.Instance.IsNightTime())
            {
                float napThreshold = personality != null 
                    ? Random.Range(personality.sleepEnergyThresholdMin, personality.sleepEnergyThresholdMax)
                    : 50f;
                
                if (Needs.Energy < napThreshold)
                {
                    TryNap();
                    return true;
                }
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
                finalDestination = targetPosition;

                if (personality != null && personality.useIndirectPaths && Random.value < 0.6f)
                {
                    int waypointCount = Random.Range(0, personality.maxDetourWaypoints + 1);
                    if (waypointCount > 0)
                    {
                        Vector3 midPoint = Vector3.Lerp(transform.position, targetPosition, 0.5f);
                        Vector3 lateralOffset = Quaternion.Euler(0, Random.Range(-90f, 90f), 0) * (targetPosition - transform.position).normalized;
                        lateralOffset *= Random.Range(2f, 5f);
                        lateralOffset.y = 0f;

                        Vector3 waypointPos = midPoint + lateralOffset;

                        if (NavMesh.SamplePosition(waypointPos, out NavMeshHit waypointHit, 10f, NavMesh.AllAreas))
                        {
                            intermediateWaypoint = waypointHit.position;
                            hasIntermediateWaypoint = true;
                            agent.SetDestination(intermediateWaypoint);
                            ChangeState(ChickenState.Walking);
                            return;
                        }
                    }
                }

                hasIntermediateWaypoint = false;
                agent.SetDestination(targetPosition);
                Debug.Log($"[{ChickenName}] WANDERING - Distance:{Vector3.Distance(transform.position, targetPosition):F1}m");
                ChangeState(ChickenState.Walking);
            }
        }

        public void TryEat()
        {
            Structures.Feeder feeder = FindAvailableFeeder();
            if (feeder != null)
            {
                float distance = Vector3.Distance(transform.position, feeder.transform.position);
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Eating", $"Going to feeder. Hunger: {Needs.Hunger:F0}%", HappyChickens.Debug.EventSeverity.Info);
                Debug.Log($"[{ChickenName}] GOING TO EAT - Hunger:{Needs.Hunger:F0}% Distance:{distance:F1}m");
                SetDestinationWithDetour(feeder.GetFeedingPosition());
                ChangeState(ChickenState.GoingToEat);
            }
            else
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Eating", $"No available feeder found. Hunger: {Needs.Hunger:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                Debug.Log($"[{ChickenName}] NO FEEDER AVAILABLE - Hunger:{Needs.Hunger:F0}%");
                stateTimer = Random.Range(5f, 10f);
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
                if (feeder != null && !feeder.IsEmpty)
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
                float distance = Vector3.Distance(transform.position, waterTrough.transform.position);
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Drinking", $"Going to water trough. Thirst: {Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Info);
                Debug.Log($"[{ChickenName}] GOING TO DRINK - Thirst:{Needs.Thirst:F0}% Distance:{distance:F1}m");
                SetDestinationWithDetour(waterTrough.GetDrinkingPosition());
                ChangeState(ChickenState.GoingToDrink);
            }
            else
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Drinking", $"No available water trough found. Thirst: {Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                Debug.Log($"[{ChickenName}] NO WATER TROUGH AVAILABLE - Thirst:{Needs.Thirst:F0}%");
                stateTimer = Random.Range(5f, 10f);
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
                if (trough != null && !trough.IsEmpty)
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
            // CRITICAL: Don't sleep if hunger/thirst is critical
            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Sleep", $"Blocked sleep attempt due to critical needs. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                
                // Force handle needs instead
                if (Needs.Hunger >= 80f && ShouldEat()) return;
                if (Needs.Thirst >= 80f && ShouldDrink()) return;
                
                // If can't find food/water, at least go idle instead of sleeping
                ChangeState(ChickenState.Idle);
                stateTimer = 2f;
                return;
            }

            Structures.Coop coop = FindAvailableCoop();
            if (coop != null)
            {
                Transform entryPoint = coop.GetEntryPoint();
                SetDestinationWithDetour(entryPoint.position);
                ChangeState(ChickenState.GoingToSleep);
                IsSleepingOutside = false;
                return;
            }

            IsSleepingOutside = true;
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
            // CRITICAL: Don't nap if hunger/thirst is critical
            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("Sleep", $"Blocked nap attempt due to critical needs. H:{Needs.Hunger:F0}% T:{Needs.Thirst:F0}%", HappyChickens.Debug.EventSeverity.Warning);
                ChangeState(ChickenState.Idle);
                stateTimer = 2f;
                return;
            }
            
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

            if (!IsValidTransition(CurrentState, newState))
            {
                GetComponent<HappyChickens.Debug.ChickenDebugger>()?.LogEvent("FSM", $"Invalid transition: {CurrentState} → {newState}", HappyChickens.Debug.EventSeverity.Warning);
                return;
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
                pathDeviationTimer = Random.Range(1f, 2f);
                navigationTimeout = 0f;
            }
        }

        private bool IsValidTransition(ChickenState from, ChickenState to)
        {
            if (from == ChickenState.LayingEgg && to != ChickenState.Idle)
            {
                return false;
            }

            if ((from == ChickenState.Eating || from == ChickenState.Drinking) && 
                (to == ChickenState.Sleeping || to == ChickenState.GoingToSleep))
            {
                return false;
            }

            return true;
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
                if (CurrentState == ChickenState.GoingToEat || 
                    CurrentState == ChickenState.Eating ||
                    CurrentState == ChickenState.GoingToDrink ||
                    CurrentState == ChickenState.Drinking ||
                    CurrentState == ChickenState.LayingEgg)
                {
                    return;
                }
                
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

        private void SetDestinationWithDetour(Vector3 destination)
        {
            finalDestination = destination;

            if (personality != null && personality.useIndirectPaths && Random.value < 0.4f)
            {
                float distance = Vector3.Distance(transform.position, destination);
                
                if (distance > 5f)
                {
                    Vector3 midPoint = Vector3.Lerp(transform.position, destination, Random.Range(0.3f, 0.7f));
                    Vector3 directionToTarget = (destination - transform.position).normalized;
                    Vector3 lateralOffset = Quaternion.Euler(0, Random.Range(-60f, 60f), 0) * directionToTarget;
                    lateralOffset *= Random.Range(2f, 4f);
                    lateralOffset.y = 0f;

                    Vector3 waypointPos = midPoint + lateralOffset;

                    if (NavMesh.SamplePosition(waypointPos, out NavMeshHit waypointHit, 8f, NavMesh.AllAreas))
                    {
                        intermediateWaypoint = waypointHit.position;
                        hasIntermediateWaypoint = true;
                        agent.SetDestination(intermediateWaypoint);
                        return;
                    }
                }
            }

            hasIntermediateWaypoint = false;
            agent.SetDestination(destination);
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
