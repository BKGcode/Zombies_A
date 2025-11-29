using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Events;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Gestor de eventos específicos para el juego de vigilante nocturno.
    /// Maneja eventos relacionados con turnos de trabajo, emergencias,
    /// cambios de comportamiento según la hora del día.
    /// </summary>
    public class VigilanteGameEvents : MonoBehaviour
    {
        [Header("🕐 Eventos por Hora")]
        [Tooltip("Evento cuando comienza el turno de día (6:00 AM)")]
        public UnityEvent onDayShiftStart;

        [Tooltip("Evento cuando comienza el turno de noche (6:00 PM)")]
        public UnityEvent onNightShiftStart;

        [Tooltip("Evento cuando llega la medianoche (12:00 AM)")]
        public UnityEvent onMidnight;

        [Tooltip("Evento cuando llega el mediodía (12:00 PM)")]
        public UnityEvent onNoon;

        [Header("🚨 Eventos Especiales")]
        [Tooltip("Evento cuando ocurren situaciones de emergencia")]
        public UnityEvent onEmergency;

        [Tooltip("Evento cuando el jugador debe reportar su posición")]
        public UnityEvent onPositionReport;

        [Tooltip("Evento cuando cambian las condiciones de patrullaje")]
        public UnityEvent onPatrolConditionsChanged;

        [Header("😴 Sistema de Fatiga")]
        [Tooltip("Evento cuando el jugador se cansa (para implementar después)")]
        public UnityEvent onPlayerFatigue;

        [Tooltip("Evento cuando el jugador necesita descansar")]
        public UnityEvent onPlayerNeedsRest;

        [Header("⚙️ Configuración")]
        [Tooltip("¿Activar eventos de emergencia aleatoriamente?")]
        [SerializeField] private bool enableRandomEmergencies = false;

        [Tooltip("Probabilidad de emergencia por hora (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float emergencyProbability = 0.1f;

        [Tooltip("Intervalo mínimo entre emergencias (en horas)")]
        [Min(0.1f)]
        [SerializeField] private float minEmergencyInterval = 2f;

        // Estado interno
        private TimeManager timeManager;
        private float lastEmergencyTime = -10f;
        private bool isNightShift = false;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            SubscribeToTimeEvents();
            InitializeGameState();
        }

        private void Update()
        {
            HandleRandomEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTimeEvents();
        }

        #endregion

        #region Inicialización

        private void CacheComponents()
        {
            timeManager = TimeManager.Instance;
        }

        private void SubscribeToTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnHourChanged += OnGameHourChanged;
                timeManager.OnDayNightChanged += OnDayNightChanged;
            }
        }

        private void UnsubscribeFromTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnHourChanged -= OnGameHourChanged;
                timeManager.OnDayNightChanged -= OnDayNightChanged;
            }
        }

        private void InitializeGameState()
        {
            if (timeManager == null) return;

            // Inicializar estado del turno
            isNightShift = timeManager.IsNight();
        }

        #endregion

        #region Eventos de Tiempo

        private void OnGameHourChanged(float hour)
        {
            HandleSpecificHourEvents(hour);
        }

        private void OnDayNightChanged(bool isDay)
        {
            if (isDay && isNightShift)
            {
                // Cambio de turno noche → día
                isNightShift = false;
                onDayShiftStart?.Invoke();
                Debug.Log("🌅 Turno de día iniciado");
            }
            else if (!isDay && !isNightShift)
            {
                // Cambio de turno día → noche
                isNightShift = true;
                onNightShiftStart?.Invoke();
                Debug.Log("🌙 Turno de noche iniciado");
            }
        }

        private void HandleSpecificHourEvents(float hour)
        {
            // Eventos específicos por hora

            if (Mathf.Abs(hour - 6f) < 0.01f) // 6:00 AM - Inicio turno día
            {
                onDayShiftStart?.Invoke();
            }
            else if (Mathf.Abs(hour - 12f) < 0.01f) // 12:00 PM - Mediodía
            {
                onNoon?.Invoke();
            }
            else if (Mathf.Abs(hour - 18f) < 0.01f) // 6:00 PM - Inicio turno noche
            {
                onNightShiftStart?.Invoke();
            }
            else if (Mathf.Abs(hour - 0f) < 0.01f) // 12:00 AM - Medianoche
            {
                onMidnight?.Invoke();
            }

            // Eventos regulares durante el turno
            if (isNightShift && hour > 18f && hour < 6f)
            {
                HandleNightShiftEvents(hour);
            }
        }

        private void HandleNightShiftEvents(float hour)
        {
            // Eventos específicos del turno de noche

            if (Mathf.Abs(hour - 20f) < 0.01f) // 8:00 PM
            {
                // Reporte inicial del turno
                onPositionReport?.Invoke();
            }
            else if (Mathf.Abs(hour - 22f) < 0.01f) // 10:00 PM
            {
                // Cambio de condiciones de patrullaje
                onPatrolConditionsChanged?.Invoke();
            }
            else if (Mathf.Abs(hour - 2f) < 0.01f) // 2:00 AM
            {
                // Evento de medianoche
                onMidnight?.Invoke();
            }
            else if (Mathf.Abs(hour - 4f) < 0.01f) // 4:00 AM
            {
                // Fatiga del jugador
                onPlayerFatigue?.Invoke();
            }
        }

        #endregion

        #region Eventos Aleatorios

        private void HandleRandomEvents()
        {
            if (!enableRandomEmergencies || timeManager == null) return;

            float currentHour = timeManager.GetCurrentGameHour();
            float timeSinceLastEmergency = currentHour - lastEmergencyTime;

            // Solo permitir emergencias durante el turno de noche
            if (isNightShift && timeSinceLastEmergency >= minEmergencyInterval)
            {
                if (Random.value < emergencyProbability * Time.deltaTime)
                {
                    TriggerEmergency();
                }
            }
        }

        private void TriggerEmergency()
        {
            lastEmergencyTime = timeManager.GetCurrentGameHour();
            onEmergency?.Invoke();

            Debug.Log("🚨 ¡Emergencia! Evento aleatorio activado");
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Fuerza el inicio de un evento específico.
        /// </summary>
        public void ForceEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "emergency":
                    onEmergency?.Invoke();
                    break;
                case "positionreport":
                    onPositionReport?.Invoke();
                    break;
                case "patrolchange":
                    onPatrolConditionsChanged?.Invoke();
                    break;
                case "fatigue":
                    onPlayerFatigue?.Invoke();
                    break;
                case "midnight":
                    onMidnight?.Invoke();
                    break;
                case "noon":
                    onNoon?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Obtiene el estado actual del turno.
        /// </summary>
        public bool IsNightShift()
        {
            return isNightShift;
        }

        /// <summary>
        /// Configura si el turno es nocturno (útil para testing).
        /// </summary>
        public void SetNightShift(bool nightShift)
        {
            isNightShift = nightShift;
        }

        #endregion
    }
}

