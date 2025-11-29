using UnityEngine;
using UnityEngine.Events;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Canal de eventos para cambios entre día y noche.
    /// Se activa cuando el juego cambia entre período diurno y nocturno.
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Game/Events/Day Night Event Channel", fileName = "DayNightEventChannel")]
    public class DayNightEventChannel : ScriptableObject
    {
        public UnityAction<bool> OnDayNightChanged; // true = día, false = noche

        /// <summary>
        /// Levanta el evento de cambio día/noche.
        /// </summary>
        public void RaiseEvent(bool isDay)
        {
            OnDayNightChanged?.Invoke(isDay);
        }
    }
}
