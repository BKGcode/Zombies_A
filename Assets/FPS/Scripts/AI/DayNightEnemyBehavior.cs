using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game;
using FPS.Game.Shared;

namespace FPS.AI
{
    /// <summary>
    /// Componente que modifica el comportamiento de enemigos según el ciclo día/noche.
    /// Se integra con el sistema de tiempo para hacer enemigos más agresivos durante la noche.
    /// </summary>
    [RequireComponent(typeof(EnemyBrain))]
    public class DayNightEnemyBehavior : MonoBehaviour
    {
        [Header("🌅 Configuración Día")]
        [Tooltip("Multiplicador de velocidad durante el día")]
        [Range(0.1f, 3f)]
        [SerializeField] private float daySpeedMultiplier = 0.8f;

        [Tooltip("Multiplicador de daño durante el día")]
        [Range(0.1f, 2f)]
        [SerializeField] private float dayDamageMultiplier = 0.7f;

        [Tooltip("Probabilidad de detección durante el día (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float dayDetectionChance = 0.6f;

        [Header("🌙 Configuración Noche")]
        [Tooltip("Multiplicador de velocidad durante la noche")]
        [Range(0.1f, 3f)]
        [SerializeField] private float nightSpeedMultiplier = 1.4f;

        [Tooltip("Multiplicador de daño durante la noche")]
        [Range(0.1f, 3f)]
        [SerializeField] private float nightDamageMultiplier = 1.3f;

        [Tooltip("Probabilidad de detección durante la noche (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float nightDetectionChance = 0.9f;

        [Header("⚡ Transición")]
        [Tooltip("Velocidad de transición entre comportamientos día/noche")]
        [Range(0.1f, 5f)]
        [SerializeField] private float transitionSpeed = 2f;

        // Componentes cacheados
        private EnemyBrain enemyBrain;
        private TimeManager timeManager;

        // Estado interno
        private float currentSpeedMultiplier = 1f;
        private float currentDamageMultiplier = 1f;
        private float currentDetectionChance = 1f;
        private float targetSpeedMultiplier = 1f;
        private float targetDamageMultiplier = 1f;
        private float targetDetectionChance = 1f;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateConfiguration();
        }

        private void Start()
        {
            SubscribeToTimeEvents();
            InitializeBehavior();
        }

        private void Update()
        {
            UpdateBehaviorTransitions();
            ApplyCurrentBehavior();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTimeEvents();
        }

        #endregion

        #region Inicialización

        private void CacheComponents()
        {
            enemyBrain = GetComponent<EnemyBrain>();
            timeManager = TimeManager.Instance;
        }

        private void ValidateConfiguration()
        {
            if (enemyBrain == null)
            {
                Debug.LogError("DayNightEnemyBehavior: No se encontró EnemyBrain en este GameObject.");
                enabled = false;
                return;
            }
        }

        private void SubscribeToTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged += OnDayNightChanged;
            }
        }

        private void UnsubscribeFromTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged -= OnDayNightChanged;
            }
        }

        private void InitializeBehavior()
        {
            if (timeManager == null) return;

            // Establecer comportamiento inicial basado en la hora actual
            UpdateTargetBehavior(timeManager.IsDay());
            currentSpeedMultiplier = targetSpeedMultiplier;
            currentDamageMultiplier = targetDamageMultiplier;
            currentDetectionChance = targetDetectionChance;
        }

        #endregion

        #region Eventos de Tiempo

        private void OnDayNightChanged(bool isDay)
        {
            UpdateTargetBehavior(isDay);
        }

        private void UpdateTargetBehavior(bool isDay)
        {
            if (isDay)
            {
                targetSpeedMultiplier = daySpeedMultiplier;
                targetDamageMultiplier = dayDamageMultiplier;
                targetDetectionChance = dayDetectionChance;
            }
            else
            {
                targetSpeedMultiplier = nightSpeedMultiplier;
                targetDamageMultiplier = nightDamageMultiplier;
                targetDetectionChance = nightDetectionChance;
            }
        }

        #endregion

        #region Actualización de Comportamiento

        private void UpdateBehaviorTransitions()
        {
            // Suavizar transición entre valores objetivo y actual
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, Time.deltaTime * transitionSpeed);
            currentDamageMultiplier = Mathf.Lerp(currentDamageMultiplier, targetDamageMultiplier, Time.deltaTime * transitionSpeed);
            currentDetectionChance = Mathf.Lerp(currentDetectionChance, targetDetectionChance, Time.deltaTime * transitionSpeed);
        }

        private void ApplyCurrentBehavior()
        {
            if (enemyBrain == null) return;

            // Aplicar multiplicadores al controlador de enemigos
            enemyBrain.SetSpeedMultiplier(currentSpeedMultiplier);
            enemyBrain.SetDamageMultiplier(currentDamageMultiplier);
            enemyBrain.SetDetectionRangeMultiplier(currentDetectionChance);
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Obtiene el multiplicador actual de velocidad.
        /// </summary>
        public float GetCurrentSpeedMultiplier()
        {
            return currentSpeedMultiplier;
        }

        /// <summary>
        /// Obtiene el multiplicador actual de daño.
        /// </summary>
        public float GetCurrentDamageMultiplier()
        {
            return currentDamageMultiplier;
        }

        /// <summary>
        /// Obtiene la probabilidad actual de detección.
        /// </summary>
        public float GetCurrentDetectionChance()
        {
            return currentDetectionChance;
        }

        /// <summary>
        /// Fuerza actualización inmediata del comportamiento.
        /// </summary>
        public void ForceBehaviorUpdate()
        {
            if (timeManager != null)
            {
                UpdateTargetBehavior(timeManager.IsDay());
                currentSpeedMultiplier = targetSpeedMultiplier;
                currentDamageMultiplier = targetDamageMultiplier;
                currentDetectionChance = targetDetectionChance;
                ApplyCurrentBehavior();
            }
        }

        #endregion
    }
}

