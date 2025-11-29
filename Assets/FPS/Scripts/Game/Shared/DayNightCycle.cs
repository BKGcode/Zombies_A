using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// ScriptableObject que define la configuración completa del ciclo día/noche.
    /// Centraliza todos los parámetros del sistema de tiempo para facilitar ajustes.
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Game/Day Night Cycle", fileName = "DayNightCycle")]
    public class DayNightCycle : ScriptableObject
    {
        [Header("⏰ Configuración de Tiempo")]
        [Tooltip("Duración en segundos de cada ciclo completo (día + noche)")]
        [Min(1f)]
        public float cycleDurationSeconds = 7200f; // 2 horas reales = 7200 segundos

        [Tooltip("Porcentaje del ciclo dedicado al día (0-1)")]
        [Range(0f, 1f)]
        public float dayPercentage = 0.5f;

        [Tooltip("Porcentaje del ciclo dedicado a la noche (0-1)")]
        [Range(0f, 1f)]
        public float nightPercentage = 0.5f;

        [Header("🌅 Configuración Día")]
        [Tooltip("Color del skybox durante el día")]
        public Color daySkyColor = new Color(0.47f, 0.76f, 1f); // Azul cielo claro

        [Tooltip("Intensidad de la luz direccional durante el día")]
        [Range(0f, 2f)]
        public float dayLightIntensity = 1.2f;

        [Tooltip("Color de la luz direccional durante el día")]
        public Color dayLightColor = Color.white;

        [Tooltip("Rotación Y del sol durante el día (arco solar)")]
        [Range(-90f, 270f)]
        public float daySunRotationY = 45f;

        [Header("🌙 Configuración Noche")]
        [Tooltip("Color del skybox durante la noche")]
        public Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f); // Azul oscuro

        [Tooltip("Intensidad de la luz direccional durante la noche")]
        [Range(0f, 2f)]
        public float nightLightIntensity = 0.3f;

        [Tooltip("Color de la luz direccional durante la noche (tinte lunar)")]
        public Color nightLightColor = new Color(0.8f, 0.9f, 1f); // Azul blanquecino

        [Tooltip("Rotación Y de la luna durante la noche")]
        [Range(-90f, 270f)]
        public float nightMoonRotationY = 225f;

        [Header("🌅 Transiciones")]
        [Tooltip("Suavizado de transiciones entre día y noche (0-1)")]
        [Range(0f, 1f)]
        public float transitionSmoothness = 0.1f;

        [Header("💡 Configuración Ambiente")]
        [Tooltip("Intensidad mínima de luz ambiente")]
        [Range(0f, 1f)]
        public float minAmbientIntensity = 0.2f;

        [Tooltip("Intensidad máxima de luz ambiente")]
        [Range(0f, 2f)]
        public float maxAmbientIntensity = 0.8f;

        [Header("🕐 Eventos por Hora")]
        [Tooltip("Lista de horas específicas que generan eventos (0-23)")]
        public int[] eventHours = { 6, 12, 18, 0 };

        [Header("🔧 Configuración Técnica")]
        [Tooltip("Actualizar el sistema cada X segundos")]
        [Min(0.1f)]
        public float updateInterval = 1f;

        // Propiedades calculadas
        public float DayDuration => cycleDurationSeconds * dayPercentage;
        public float NightDuration => cycleDurationSeconds * nightPercentage;
        public float DayStartTime => 0f;
        public float NightStartTime => DayDuration;

        private void OnValidate()
        {
            // Asegurar que día + noche = ciclo completo
            float totalPercentage = dayPercentage + nightPercentage;
            if (Mathf.Abs(totalPercentage - 1f) > 0.01f)
            {
                dayPercentage = 0.5f;
                nightPercentage = 0.5f;
                Debug.LogWarning("DayNightCycle: Porcentajes ajustados automáticamente a 50/50 para completar el ciclo");
            }
        }
    }
}
