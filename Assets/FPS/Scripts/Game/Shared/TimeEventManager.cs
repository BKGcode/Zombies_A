using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Gestor de eventos horarios que coordina múltiples canales de eventos.
    /// Se conecta con el TimeManager para activar eventos en horas específicas.
    /// </summary>
    public class TimeEventManager : MonoBehaviour
    {
        [Header("📅 Canales de Eventos")]
        [Tooltip("Lista de canales de eventos horarios")]
        [SerializeField] private HourEventChannel[] hourEventChannels;

        [Header("🌅 Canal Día/Noche")]
        [Tooltip("Canal para eventos de cambio día/noche")]
        [SerializeField] private DayNightEventChannel dayNightEventChannel;

        // Componentes cacheados
        private TimeManager timeManager;

        // Estado interno
        private int lastCheckedHour = -1;
        private bool lastDayState = true;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateConfiguration();
        }

        private void Start()
        {
            SubscribeToTimeEvents();
            InitializeEventStates();
        }

        private void Update()
        {
            if (timeManager == null) return;

            CheckHourEvents();
            CheckDayNightEvents();
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

        private void ValidateConfiguration()
        {
            if (hourEventChannels == null || hourEventChannels.Length == 0)
            {
                Debug.LogWarning("TimeEventManager: No hay canales de eventos horarios configurados.");
            }
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

        private void InitializeEventStates()
        {
            lastCheckedHour = Mathf.FloorToInt(timeManager.GetCurrentGameHour());
            lastDayState = timeManager.IsDay();
        }

        #endregion

        #region Eventos de Tiempo

        private void OnGameHourChanged(float gameHour)
        {
            int currentHour = Mathf.FloorToInt(gameHour);

            if (currentHour != lastCheckedHour)
            {
                CheckHourEvents();
                lastCheckedHour = currentHour;
            }
        }

        private void OnDayNightChanged(bool isDay)
        {
            if (isDay != lastDayState && dayNightEventChannel != null)
            {
                dayNightEventChannel.RaiseEvent(isDay);
                lastDayState = isDay;
            }
        }

        #endregion

        #region Gestión de Eventos

        private void CheckHourEvents()
        {
            if (hourEventChannels == null || timeManager == null) return;

            int currentHour = Mathf.FloorToInt(timeManager.GetCurrentGameHour());
            bool isNewCycle = (currentHour == 0 && lastCheckedHour == 23);

            foreach (HourEventChannel channel in hourEventChannels)
            {
                if (channel != null)
                {
                    channel.CheckAndRaiseEvent(currentHour, isNewCycle);
                }
            }
        }

        private void CheckDayNightEvents()
        {
            if (timeManager == null) return;

            bool currentDayState = timeManager.IsDay();

            if (currentDayState != lastDayState && dayNightEventChannel != null)
            {
                dayNightEventChannel.RaiseEvent(currentDayState);
                lastDayState = currentDayState;
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Agrega un nuevo canal de eventos horarios en runtime.
        /// </summary>
        public void AddHourEventChannel(HourEventChannel channel)
        {
            if (channel == null) return;

            // Verificar si ya existe
            foreach (HourEventChannel existing in hourEventChannels)
            {
                if (existing == channel) return;
            }

            // Crear nuevo array con el canal adicional
            HourEventChannel[] newChannels = new HourEventChannel[hourEventChannels.Length + 1];
            hourEventChannels.CopyTo(newChannels, 0);
            newChannels[newChannels.Length - 1] = channel;
            hourEventChannels = newChannels;
        }

        /// <summary>
        /// Remueve un canal de eventos horarios en runtime.
        /// </summary>
        public void RemoveHourEventChannel(HourEventChannel channel)
        {
            if (channel == null || hourEventChannels == null) return;

            int index = System.Array.IndexOf(hourEventChannels, channel);
            if (index >= 0)
            {
                HourEventChannel[] newChannels = new HourEventChannel[hourEventChannels.Length - 1];
                System.Array.Copy(hourEventChannels, 0, newChannels, 0, index);
                System.Array.Copy(hourEventChannels, index + 1, newChannels, index, hourEventChannels.Length - index - 1);
                hourEventChannels = newChannels;
            }
        }

        #endregion
    }
}
