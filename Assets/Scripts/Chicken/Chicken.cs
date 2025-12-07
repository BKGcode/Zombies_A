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
    [RequireComponent(typeof(ChickenMovement))]
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

        private ChickenMovement movement;
        private NavMeshAgent agent;
        private float stateTimer;
        
        private Coroutine activeStateRoutine;
        private ConsumableStructure currentStructure;

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

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            movement = GetComponent<ChickenMovement>();
            Needs = new ChickenNeeds();
            Happiness = new ChickenHappiness();

            if (personality != null)
            {
                agent.speed *= personality.walkSpeedMultiplier;
                agent.avoidancePriority = personality.avoidancePriority;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.angularSpeed = personality.rotationSpeed;
            }
            else
            {
                agent.angularSpeed = 540f;
            }
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
            if (TimeController.Instance != null)
            {
                TimeController.Instance.OnTimeOfDayChanged.RemoveListener(OnTimeOfDayChanged);
            }
        }

        private void Update()
        {
            UpdateNeeds();
            Happiness.UpdateHappiness(Time.deltaTime, 0.1f);
            UpdateStateMachine();
        }

        private void UpdateNeeds()
        {
            if (CurrentState != ChickenState.Eating && CurrentState != ChickenState.Drinking)
            {
                Needs.IncreaseHunger(Time.deltaTime * (gameBalance != null ? gameBalance.hungerIncreaseRate : 1f));
                Needs.IncreaseThirst(Time.deltaTime * (gameBalance != null ? gameBalance.thirstIncreaseRate : 1.5f));
            }

            if (CurrentState != ChickenState.Sleeping)
            {
                float energyRate = IsSleepingOutside ? 2f : 1f;
                Needs.DecreaseEnergy(Time.deltaTime * energyRate);
            }
        }

        private void UpdateStateMachine()
        {
            if (stateTimer > 0f)
            {
                stateTimer -= Time.deltaTime;
            }

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

                case ChickenState.GoingToEat:
                    HandleGoingToEatState();
                    break;

                case ChickenState.GoingToDrink:
                    HandleGoingToDrinkState();
                    break;

                case ChickenState.GoingToSleep:
                    HandleGoingToSleepState();
                    break;

                case ChickenState.GoingToNest:
                    HandleGoingToNestState();
                    break;

                case ChickenState.LayingEgg:
                    HandleLayingEggState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            if (stateTimer <= 0f)
            {
                if (CheckNeeds())
                {
                    return;
                }
            }

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
                if (Random.value < 0.7f)
                {
                    StartWandering();
                }
                else
                {
                    float idleTime = personality != null 
                        ? Random.Range(personality.minIdleTime, personality.maxIdleTime) * personality.idleTimeMultiplier
                        : Random.Range(1f, 4f);
                    stateTimer = idleTime;
                }
            }
        }

        private void HandleWalkingState()
        {
            if (movement.IsStuck())
            {
                movement.StopMoving();
                ChangeState(ChickenState.Idle);
                stateTimer = 1f;
                return;
            }

            if (movement.HasArrived)
            {
                float waitTime = personality != null
                    ? Random.Range(personality.minWaitAfterWalk, personality.maxWaitAfterWalk) * personality.idleTimeMultiplier
                    : Random.Range(2f, 6f);
                
                stateTimer = waitTime;
                ChangeState(ChickenState.Idle);
            }
        }

        private void HandleEatingState()
        {
            if (activeStateRoutine == null)
            {
                activeStateRoutine = StartCoroutine(EatingRoutine());
            }
        }
        
        private System.Collections.IEnumerator EatingRoutine()
        {
            if (currentStructure == null)
            {
                ChangeState(ChickenState.Idle);
                yield break;
            }
            
            float consumeInterval = 1f;
            float feedAmount = 10f;
            float consumeAmount = gameBalance != null ? gameBalance.consumptionPerUse : 5f;
            
            while (!Needs.IsHungry() && currentStructure != null && !currentStructure.IsEmpty)
            {
                yield return new WaitForSeconds(consumeInterval);
                
                if (currentStructure.TryConsume(consumeAmount))
                {
                    Needs.Feed(feedAmount);
                }
                else
                {
                    break;
                }
            }
            
            ChangeState(ChickenState.Idle);
            stateTimer = 1f;
        }

        private void HandleDrinkingState()
        {
            if (activeStateRoutine == null)
            {
                activeStateRoutine = StartCoroutine(DrinkingRoutine());
            }
        }
        
        private System.Collections.IEnumerator DrinkingRoutine()
        {
            if (currentStructure == null)
            {
                ChangeState(ChickenState.Idle);
                yield break;
            }
            
            float consumeInterval = 1f;
            float waterAmount = 15f;
            float consumeAmount = gameBalance != null ? gameBalance.consumptionPerUse : 5f;
            
            while (!Needs.IsThirsty() && currentStructure != null && !currentStructure.IsEmpty)
            {
                yield return new WaitForSeconds(consumeInterval);
                
                if (currentStructure.TryConsume(consumeAmount))
                {
                    Needs.GiveWater(waterAmount);
                }
                else
                {
                    break;
                }
            }
            
            ChangeState(ChickenState.Idle);
            stateTimer = 1f;
        }

        private void HandleSleepingState()
        {
            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                ShowVisual();
                ChangeState(ChickenState.Idle);
                stateTimer = 0.5f;
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

        private void HandleGoingToEatState()
        {
            if (movement.IsStuck())
            {
                movement.StopMoving();
                ChangeState(ChickenState.Idle);
                return;
            }
        }

        private void HandleGoingToDrinkState()
        {
            if (movement.IsStuck())
            {
                WaterTrough target = ChickenStructureCache.FindClosestAvailableWaterTrough(transform.position);
                if (target != null)
                {
                    float distance = Vector3.Distance(transform.position, target.GetDrinkingPosition());
                    Debug.LogWarning($"[{chickenName}] STUCK going to water trough. Distance: {distance:F2}m. NavMesh path valid: {movement.IsMoving}");
                }
                
                movement.StopMoving();
                ChangeState(ChickenState.Idle);
                return;
            }
        }

        private void HandleGoingToSleepState()
        {
            if (movement.IsStuck())
            {
                movement.StopMoving();
                IsSleepingOutside = true;
                ChangeState(ChickenState.Sleeping);
                return;
            }

            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                movement.StopMoving();
                ChangeState(ChickenState.Idle);
                stateTimer = 1f;
                return;
            }

            if (movement.HasArrived)
            {
                movement.StopMoving();
                IsSleepingOutside = false;
                ChangeState(ChickenState.Sleeping);
            }
        }

        private void HandleGoingToNestState()
        {
            if (movement.IsStuck())
            {
                movement.StopMoving();
                ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                if (producer != null)
                {
                    producer.CancelEggProduction();
                }
                ChangeState(ChickenState.Idle);
                return;
            }

            if (Needs.Hunger >= 90f || Needs.Thirst >= 90f)
            {
                movement.StopMoving();
                ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                if (producer != null)
                {
                    producer.CancelEggProduction();
                }
                ChangeState(ChickenState.Idle);
                return;
            }

            if (movement.HasArrived)
            {
                movement.StopMoving();
                ChangeState(ChickenState.LayingEgg);
                
                float duration = personality != null
                    ? Random.Range(personality.minLayingEggDuration, personality.maxLayingEggDuration)
                    : Random.Range(4f, 7f);
                stateTimer = duration;
            }
        }

        private void HandleLayingEggState()
        {
            if (stateTimer <= 0f)
            {
                OnEggLaid?.Invoke();
                ChangeState(ChickenState.Idle);
                stateTimer = 1f;
            }
        }

        private bool CheckNeeds()
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
                if (Needs.Hunger >= 80f && TryEat()) return true;
                if (Needs.Thirst >= 80f && TryDrink()) return true;
            }

            if (CheckSleep()) return true;
            
            if (isGoingToNest)
            {
                if (Needs.Hunger >= 90f || Needs.Thirst >= 90f)
                {
                    ChickenEggProducer producer = GetComponent<ChickenEggProducer>();
                    if (producer != null)
                    {
                        producer.CancelEggProduction();
                    }
                    
                    if (Needs.Hunger >= 90f && TryEat()) return true;
                    if (Needs.Thirst >= 90f && TryDrink()) return true;
                    
                    movement.StopMoving();
                    ChangeState(ChickenState.Idle);
                    stateTimer = 2f;
                    return true;
                }
                return false;
            }
            
            if (Needs.IsHungry() && TryEat()) return true;
            if (Needs.IsThirsty() && TryDrink()) return true;

            return false;
        }

        private bool CheckSleep()
        {
            if (Needs.Hunger >= 80f || Needs.Thirst >= 80f)
            {
                return false;
            }
            
            if (TimeController.Instance != null && TimeController.Instance.IsNightTime())
            {
                GoToSleep();
                return true;
            }

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

        private void StartWandering()
        {
            float radius = personality != null ? personality.wanderRadius : 10f;
            
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                if (movement.GoTo(hit.position))
                {
                    ChangeState(ChickenState.Walking);
                }
            }
        }

        public bool TryEat()
        {
            Feeder feeder = ChickenStructureCache.FindClosestAvailableFeeder(transform.position);
            if (feeder != null && movement.GoTo(feeder.GetFeedingPosition()))
            {
                ChangeState(ChickenState.GoingToEat);
                return true;
            }

            stateTimer = Random.Range(5f, 10f);
            return false;
        }

        public bool TryDrink()
        {
            WaterTrough waterTrough = ChickenStructureCache.FindClosestAvailableWaterTrough(transform.position);
            if (waterTrough != null && movement.GoTo(waterTrough.GetDrinkingPosition()))
            {
                ChangeState(ChickenState.GoingToDrink);
                return true;
            }

            stateTimer = Random.Range(5f, 10f);
            return false;
        }

        public void GoToSleep()
        {
            Coop coop = ChickenStructureCache.FindClosestAvailableCoop(transform.position);
            if (coop != null && movement.GoTo(coop.GetEntryPoint().position))
            {
                ChangeState(ChickenState.GoingToSleep);
                IsSleepingOutside = false;
                return;
            }

            IsSleepingOutside = true;
            ChangeState(ChickenState.Sleeping);
        }

        public void TryNap()
        {
            GoToSleep();
        }

        public void StartLayingEgg()
        {
            ChangeState(ChickenState.GoingToNest);
        }

        public void ChangeState(ChickenState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }
            
            if (activeStateRoutine != null)
            {
                StopCoroutine(activeStateRoutine);
                activeStateRoutine = null;
            }
            
            if (currentStructure != null)
            {
                currentStructure.StopUsing();
                currentStructure = null;
            }

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void SetStateTimer(float duration)
        {
            stateTimer = duration;
        }

        public void SetCurrentStructure(ConsumableStructure structure)
        {
            currentStructure = structure;
        }

        private void OnTimeOfDayChanged(TimeOfDay timeOfDay)
        {
            if (timeOfDay == TimeOfDay.Night && CurrentState == ChickenState.Idle)
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
                agent.avoidancePriority = personality.avoidancePriority;
                agent.angularSpeed = personality.rotationSpeed;
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

        private void OnDrawGizmosSelected()
        {
            float radius = personality != null ? personality.wanderRadius : 10f;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);

            if (Application.isPlaying && movement != null && movement.IsMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, movement.CurrentDestination);
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
