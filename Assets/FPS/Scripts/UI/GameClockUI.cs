using UnityEngine;
using TMPro;
using FPS.Game.Shared;

namespace FPS.UI
{
    /// <summary>
    /// Componente UI que muestra la hora actual del juego al jugador.
    /// Se actualiza automáticamente según el ciclo día/noche.
    /// </summary>
    public class GameClockUI : MonoBehaviour
    {
        [Header("📱 Referencias UI")]
        [Tooltip("Texto donde se mostrará la hora del juego")]
        [SerializeField] private TextMeshProUGUI clockText;

        [Header("🎨 Configuración Visual")]
        [Tooltip("Formato de la hora (24h o 12h con AM/PM)")]
        [SerializeField] private ClockFormat clockFormat = ClockFormat.Format24H;

        [Tooltip("Color del texto durante el día")]
        [SerializeField] private Color dayColor = Color.black;

        [Tooltip("Color del texto durante la noche")]
        [SerializeField] private Color nightColor = Color.white;

        [Header("⚡ Configuración de Actualización")]
        [Tooltip("¿Actualizar cada segundo o solo cuando cambie el minuto?")]
        [SerializeField] private bool updateEverySecond = true;

        [Tooltip("Suavizado del cambio de color día/noche")]
        [Range(0.1f, 5f)]
        [SerializeField] private float colorTransitionSpeed = 2f;

        // Componentes cacheados
        private TimeManager timeManager;

        // Estado interno
        private int lastDisplayedMinute = -1;
        private Color targetColor = Color.white;

        public enum ClockFormat
        {
            Format24H,
            Format12H
        }

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateReferences();
        }

        private void Start()
        {
            SubscribeToTimeEvents();
            InitializeClock();
        }

        private void Update()
        {
            if (timeManager == null) return;

            UpdateClockDisplay();
            UpdateClockColor();
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

        private void ValidateReferences()
        {
            if (clockText == null)
            {
                clockText = GetComponent<TextMeshProUGUI>();
            }

            if (clockText == null)
            {
                Debug.LogError("GameClockUI: No se encontró componente TextMeshProUGUI.");
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

        private void InitializeClock()
        {
            if (timeManager == null) return;

            UpdateClockDisplay();
            UpdateClockColor();
            lastDisplayedMinute = Mathf.FloorToInt(timeManager.GetCurrentGameHour() * 60f) % 60;
        }

        #endregion

        #region Eventos de Tiempo

        private void OnDayNightChanged(bool isDay)
        {
            targetColor = isDay ? dayColor : nightColor;
        }

        #endregion

        #region Actualización de UI

        private void UpdateClockDisplay()
        {
            if (clockText == null || timeManager == null) return;

            float gameHour = timeManager.GetCurrentGameHour();
            int hour = Mathf.FloorToInt(gameHour);
            int minute = Mathf.FloorToInt((gameHour - hour) * 60f);

            // Solo actualizar si cambió el minuto (o cada segundo si está habilitado)
            if (!updateEverySecond && minute == lastDisplayedMinute) return;

            string timeString = FormatTime(hour, minute);
            clockText.text = timeString;

            lastDisplayedMinute = minute;
        }

        private void UpdateClockColor()
        {
            if (clockText == null) return;

            // Suavizado de transición de color
            clockText.color = Color.Lerp(clockText.color, targetColor, Time.deltaTime * colorTransitionSpeed);
        }

        private string FormatTime(int hour, int minute)
        {
            switch (clockFormat)
            {
                case ClockFormat.Format24H:
                    return string.Format("{0:D2}:{1:D2}", hour, minute);

                case ClockFormat.Format12H:
                    string period = hour >= 12 ? "PM" : "AM";
                    int displayHour = hour % 12;
                    if (displayHour == 0) displayHour = 12;
                    return string.Format("{0:D2}:{1:D2} {2}", displayHour, minute, period);

                default:
                    return string.Format("{0:D2}:{1:D2}", hour, minute);
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Cambia el formato del reloj (24h/12h).
        /// </summary>
        public void SetClockFormat(ClockFormat format)
        {
            clockFormat = format;
            UpdateClockDisplay();
        }

        /// <summary>
        /// Muestra/oculta el reloj.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (clockText != null)
            {
                clockText.enabled = visible;
            }
        }

        /// <summary>
        /// Fuerza una actualización inmediata del reloj.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateClockDisplay();
            UpdateClockColor();
        }

        #endregion
    }
}
