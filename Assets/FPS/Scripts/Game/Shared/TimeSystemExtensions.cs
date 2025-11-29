using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Utilidades y métodos de extensión para facilitar el trabajo con el sistema de día/noche.
    /// Proporciona funciones útiles para integración rápida con otros sistemas.
    /// </summary>
    public static class TimeSystemExtensions
    {
        /// <summary>
        /// Obtiene el progreso normalizado del día actual (0-1).
        /// </summary>
        public static float GetDayProgress(this TimeManager timeManager)
        {
            if (timeManager == null) return 0f;

            float cycleTime = timeManager.GetCurrentCycleTime();
            float dayPercentage = timeManager.dayNightConfig?.dayPercentage ?? 0.5f;

            if (cycleTime >= dayPercentage)
            {
                return 1f; // El día ha terminado, progreso al 100%
            }

            if (dayPercentage == 0)
            {
                return 1f; // Evitar división por cero
            }

            return cycleTime / dayPercentage;
        }

        /// <summary>
        /// Obtiene el progreso normalizado de la noche actual (0-1).
        /// </summary>
        public static float GetNightProgress(this TimeManager timeManager)
        {
            if (timeManager == null) return 0f;

            float cycleTime = timeManager.GetCurrentCycleTime();
            float dayDuration = timeManager.dayNightConfig?.dayPercentage ?? 0.5f;

            if (cycleTime < dayDuration) return 0f;

            float nightDuration = timeManager.dayNightConfig?.nightPercentage ?? 0.5f;
            return Mathf.Clamp01((cycleTime - dayDuration) / nightDuration);
        }

        /// <summary>
        /// Verifica si es una hora específica del día con un margen de tolerancia.
        /// </summary>
        public static bool IsHour(this TimeManager timeManager, float targetHour, float tolerance = 0.1f)
        {
            if (timeManager == null) return false;

            float currentHour = timeManager.GetCurrentGameHour();
            return Mathf.Abs(currentHour - targetHour) <= tolerance;
        }

        /// <summary>
        /// Obtiene el tiempo restante hasta una hora específica en horas.
        /// </summary>
        public static float GetHoursUntil(this TimeManager timeManager, float targetHour)
        {
            if (timeManager == null) return 0f;

            float currentHour = timeManager.GetCurrentGameHour();
            float hoursUntil = targetHour - currentHour;

            if (hoursUntil < 0)
            {
                hoursUntil += 24f; // Pasar al día siguiente
            }

            return hoursUntil;
        }

        /// <summary>
        /// Formatea la hora del juego como string legible.
        /// </summary>
        public static string GetFormattedTime(this TimeManager timeManager, bool use24HourFormat = true)
        {
            if (timeManager == null) return "00:00";

            float gameHour = timeManager.GetCurrentGameHour();
            int hour = Mathf.FloorToInt(gameHour);
            int minute = Mathf.FloorToInt((gameHour - hour) * 60f);

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

        /// <summary>
        /// Obtiene información completa del estado del tiempo.
        /// </summary>
        public static string GetTimeInfo(this TimeManager timeManager)
        {
            if (timeManager == null) return "Sistema no disponible";

            return $"Hora: {timeManager.GetFormattedTime()} | " +
                   $"Es día: {timeManager.IsDay()} | " +
                   $"Progreso día: {timeManager.GetDayProgress():P} | " +
                   $"Progreso noche: {timeManager.GetNightProgress():P}";
        }
    }

    /// <summary>
    /// MonoBehaviour con métodos útiles para trabajar con el sistema de tiempo.
    /// </summary>
    public class TimeHelper : MonoBehaviour
    {
        [Header("⚙️ Configuración")]
        [Tooltip("Actualizar información cada X segundos")]
        [SerializeField] private float updateInterval = 1f;

        private TimeManager timeManager;
        private float lastUpdateTime;

        private void Awake()
        {
            timeManager = TimeManager.Instance;
        }

        private void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                lastUpdateTime = Time.time;
                // Aquí puedes añadir lógica que necesite actualización periódica
            }
        }

        /// <summary>
        /// Registra un evento que ocurre en una hora específica.
        /// </summary>
        public void RegisterHourlyEvent(float hour, UnityEngine.Events.UnityAction action)
        {
            if (timeManager != null)
            {
                timeManager.OnHourChanged += (gameHour) =>
                {
                    if (Mathf.Abs(gameHour - hour) < 0.01f)
                    {
                        action?.Invoke();
                    }
                };
            }
        }

        /// <summary>
        /// Registra un evento que ocurre al cambiar entre día y noche.
        /// </summary>
        public void RegisterDayNightEvent(UnityEngine.Events.UnityAction<bool> action)
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged += (isDay) => action?.Invoke(isDay);
            }
        }

        /// <summary>
        /// Obtiene información detallada del sistema de tiempo.
        /// </summary>
        public string GetDetailedTimeInfo()
        {
            return timeManager?.GetTimeInfo() ?? "Sistema no disponible";
        }
    }
}
