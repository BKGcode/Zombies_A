using UnityEngine;

namespace FPS.Game.Shared
{
    [CreateAssetMenu(fileName = "DayNightProfile", menuName = "FPS/Game/Day-Night Profile")]
    public class DayNightProfile : ScriptableObject
    {
        [Header("Colores del Cielo de Día")]
        public Color dayTopColor = new Color(0.8f, 0.9f, 1f);
        public Color dayHorizonColor = new Color(0.47f, 0.76f, 1f);
        public Color dayBottomColor = new Color(0.3f, 0.5f, 0.8f);

        [Header("Colores del Cielo de Noche")]
        public Color nightTopColor = new Color(0.02f, 0.02f, 0.08f);
        public Color nightHorizonColor = new Color(0.05f, 0.05f, 0.15f);
        public Color nightBottomColor = new Color(0.02f, 0.02f, 0.05f);
    }
}
