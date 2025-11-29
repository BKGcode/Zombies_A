using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.FPS.Game;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(Health))]
[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DetectionModule))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Configuración (SO)")]
        [Tooltip("Define los atributos y comportamiento base de este enemigo.")]
        public EnemyTypeSO EnemyType;

        [Header("Patrulla (Ruta)")]
        [Tooltip("La ruta que seguirá este enemigo. Si es nulo, se quedará quieto.")]
        public PatrolPath PatrolPath;

        [Header("Componentes")]
        [Tooltip("Animator para controlar las animaciones del enemigo.")]
        public Animator Animator;

        [Tooltip("AudioSource para los sonidos de movimiento.")]
        public AudioSource MovementAudio;

        // --- ESTADO INTERNO ---
        public enum AIState { Patrol, Follow, Attack }
        public AIState CurrentState { get; private set; }

        // --- REFERENCIAS PRIVADAS ---
        private Health _health;
        private Actor _actor;
        private NavMeshAgent _navMeshAgent;
        private DetectionModule _detectionModule;
        private Collider[] _selfColliders;

        // --- VARIABLES DE COMPORTAMIENTO ---
        private int _patrolNodeIndex;
        private bool _isWaitingAtNode;
        private float _timeLastSeenTarget;

        // --- CONSTANTES DE ANIMACIÓN ---
        private const string k_AnimMoveSpeed = "MoveSpeed";
        private const string k_AnimAttack = "Attack";
        private const string k_AnimAlerted = "Alerted";
        private const string k_AnimOnDamaged = "OnDamaged";

        void Awake()
        {
            // --- RECOLECCIÓN DE COMPONENTES ---
            _health = GetComponent<Health>();
            _actor = GetComponent<Actor>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _detectionModule = GetComponent<DetectionModule>();
            _selfColliders = GetComponentsInChildren<Collider>();
        }

        void Start()
        {
            // --- CONFIGURACIÓN DESDE SO ---
            if (EnemyType != null)
            {
                _health.MaxHealth = EnemyType.MaxHealth;
                _health.CurrentHealth = EnemyType.MaxHealth;
                _navMeshAgent.speed = EnemyType.MoveSpeed;
                _detectionModule.AttackRange = EnemyType.AttackRange;
            }

            // --- SUSCRIPCIÓN A EVENTOS ---
            _health.OnDie += OnDie;
            _health.OnDamaged += OnDamaged;
            _detectionModule.onDetectedTarget += OnDetectedTarget;
            _detectionModule.onLostTarget += OnLostTarget;

            // --- INICIALIZACIÓN DE ESTADO ---
            if (PatrolPath != null && PatrolPath.PathNodes.Count > 0)
            {
                _patrolNodeIndex = 0; // Empezar en el primer nodo
                _navMeshAgent.SetDestination(PatrolPath.PathNodes[_patrolNodeIndex].position);
            }

            CurrentState = AIState.Patrol;
        }

        void Update()
        {
            // 1. Detección
            _detectionModule.HandleTargetDetection(_actor, _selfColliders, EnemyType.VisionRange, EnemyType.VisionAngle);

            // 2. Transiciones de Estado
            UpdateStateTransitions();

            // 3. Lógica del Estado Actual
            UpdateCurrentState();

            // 4. Animaciones y Sonido
            UpdateFeedback();
        }

        void UpdateStateTransitions()
        {
            switch (CurrentState)
            {
                case AIState.Patrol:
                    if (_detectionModule.IsSeeingTarget)
                    {
                        CurrentState = AIState.Follow;
                    }
                    break;

                case AIState.Follow:
                    if (!_detectionModule.KnownDetectedTarget)
                    {
                        CurrentState = AIState.Patrol;
                    }
                    else if (_detectionModule.IsSeeingTarget && _detectionModule.IsTargetInAttackRange)
                    {
                        CurrentState = AIState.Attack;
                    }
                    break;

                case AIState.Attack:
                    if (!_detectionModule.KnownDetectedTarget)
                    {
                        CurrentState = AIState.Patrol;
                    }
                    else if (!_detectionModule.IsTargetInAttackRange)
                    {
                        CurrentState = AIState.Follow;
                    }
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

        void HandlePatrolState()
        {
            if (PatrolPath == null || _isWaitingAtNode) return;

            // Si hemos llegado al destino
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                StartCoroutine(PatrolWaitCoroutine());
            }
        }

        IEnumerator PatrolWaitCoroutine()
        {
            _isWaitingAtNode = true;

            float waitTime = Random.Range(EnemyType.WaitTimeMin, EnemyType.WaitTimeMax);
            yield return new WaitForSeconds(waitTime);

            // Avanzar al siguiente nodo
            _patrolNodeIndex = (_patrolNodeIndex + 1) % PatrolPath.PathNodes.Count;
            Vector3 nextDestination = PatrolPath.PathNodes[_patrolNodeIndex].position;
            _navMeshAgent.SetDestination(nextDestination);

            _isWaitingAtNode = false;
        }

        void HandleFollowState()
        {
            if (_detectionModule.KnownDetectedTarget)
            {
                _navMeshAgent.SetDestination(_detectionModule.KnownDetectedTarget.transform.position);
            }
        }

        void HandleAttackState()
        {
            if (_detectionModule.KnownDetectedTarget)
            {
                _navMeshAgent.SetDestination(transform.position); // Detenerse para atacar
                transform.LookAt(_detectionModule.KnownDetectedTarget.transform.position);
                // Lógica de ataque (ej. disparar) iría aquí
            }
        }

        void UpdateFeedback()
        {
            Animator.SetFloat(k_AnimMoveSpeed, _navMeshAgent.velocity.magnitude / _navMeshAgent.speed);
        }

        // --- MANEJADORES DE EVENTOS ---
        void OnDetectedTarget()
        {
            Animator.SetBool(k_AnimAlerted, true);
        }

        void OnLostTarget()
        {
            Animator.SetBool(k_AnimAlerted, false);
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            // Reaccionar al daño
            if (damageSource && damageSource.GetComponent<Actor>() != null)
            {
                _detectionModule.OnDamaged(damageSource);
                CurrentState = AIState.Follow;
            }
            Animator.SetTrigger(k_AnimOnDamaged);
        }

        void OnDie()
        {
            // Lógica de muerte (efectos, loot, etc.)
            Destroy(gameObject, 2f); // Destruir el objeto después de 2 segundos
        }
    }
}
