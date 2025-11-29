using UnityEngine;
using UnityEngine.AI;
using Unity.FPS.Game;
using System;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(DetectionModule))]
    public class EnemyBrain : MonoBehaviour
    {
        [Header("SO Configuration")]
        [Tooltip("Defines the base attributes and behavior of this enemy.")]
        public EnemyTypeSO enemyType;

        [Header("Pathing")]
        [Tooltip("The path this enemy will follow when patrolling. If null, it will stand still.")]
        public PatrolPath patrolPath;

        [Header("Parameters")]
        [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
        [SerializeField] private float selfDestructYHeight = -20f;

        [Tooltip("The speed at which the enemy rotates")]
        [SerializeField] private float orientationSpeed = 10f;

        public UnityAction OnAttack;
        public UnityAction OnDetectedTarget;
        public UnityAction OnLostTarget;
        public UnityAction<AIState> OnStateChanged;

        public enum AIState { Patrol, Follow, Attack }
        public AIState CurrentState { get; private set; }

        public NavMeshAgent NavMeshAgent { get; private set; }
        public DetectionModule DetectionModule { get; private set; }
        public WeaponController CurrentWeapon { get; private set; }
        
        private Health m_Health;
        private Actor m_Actor;
        private Damageable m_Damageable;
        private Collider[] m_SelfColliders;
        private WeaponController[] m_Weapons;
        private int m_PathDestinationNodeIndex;
        private bool m_IsWaitingAtNode;
        private float m_TimeStartedWaiting;
        private float m_WaitTime;

        void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Health = GetComponent<Health>();
            m_Actor = GetComponent<Actor>();
            m_Damageable = GetComponent<Damageable>();
            DetectionModule = GetComponent<DetectionModule>();
            m_Weapons = GetComponentsInChildren<WeaponController>();
            m_SelfColliders = GetComponentsInChildren<Collider>();
        }

        void Start()
        {
            if (enemyType == null)
            {
                Debug.LogError("EnemyType ScriptableObject not assigned! Disabling Brain.", this);
                enabled = false;
                return;
            }

            // Configure components from SO
            m_Health.MaxHealth = enemyType.MaxHealth;
            NavMeshAgent.speed = enemyType.MoveSpeed;
            NavMeshAgent.stoppingDistance = enemyType.AttackRange * 0.8f;
            DetectionModule.AttackRange = enemyType.AttackRange;

            foreach (var weapon in m_Weapons)
            {
                weapon.Owner = gameObject;
            }
            if (m_Weapons.Length > 0)
            {
                CurrentWeapon = m_Weapons[0];
            }

            m_Health.OnDie += HandleDeath;
            DetectionModule.onDetectedTarget += HandleDetectedTarget;
            DetectionModule.onLostTarget += HandleLostTarget;

            if (patrolPath != null && patrolPath.PathNodes.Count > 0)
            {
                m_PathDestinationNodeIndex = 0;
                SetNavDestination(patrolPath.PathNodes[m_PathDestinationNodeIndex].position);
            }
            
            ChangeState(AIState.Patrol);
        }

        void Update()
        {
            EnsureIsWithinLevelBounds();
            DetectionModule.HandleTargetDetection(m_Actor, m_SelfColliders, enemyType.VisionRange, enemyType.VisionAngle);
            UpdateStateTransitions();
            UpdateCurrentState();
        }

        void UpdateStateTransitions()
        {
            switch (CurrentState)
            {
                case AIState.Patrol:
                    if (DetectionModule.IsSeeingTarget) ChangeState(AIState.Follow);
                    break;

                case AIState.Follow:
                    if (DetectionModule.KnownDetectedTarget == null) ChangeState(AIState.Patrol);
                    else if (DetectionModule.IsSeeingTarget && DetectionModule.IsTargetInAttackRange) ChangeState(AIState.Attack);
                    break;

                case AIState.Attack:
                    if (DetectionModule.KnownDetectedTarget == null) ChangeState(AIState.Patrol);
                    else if (!DetectionModule.IsTargetInAttackRange) ChangeState(AIState.Follow);
                    break;
            }
        }

        void UpdateCurrentState()
        {
            switch (CurrentState)
            {
                case AIState.Patrol:
                    HandlePatrolState();
                    break;
                case AIState.Follow:
                    HandleFollowState();
                    break;
                case AIState.Attack:
                    HandleAttackState();
                    break;
            }
        }

        void ChangeState(AIState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            if (newState == AIState.Patrol && patrolPath != null && patrolPath.PathNodes.Count > 0)
            {
                SetPathDestinationToClosestNode();
                SetNavDestination(GetDestinationOnPath());
            }
        }

        void HandlePatrolState()
        {
            if (patrolPath == null || NavMeshAgent.pathPending || enemyType.MoveSpeed <= 0) return;

            if (!m_IsWaitingAtNode && NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
            {
                m_IsWaitingAtNode = true;
                m_TimeStartedWaiting = Time.time;
                m_WaitTime = UnityEngine.Random.Range(enemyType.WaitTimeMin, enemyType.WaitTimeMax);
            }

            if (m_IsWaitingAtNode && Time.time >= m_TimeStartedWaiting + m_WaitTime)
            {
                m_PathDestinationNodeIndex = (m_PathDestinationNodeIndex + 1) % patrolPath.PathNodes.Count;
                SetNavDestination(GetDestinationOnPath());
                m_IsWaitingAtNode = false;
            }
        }

        void HandleFollowState()
        {
            if (DetectionModule.KnownDetectedTarget != null && enemyType.MoveSpeed > 0)
            {
                SetNavDestination(DetectionModule.KnownDetectedTarget.transform.position);
                OrientTowards(DetectionModule.KnownDetectedTarget.transform.position);
            }
        }

        void HandleAttackState()
        {
            if (DetectionModule.KnownDetectedTarget != null)
            {
                if (NavMeshAgent.isOnNavMesh)
                {
                    NavMeshAgent.isStopped = true;
                    NavMeshAgent.velocity = Vector3.zero;
                }
                OrientTowards(DetectionModule.KnownDetectedTarget.transform.position);
                TryAttack(DetectionModule.KnownDetectedTarget.transform.position);
            }
        }

        void SetNavDestination(Vector3 destination)
        {
            if (NavMeshAgent.isOnNavMesh && enemyType.MoveSpeed > 0)
            {
                NavMeshAgent.isStopped = false;
                NavMeshAgent.SetDestination(destination);
            }
        }

        void OrientTowards(Vector3 lookPosition)
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude > 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * orientationSpeed);
            }
        }

        public void TryAttack(Vector3 enemyPosition)
        {
            if (CurrentWeapon == null) return;

            CurrentWeapon.transform.forward = (enemyPosition - CurrentWeapon.WeaponMuzzle.position).normalized;

            if (CurrentWeapon.HandleShootInputs(false, true, false))
            {
                OnAttack?.Invoke();
            }
        }

        void HandleDetectedTarget()
        {
            OnDetectedTarget?.Invoke();
            if(CurrentState == AIState.Patrol) ChangeState(AIState.Follow);
        }

        void HandleLostTarget()
        {
            OnLostTarget?.Invoke();
            ChangeState(AIState.Patrol);
        }

        void HandleDeath()
        {
            // Unsubscribe from all events
            m_Health.OnDie -= HandleDeath;
            DetectionModule.onDetectedTarget -= HandleDetectedTarget;
            DetectionModule.onLostTarget -= HandleLostTarget;

            // Disable all components on this GameObject
            foreach (var component in GetComponents<MonoBehaviour>())
            {
                component.enabled = false;
            }
            NavMeshAgent.enabled = false;
            GetComponent<Collider>().enabled = false;

            Destroy(gameObject, 5f); // Delay destruction to allow death effects
        }

        void EnsureIsWithinLevelBounds()
        {
            if (transform.position.y < selfDestructYHeight)
            {
                m_Health.Kill();
            }
        }

        void SetPathDestinationToClosestNode()
        {
            if (patrolPath == null || patrolPath.PathNodes.Count == 0) return;

            int closestPathNodeIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < patrolPath.PathNodes.Count; i++)
            {
                float distanceToNode = Vector3.Distance(transform.position, patrolPath.PathNodes[i].position);
                if (distanceToNode < minDistance)
                {
                    minDistance = distanceToNode;
                    closestPathNodeIndex = i;
                }
            }
            m_PathDestinationNodeIndex = closestPathNodeIndex;
        }

        Vector3 GetDestinationOnPath()
        {
            return (patrolPath != null && patrolPath.PathNodes.Count > 0)
                ? patrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex)
                : transform.position;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            NavMeshAgent.speed = enemyType.MoveSpeed * multiplier;
        }

        public void SetDamageMultiplier(float multiplier)
        {
            if(m_Damageable) m_Damageable.DamageMultiplier = multiplier;
        }

        public void SetDetectionRangeMultiplier(float multiplier)
        {
            // This is a bit of a hack, as DetectionModule doesn't have a range property.
            // We can't change the visionRange passed to HandleTargetDetection on the fly.
            // A better solution would be to refactor DetectionModule to have a DetectionRange property.
        }

        void OnDrawGizmosSelected()
        {
            if (enemyType == null) return;

            // Detection Range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyType.VisionRange);

            // Attack Range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyType.AttackRange);
        }
    }
}

/* 
# METADATA
ScriptRole: Core AI logic for an enemy, including state machine (Patrol, Follow, Attack) and navigation.
RelatedScripts: EnemyTypeSO, PatrolPath, Health, Actor, DetectionModule, WeaponController, EnemyVFX, EnemyAnimation, EnemyAudio, EnemyLoot.
UsesSO: EnemyTypeSO.
ReceivesFrom: Health (OnDie), DetectionModule (onDetectedTarget, onLostTarget).
SendsTo: EnemyVFX, EnemyAnimation, EnemyAudio (via C# events).
Setup:
- Attach to the root of the enemy GameObject.
- Assign the 'EnemyType' ScriptableObject.
- Optionally, assign a 'PatrolPath'.
- Requires NavMeshAgent, Health, Actor, and DetectionModule components on the same GameObject.
*/