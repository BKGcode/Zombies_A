using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Utilidad para testing y debugging del sistema de día/noche.
    /// Proporciona controles en el editor para manipular el tiempo fácilmente.
    /// </summary>
    public class TimeSystemDebugger : MonoBehaviour
    {
        [Header("🎮 Controles de Debug")]
        [Tooltip("Tecla para avanzar tiempo rápidamente")]
        [SerializeField] private KeyCode fastForwardKey = KeyCode.F;

        [Tooltip("Tecla para pausar/reanudar tiempo")]
        [SerializeField] private KeyCode pauseKey = KeyCode.P;

        [Tooltip("Tecla para resetear el ciclo")]
        [SerializeField] private KeyCode resetKey = KeyCode.R;

        [Header("⚡ Configuración Debug")]
        [Tooltip("Multiplicador de velocidad cuando se avanza rápidamente")]
        [Range(1f, 100f)]
        [SerializeField] private float fastForwardMultiplier = 10f;

        [Tooltip("Mostrar información del sistema en pantalla")]
        [SerializeField] private bool showDebugInfo = true;

        [Tooltip("Posición del texto debug en pantalla")]
        [SerializeField] private Vector2 debugInfoPosition = new Vector2(10, 10);

        // Estado interno
        private TimeManager timeManager;
        private bool fastForwardActive = false;
        private float originalTimeScale = 1f;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void OnGUI()
        {
            if (showDebugInfo)
            {
                DrawDebugInfo();
            }
        }

        #endregion

        #region Inicialización

        private void CacheComponents()
        {
            timeManager = TimeManager.Instance;
        }

        #endregion

        #region Controles de Debug

        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(fastForwardKey))
            {
                ToggleFastForward();
            }

            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }

            if (Input.GetKeyDown(resetKey))
            {
                ResetTimeCycle();
            }

            // Controles con Shift para horas específicas
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                HandleShiftControls();
            }
        }

        private void HandleShiftControls()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetHour(6f);   // 6:00 AM
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetHour(12f);  // 12:00 PM
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetHour(18f);  // 6:00 PM
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetHour(0f);   // 12:00 AM
        }

        private void ToggleFastForward()
        {
            fastForwardActive = !fastForwardActive;

            if (fastForwardActive)
            {
                originalTimeScale = Time.timeScale;
                Time.timeScale = fastForwardMultiplier;
                if (timeManager != null) timeManager.SetTimeScale(fastForwardMultiplier);
                Debug.Log($"⏩ Avance rápido activado (x{fastForwardMultiplier})");
            }
            else
            {
                Time.timeScale = originalTimeScale;
                if (timeManager != null) timeManager.SetTimeScale(originalTimeScale);
                Debug.Log("⏯️ Avance rápido desactivado");
            }
        }

        private void TogglePause()
        {
            if (timeManager != null)
            {
                bool isPaused = Time.timeScale == 0f;
                timeManager.SetPaused(!isPaused);

                string state = !isPaused ? "pausado" : "reanudado";
                Debug.Log($"⏸️ Tiempo del juego {state}");
            }
        }

        private void ResetTimeCycle()
        {
            if (timeManager != null)
            {
                timeManager.SetGameHour(12f); // Reiniciar desde mediodía
                Time.timeScale = 1f;
                fastForwardActive = false;
                Debug.Log("🔄 Ciclo de tiempo reiniciado");
            }
        }

        private void SetHour(float hour)
        {
            if (timeManager != null)
            {
                timeManager.SetGameHour(hour);
                Time.timeScale = 1f;
                fastForwardActive = false;
                Debug.Log($"🕐 Tiempo establecido a las {hour:F1} horas");
            }
        }

        #endregion

        #region Información de Debug

        private void DrawDebugInfo()
        {
            if (timeManager == null) return;

            GUIStyle style = new GUIStyle
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };

            float x = debugInfoPosition.x;
            float y = debugInfoPosition.y;

            // Información del sistema de tiempo
            GUI.Label(new Rect(x, y, 300, 20), $"Hora del juego: {timeManager.GetCurrentGameHour():F2}", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), $"Es de día: {timeManager.IsDay()}", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), $"Progreso del período: {timeManager.GetCurrentPeriodProgress():P}", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), $"TimeScale: {Time.timeScale:F1}", style);
            y += 15;

            // Controles disponibles
            y += 10;
            GUI.Label(new Rect(x, y, 300, 20), "Controles:", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), $"{fastForwardKey}: Avance rápido", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), $"{pauseKey}: Pausar/Reanudar", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), $"{resetKey}: Reiniciar ciclo", style);
            y += 15;

            GUI.Label(new Rect(x, y, 300, 20), "Shift + 1-4: Hora específica", style);
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Activa/desactiva la información de debug en pantalla.
        /// </summary>
        public void ToggleDebugInfo()
        {
            showDebugInfo = !showDebugInfo;
        }

        /// <summary>
        /// Establece la posición del texto debug.
        /// </summary>
        public void SetDebugInfoPosition(Vector2 position)
        {
            debugInfoPosition = position;
        }

        /// <summary>
        /// Obtiene estadísticas actuales del sistema.
        /// </summary>
        public string GetSystemStats()
        {
            if (timeManager == null) return "Sistema no inicializado";

            return $"Hora: {timeManager.GetCurrentGameHour():F2}h | " +
                   $"Día: {timeManager.IsDay()} | " +
                   $"Progreso: {timeManager.GetCurrentPeriodProgress():P} | " +
                   $"TimeScale: {Time.timeScale:F1}";
        }

        #endregion
    }
}
