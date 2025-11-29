using UnityEngine;
using UnityEngine.Events;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Canal de eventos para horas específicas del día.
    /// Se activa cuando el reloj del juego alcanza una hora específica.
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Game/Events/Hour Event Channel", fileName = "HourEventChannel")]
    public class HourEventChannel : ScriptableObject
    {
        [Header("⏰ Configuración de Hora")]
        [Tooltip("Hora específica que activa este evento (0-23)")]
        [Range(0, 23)]
        public int targetHour = 12;

        [Tooltip("¿Se activa solo una vez por ciclo o cada ciclo?")]
        public bool oneTimePerCycle = false;

        [Header("🔄 Estado")]
        [Tooltip("¿Ya se activó este evento en el ciclo actual?")]
        [SerializeField] private bool eventTriggeredThisCycle = false;

        public UnityAction<int> OnHourReached;

        /// <summary>
        /// Levanta el evento si la hora coincide con la objetivo.
        /// </summary>
        public void CheckAndRaiseEvent(int currentHour, bool isNewCycle)
        {
            if (isNewCycle)
            {
                eventTriggeredThisCycle = false;
            }

            if (currentHour == targetHour && (!oneTimePerCycle || !eventTriggeredThisCycle))
            {
                eventTriggeredThisCycle = true;
                OnHourReached?.Invoke(targetHour);
            }
        }

        /// <summary>
        /// Resetea el estado del evento (útil para testing).
        /// </summary>
        public void ResetEvent()
        {
            eventTriggeredThisCycle = false;
        }
    }
}
