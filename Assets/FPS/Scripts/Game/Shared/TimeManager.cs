using UnityEngine;
using System;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Gestor central del sistema de tiempo del juego.
    /// Controla el ciclo día/noche, pausa del juego y eventos horarios.
    /// Implementa singleton para acceso global seguro.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Ejecutar antes que otros sistemas
    public class TimeManager : MonoBehaviour
    {
        [Header("🔧 Configuración")]
        [Tooltip("ScriptableObject con configuración del ciclo día/noche")]
        [SerializeField] public DayNightCycle dayNightConfig;

        [Header("⚡ PRUEBAS RÁPIDAS (Override)")]
        [Tooltip("¿Usar duración personalizada en lugar del DayNightCycle?")]
        [SerializeField] private bool useCustomDuration = false;

        [Tooltip("Duración TOTAL del ciclo completo en SEGUNDOS (día + noche)")]
        [SerializeField] 
        [Range(10f, 7200f)] // 10 segundos a 2 horas
        private float customCycleDurationSeconds = 120f; // 2 minutos por defecto

        [Tooltip("Multiplicador de velocidad del tiempo (1 = normal, 2 = doble velocidad)")]
        [SerializeField]
        [Range(0.1f, 100f)]
        private float timeSpeedMultiplier = 1f;

        [Header("⏸️ Estado de Pausa")]
        [Tooltip("¿Está el juego pausado actualmente?")]
        [SerializeField] private bool isPaused = false;

        [Header("⏰ Estado del Tiempo (Solo lectura)")]
        [Tooltip("Tiempo actual dentro del ciclo (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float currentCycleTime = 0f;

        [Tooltip("Hora actual del juego (0-23)")]
        [Range(0f, 23.99f)]
        [SerializeField] private float currentGameHour = 12f;

        [Header("📊 Información en Tiempo Real")]
        [SerializeField] private string currentPeriod = "Día";
        [SerializeField] private string timeFormatted = "12:00";
        [SerializeField] private float cycleDurationUsed = 0f;

        // Eventos del sistema
        public event Action<bool> OnDayNightChanged; // true = día, false = noche
        public event Action<float> OnHourChanged; // hora actual
        public event Action<float> OnCycleTimeChanged; // tiempo del ciclo (0-1)

        // Estado interno
        private float elapsedTime = 0f;
        private float lastUpdateTime = 0f;
        private bool wasDay = true;

        // Singleton
        public static TimeManager Instance { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSingleton();
            ValidateConfiguration();
            InitializeTime();
        }

        private void Update()
        {
            if (isPaused || dayNightConfig == null) return;

            UpdateGameTime();
            CheckForTimeEvents();
            UpdateDebugInfo();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Inicialización

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("TimeManager: Múltiples instancias detectadas. Destruyendo duplicado.");
                Destroy(gameObject);
            }
        }

        private void ValidateConfiguration()
        {
            if (dayNightConfig == null)
            {
                Debug.LogError("TimeManager: DayNightCycle no asignado. El sistema no funcionará correctamente.");
                enabled = false;
                return;
            }
        }

        private void InitializeTime()
        {
            elapsedTime = 0f;
            lastUpdateTime = Time.time;
            UpdateGameTime();
        }

        #endregion

        #region Control de Tiempo

        /// <summary>
        /// Pausa o reanuda el flujo del tiempo del juego.
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (isPaused == paused) return;

            isPaused = paused;

            if (isPaused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// Cambia la velocidad del tiempo del juego.
        /// </summary>
        public void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Max(0f, scale);
        }

        #endregion

        #region Consulta de Tiempo

        /// <summary>
        /// Obtiene la hora actual del juego (0-23).
        /// </summary>
        public float GetCurrentGameHour()
        {
            return currentGameHour;
        }

        /// <summary>
        /// Obtiene el tiempo normalizado del ciclo (0-1).
        /// </summary>
        public float GetCurrentCycleTime()
        {
            return currentCycleTime;
        }

        /// <summary>
        /// Verifica si actualmente es de día.
        /// </summary>
        public bool IsDay()
        {
            return currentCycleTime < dayNightConfig.dayPercentage;
        }

        /// <summary>
        /// Verifica si actualmente es de noche.
        /// </summary>
        public bool IsNight()
        {
            return !IsDay();
        }

        /// <summary>
        /// Obtiene el progreso del período actual (0-1).
        /// </summary>
        public float GetCurrentPeriodProgress()
        {
            if (dayNightConfig == null) return 0f;

            if (IsDay())
            {
                return currentCycleTime / dayNightConfig.dayPercentage;
            }
            else
            {
                float nightStart = dayNightConfig.dayPercentage;
                return (currentCycleTime - nightStart) / dayNightConfig.nightPercentage;
            }
        }

        /// <summary>
        /// Formatea la hora del juego como string legible.
        /// </summary>
        public string GetFormattedTime(bool use24HourFormat = true)
        {
            int hour = Mathf.FloorToInt(currentGameHour);
            int minute = Mathf.FloorToInt((currentGameHour - hour) * 60f);

            if (use24HourFormat)
            {
                return $"{hour:D2}:{minute:D2}";
            }
            else
            {
                string period = hour >= 12 ? "PM" : "AM";
                int displayHour = hour % 12;
                if (displayHour == 0) displayHour = 12;
                return $"{displayHour:D2}:{minute:D2} {period}";
            }
        }

        #endregion

        #region Actualización de Tiempo

        private void UpdateGameTime()
        {
            float deltaTime = Time.time - lastUpdateTime;
            
            // Aplicar multiplicador de velocidad
            deltaTime *= timeSpeedMultiplier;
            
            elapsedTime += deltaTime;

            // Obtener duración del ciclo (custom o del ScriptableObject)
            float cycleDuration = GetActiveCycleDuration();

            // Actualizar tiempo del ciclo
            currentCycleTime = (elapsedTime / cycleDuration) % 1f;

            // Calcular hora del juego (24 horas por ciclo)
            currentGameHour = currentCycleTime * 24f;

            lastUpdateTime = Time.time;
        }

        /// <summary>
        /// Obtiene la duración del ciclo activa (custom o del config).
        /// </summary>
        private float GetActiveCycleDuration()
        {
            if (useCustomDuration)
            {
                return customCycleDurationSeconds;
            }
            return dayNightConfig?.cycleDurationSeconds ?? 7200f;
        }

        private void CheckForTimeEvents()
        {
            bool currentlyDay = IsDay();

            // Evento de cambio día/noche
            if (currentlyDay != wasDay)
            {
                OnDayNightChanged?.Invoke(currentlyDay);
                wasDay = currentlyDay;
            }

            // Eventos por hora específica
            if (dayNightConfig != null && dayNightConfig.eventHours != null)
            {
                foreach (int eventHour in dayNightConfig.eventHours)
                {
                    if (Mathf.Abs(currentGameHour - eventHour) < 0.01f) // Pequeño margen para precisión float
                    {
                        OnHourChanged?.Invoke(currentGameHour);
                        break;
                    }
                }
            }
        }

        private void UpdateDebugInfo()
        {
            // Actualizar información de debug visible en el Inspector
            currentPeriod = IsDay() ? "☀️ Día" : "🌙 Noche";
            timeFormatted = GetFormattedTime(true);
            cycleDurationUsed = GetActiveCycleDuration();
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Establece la hora específica del juego (útil para testing o eventos específicos).
        /// </summary>
        public void SetGameHour(float hour)
        {
            currentGameHour = Mathf.Clamp(hour, 0f, 23.99f);
            currentCycleTime = currentGameHour / 24f;
            
            float cycleDuration = GetActiveCycleDuration();
            elapsedTime = currentCycleTime * cycleDuration;

            // Forzar actualización de eventos
            wasDay = !IsDay();
            OnDayNightChanged?.Invoke(IsDay());
            OnCycleTimeChanged?.Invoke(currentCycleTime);
        }

        /// <summary>
        /// Avanza el tiempo del juego en la cantidad especificada (en horas).
        /// </summary>
        public void AdvanceTime(float hours)
        {
            float targetHour = (currentGameHour + hours) % 24f;
            SetGameHour(targetHour);
        }

        /// <summary>
        /// Establece el multiplicador de velocidad del tiempo.
        /// </summary>
        public void SetTimeSpeedMultiplier(float multiplier)
        {
            timeSpeedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        /// <summary>
        /// Obtiene el multiplicador de velocidad actual.
        /// </summary>
        public float GetTimeSpeedMultiplier()
        {
            return timeSpeedMultiplier;
        }

        /// <summary>
        /// Establece la duración personalizada del ciclo.
        /// </summary>
        public void SetCustomCycleDuration(float durationInSeconds)
        {
            customCycleDurationSeconds = Mathf.Clamp(durationInSeconds, 10f, 7200f);
            useCustomDuration = true;
        }

        /// <summary>
        /// Obtiene la duración del ciclo actualmente en uso.
        /// </summary>
        public float GetCurrentCycleDuration()
        {
            return GetActiveCycleDuration();
        }

        /// <summary>
        /// Activa/desactiva el uso de duración personalizada.
        /// </summary>
        public void SetUseCustomDuration(bool useCustom)
        {
            useCustomDuration = useCustom;
        }

        #endregion
    }
}
