using System;
using UnityEngine;
using UnityEngine.AI;

namespace HappyChickens.Debug
{
    public class ChickenDebugger : MonoBehaviour
    {
        public string ChickenID { get; private set; }
        public string CurrentState { get; private set; }
        public float TimeInCurrentState { get; private set; }
        public string PreviousState { get; private set; }
        public float LastTransitionTime { get; private set; }
        
        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        public float Tiredness { get; private set; }
        public float Energy { get; private set; }
        
        public Vector3 CurrentPosition { get; private set; }
        public Vector3 NavMeshDestination { get; private set; }
        public bool HasNavMeshPath { get; private set; }
        public bool IsMoving { get; private set; }
        
        public string AssignedFeeder { get; private set; }
        public string AssignedWaterTrough { get; private set; }
        
        public bool HasCriticalNeed { get; private set; }
        public bool IsStuck { get; private set; }

        private GallinasFelices.Chicken.Chicken chickenComponent;
        private NavMeshAgent navAgent;
        private float stateStartTime;
        private Vector3 lastPosition;
        private float stuckCheckTimer;
        private const float StuckThreshold = 15f;

        private void Awake()
        {
            ChickenID = gameObject.name;
            chickenComponent = GetComponent<GallinasFelices.Chicken.Chicken>();
            navAgent = GetComponent<NavMeshAgent>();
            
            if (ChickenMonitorManager.Instance != null)
            {
                ChickenMonitorManager.Instance.RegisterChicken(this);
            }
        }

        private void OnDestroy()
        {
            if (ChickenMonitorManager.Instance != null)
            {
                ChickenMonitorManager.Instance.UnregisterChicken(this);
            }
        }

        private void Update()
        {
            UpdateStateTracking();
            UpdateNeedsTracking();
            UpdatePositionTracking();
            UpdateStuckDetection();
        }

        private void UpdateStateTracking()
        {
            if (chickenComponent == null) return;

            string newState = chickenComponent.CurrentState.ToString();
            
            if (newState != CurrentState)
            {
                PreviousState = CurrentState;
                CurrentState = newState;
                LastTransitionTime = Time.time;
                stateStartTime = Time.time;
                
                ChickenMonitorManager.Instance.LogTransition(ChickenID, PreviousState, CurrentState);
            }
            
            TimeInCurrentState = Time.time - stateStartTime;
        }

        private void UpdateNeedsTracking()
        {
            if (chickenComponent == null) return;

            Hunger = chickenComponent.Needs.Hunger;
            Thirst = chickenComponent.Needs.Thirst;
            Tiredness = 100f - chickenComponent.Needs.Energy;
            Energy = chickenComponent.Needs.Energy;
            
            HasCriticalNeed = Hunger > 80f || Thirst > 80f || Tiredness > 80f || Energy < 20f;
        }

        private void UpdatePositionTracking()
        {
            CurrentPosition = transform.position;
            
            if (navAgent != null)
            {
                HasNavMeshPath = navAgent.hasPath;
                NavMeshDestination = navAgent.destination;
                IsMoving = navAgent.velocity.magnitude > 0.1f;
            }
        }

        private void UpdateStuckDetection()
        {
            float distanceMoved = Vector3.Distance(CurrentPosition, lastPosition);
            
            if (distanceMoved < 0.01f && IsMoving)
            {
                stuckCheckTimer += Time.deltaTime;
                IsStuck = stuckCheckTimer > StuckThreshold;
            }
            else
            {
                stuckCheckTimer = 0f;
                IsStuck = false;
            }
            
            lastPosition = CurrentPosition;
        }

        public void SetAssignedFeeder(string feederName)
        {
            AssignedFeeder = feederName;
        }

        public void SetAssignedWaterTrough(string troughName)
        {
            AssignedWaterTrough = troughName;
        }
    }
}
