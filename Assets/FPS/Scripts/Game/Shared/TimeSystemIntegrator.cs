using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Game;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Integrador que conecta el sistema de tiempo con el flujo del juego existente.
    /// Gestiona la inicialización y coordinación entre TimeManager y otros sistemas del juego.
    /// </summary>
    public class TimeSystemIntegrator : MonoBehaviour
    {
        [Header("🔗 Referencias de Sistemas")]
        [Tooltip("Manager de flujo del juego existente")]
        [SerializeField] private GameFlowManager gameFlowManager;

        [Tooltip("Prefab del TimeManager (se instancia si no existe)")]
        [SerializeField] private GameObject timeManagerPrefab;

        [Header("⚙️ Configuración")]
        [Tooltip("¿Inicializar automáticamente el sistema de tiempo al cargar la escena?")]
        [SerializeField] private bool autoInitialize = true;

        [Tooltip("Hora de inicio del juego (útil para testing o escenarios específicos)")]
        [Range(0f, 23.99f)]
        [SerializeField] private float startHour = 12f;

        // Estado interno
        private TimeManager timeManager;
        private bool systemsInitialized = false;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSystems();
        }

        private void Start()
        {
            if (autoInitialize && systemsInitialized)
            {
                SetupInitialTime();
            }
        }

        #endregion

        #region Inicialización

        private void InitializeSystems()
        {
            // Buscar TimeManager existente o crear uno nuevo
            timeManager = FindObjectOfType<TimeManager>();
            if (timeManager == null && timeManagerPrefab != null)
            {
                GameObject timeManagerInstance = Instantiate(timeManagerPrefab);
                timeManager = timeManagerInstance.GetComponent<TimeManager>();
            }

            if (timeManager == null)
            {
                Debug.LogError("TimeSystemIntegrator: No se pudo encontrar o crear TimeManager.");
                enabled = false;
                return;
            }

            // Buscar GameFlowManager si no está asignado
            if (gameFlowManager == null)
            {
                gameFlowManager = FindObjectOfType<GameFlowManager>();
            }

            systemsInitialized = true;
        }

        private void SetupInitialTime()
        {
            if (timeManager == null) return;

            // Establecer hora inicial
            timeManager.SetGameHour(startHour);

            // Conectar eventos del sistema de tiempo con el flujo del juego
            ConnectTimeEventsToGameFlow();
        }

        #endregion

        #region Integración con GameFlowManager

        private void ConnectTimeEventsToGameFlow()
        {
            if (gameFlowManager == null || timeManager == null) return;

            // Conectar evento de cambio día/noche para afectar gameplay
            timeManager.OnDayNightChanged += OnDayNightChanged;

            // Conectar eventos horarios para eventos específicos del juego
            timeManager.OnHourChanged += OnGameHourChanged;
        }

        private void OnDayNightChanged(bool isDay)
        {
            if (gameFlowManager == null) return;

            // Aquí puedes agregar lógica específica para afectar el gameplay según día/noche
            // Por ejemplo: cambiar dificultad, spawneo de enemigos, etc.

            if (isDay)
            {
                // Lógica para día
                Debug.Log("🌅 Amanece en el juego - Cambiando condiciones diurnas");
            }
            else
            {
                // Lógica para noche
                Debug.Log("🌙 Noche en el juego - Cambiando condiciones nocturnas");
            }
        }

        private void OnGameHourChanged(float hour)
        {
            if (gameFlowManager == null) return;

            // Eventos específicos por hora
            // Puedes expandir esto según las necesidades del juego

            if (Mathf.Abs(hour - 6f) < 0.01f) // 6:00 AM
            {
                Debug.Log("🌅 Amanecer - Inicio del turno diurno");
            }
            else if (Mathf.Abs(hour - 18f) < 0.01f) // 6:00 PM
            {
                Debug.Log("🌙 Atardecer - Inicio del turno nocturno");
            }
            else if (Mathf.Abs(hour - 0f) < 0.01f) // 12:00 AM
            {
                Debug.Log("🕛 Medianoche - Eventos especiales nocturnos");
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Pausa o reanuda todo el sistema de tiempo.
        /// </summary>
        public void SetTimePaused(bool paused)
        {
            if (timeManager != null)
            {
                timeManager.SetPaused(paused);
            }
        }

        /// <summary>
        /// Obtiene el estado actual del sistema de tiempo.
        /// </summary>
        public bool IsTimeSystemReady()
        {
            return systemsInitialized && timeManager != null;
        }

        /// <summary>
        /// Reinicia el ciclo de tiempo desde el principio.
        /// </summary>
        public void ResetTimeCycle()
        {
            if (timeManager != null)
            {
                timeManager.SetGameHour(startHour);
            }
        }

        #endregion
    }
}




