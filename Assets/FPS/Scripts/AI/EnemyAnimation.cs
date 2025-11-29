
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyBrain), typeof(Health))]
    public class EnemyAnimation : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Animator component for the enemy")]
        [SerializeField] private Animator animator;

        // Animator parameters
        private static readonly int k_AnimMoveSpeedParameter = Animator.StringToHash("MoveSpeed");
        private static readonly int k_AnimAttackParameter = Animator.StringToHash("Attack");
        private static readonly int k_AnimAlertedParameter = Animator.StringToHash("Alerted");
        private static readonly int k_AnimOnDamagedParameter = Animator.StringToHash("OnDamaged");

        private EnemyBrain m_EnemyBrain;
        private Health m_Health;

        void Awake()
        {
            m_EnemyBrain = GetComponent<EnemyBrain>();
            m_Health = GetComponent<Health>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        void OnEnable()
        {
            m_EnemyBrain.OnAttack += OnAttack;
            m_EnemyBrain.OnDetectedTarget += OnDetectedTarget;
            m_EnemyBrain.OnLostTarget += OnLostTarget;
            m_Health.OnDamaged += OnDamaged;
        }

        void OnDisable()
        {
            m_EnemyBrain.OnAttack -= OnAttack;
            m_EnemyBrain.OnDetectedTarget -= OnDetectedTarget;
            m_EnemyBrain.OnLostTarget -= OnLostTarget;
            m_Health.OnDamaged -= OnDamaged;
        }

        void Update()
        {
            if (animator != null && m_EnemyBrain.NavMeshAgent != null)
            {
                float moveSpeed = m_EnemyBrain.NavMeshAgent.velocity.magnitude;
                animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);
            }
        }

        private void OnAttack()
        {
            if (animator != null) animator.SetTrigger(k_AnimAttackParameter);
        }

        private void OnDetectedTarget()
        {
            if (animator != null) animator.SetBool(k_AnimAlertedParameter, true);
        }

        private void OnLostTarget()
        {
            if (animator != null) animator.SetBool(k_AnimAlertedParameter, false);
        }

        private void OnDamaged(float damage, GameObject source)
        {
            if (animator != null) animator.SetTrigger(k_AnimOnDamagedParameter);
        }
    }
}

/* 
# METADATA
ScriptRole: Manages enemy animations by listening to events from other components and updating the Animator.
RelatedScripts: EnemyBrain, Health.
UsesSO: None.
ReceivesFrom: EnemyBrain (OnAttack, OnDetectedTarget, OnLostTarget), Health (OnDamaged).
SendsTo: None.
Setup:
- Attach to the root of the enemy GameObject.
- Assign the 'Animator' component in the Inspector.
- Requires EnemyBrain and Health components.
*/
