
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyBrain), typeof(Health), typeof(AudioSource))]
    public class EnemyAudio : MonoBehaviour
    {
        [Header("Audio Clips")]
        [Tooltip("Sound played when receiving damage")]
        [SerializeField] private AudioClip damageSfx;

        [Tooltip("Sound played when detecting the target")]
        [SerializeField] private AudioClip detectionSfx;

        [Tooltip("Sound played continuously while moving")]
        [SerializeField] private AudioClip movementSfx;

        [Header("Movement Sound Parameters")]
        [Tooltip("Pitch range for the movement sound based on speed")]
        [SerializeField] private MinMaxFloat pitchDistortionMovementSpeed = new MinMaxFloat { Min = 1f, Max = 1.5f };

        private EnemyBrain m_EnemyBrain;
        private Health m_Health;
        private AudioSource m_AudioSource;
        private WeaponController m_WeaponController;

        void Awake()
        {
            m_EnemyBrain = GetComponent<EnemyBrain>();
            m_Health = GetComponent<Health>();
            m_AudioSource = GetComponent<AudioSource>();
            m_WeaponController = GetComponentInChildren<WeaponController>();
        }

        void OnEnable()
        {
            m_Health.OnDamaged += OnDamaged;
            m_EnemyBrain.OnDetectedTarget += OnDetectedTarget;
            if (m_WeaponController != null)
            {
                m_WeaponController.OnShoot += OnShoot;
            }
        }

        void OnDisable()
        {
            m_Health.OnDamaged -= OnDamaged;
            m_EnemyBrain.OnDetectedTarget -= OnDetectedTarget;
            if (m_WeaponController != null)
            {
                m_WeaponController.OnShoot -= OnShoot;
            }
        }

        void Start()
        {
            if (movementSfx != null)
            {
                m_AudioSource.clip = movementSfx;
                m_AudioSource.loop = true;
                m_AudioSource.Play();
            }
        }

        void Update()
        {
            if (movementSfx != null && m_EnemyBrain.NavMeshAgent != null)
            {
                float moveSpeed = m_EnemyBrain.NavMeshAgent.velocity.magnitude;
                m_AudioSource.pitch = Mathf.Lerp(pitchDistortionMovementSpeed.Min, pitchDistortionMovementSpeed.Max, moveSpeed / m_EnemyBrain.NavMeshAgent.speed);
            }
        }

        private void OnDamaged(float damage, GameObject source)
        {
            if (damageSfx != null)
            {
                AudioUtility.CreateSFX(damageSfx, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
            }
        }

        private void OnDetectedTarget()
        {
            if (detectionSfx != null)
            {
                AudioUtility.CreateSFX(detectionSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }
        }

        private void OnShoot()
        {
            // The WeaponController itself plays its own shooting sound. 
            // This event is here if you need to trigger an additional sound from the enemy's body, for example.
        }
    }
}

/* 
# METADATA
ScriptRole: Manages all audio feedback for an enemy, including movement, damage, and detection sounds.
RelatedScripts: EnemyBrain, Health, WeaponController, AudioUtility.
UsesSO: None.
ReceivesFrom: Health (OnDamaged), EnemyBrain (OnDetectedTarget), WeaponController (OnShoot).
SendsTo: AudioUtility.
Setup:
- Attach to the root of the enemy GameObject.
- Assign AudioClips in the Inspector.
- Requires an AudioSource component for the movement sound.
- Requires EnemyBrain and Health components.
*/
