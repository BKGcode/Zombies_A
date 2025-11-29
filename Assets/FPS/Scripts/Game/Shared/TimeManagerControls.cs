using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Controles visuales en el Inspector para manipular el TimeManager.
    /// Útil para pruebas y debugging rápido sin tocar código.
    /// </summary>
    [RequireComponent(typeof(TimeManager))]
    public class TimeManagerControls : MonoBehaviour
    {
        [Header("⚡ CONTROLES RÁPIDOS DE TESTING")]
        [Space(10)]
        
        [Header("📝 Instrucciones:")]
        [TextArea(3, 5)]
        [SerializeField] private string instructions = 
            "Usa los botones de contexto (click derecho) para:\n" +
            "• Saltar a horas específicas\n" +
            "• Cambiar velocidad del tiempo\n" +
            "• Pausar/Reanudar\n" +
            "• Resetear el ciclo";

        [Header("⏱️ Presets de Duración Rápida")]
        [Tooltip("Duración del ciclo completo en SEGUNDOS")]
        public PresetDuration[] durationPresets = new PresetDuration[]
        {
            new PresetDuration("⚡ Muy Rápido (30 seg)", 30f),
            new PresetDuration("🏃 Rápido (1 min)", 60f),
            new PresetDuration("🚶 Moderado (2 min)", 120f),
            new PresetDuration("🐢 Normal (5 min)", 300f),
            new PresetDuration("🕐 Lento (10 min)", 600f),
            new PresetDuration("🌍 Realista (2 horas)", 7200f)
        };

        [Header("⏰ Saltos de Hora Rápidos")]
        [Tooltip("Horas predefinidas para saltar rápidamente")]
        public HourPreset[] hourPresets = new HourPreset[]
        {
            new HourPreset("🌅 Amanecer", 6f),
            new HourPreset("☀️ Mediodía", 12f),
            new HourPreset("🌆 Atardecer", 18f),
            new HourPreset("🌙 Medianoche", 0f),
            new HourPreset("🌃 Madrugada", 3f)
        };

        [Header("⚡ Multiplicadores de Velocidad")]
        [Tooltip("Presets de velocidad del tiempo")]
        public SpeedPreset[] speedPresets = new SpeedPreset[]
        {
            new SpeedPreset("⏸️ Pausa", 0f),
            new SpeedPreset("🐌 Muy Lento", 0.25f),
            new SpeedPreset("🐢 Lento", 0.5f),
            new SpeedPreset("▶️ Normal", 1f),
            new SpeedPreset("⏩ Rápido", 2f),
            new SpeedPreset("⏩⏩ Muy Rápido", 5f),
            new SpeedPreset("⚡ Ultra Rápido", 10f),
            new SpeedPreset("🚀 Extremo", 50f)
        };

        private TimeManager timeManager;

        [System.Serializable]
        public struct PresetDuration
        {
            public string name;
            public float durationInSeconds;

            public PresetDuration(string name, float duration)
            {
                this.name = name;
                this.durationInSeconds = duration;
            }
        }

        [System.Serializable]
        public struct HourPreset
        {
            public string name;
            [Range(0f, 23.99f)]
            public float hour;

            public HourPreset(string name, float hour)
            {
                this.name = name;
                this.hour = hour;
            }
        }

        [System.Serializable]
        public struct SpeedPreset
        {
            public string name;
            [Range(0f, 100f)]
            public float multiplier;

            public SpeedPreset(string name, float multiplier)
            {
                this.name = name;
                this.multiplier = multiplier;
            }
        }

        private void Awake()
        {
            timeManager = GetComponent<TimeManager>();
        }

        #region Context Menu - Duración del Ciclo

        [ContextMenu("⚡ Ciclo MUY RÁPIDO (30 seg)")]
        private void SetVeryFastCycle() => ApplyDurationPreset(0);

        [ContextMenu("🏃 Ciclo RÁPIDO (1 min)")]
        private void SetFastCycle() => ApplyDurationPreset(1);

        [ContextMenu("🚶 Ciclo MODERADO (2 min)")]
        private void SetModerateCycle() => ApplyDurationPreset(2);

        [ContextMenu("🐢 Ciclo NORMAL (5 min)")]
        private void SetNormalCycle() => ApplyDurationPreset(3);

        [ContextMenu("🕐 Ciclo LENTO (10 min)")]
        private void SetSlowCycle() => ApplyDurationPreset(4);

        [ContextMenu("🌍 Ciclo REALISTA (2 horas)")]
        private void SetRealisticCycle() => ApplyDurationPreset(5);

        private void ApplyDurationPreset(int index)
        {
            if (timeManager == null || index < 0 || index >= durationPresets.Length) return;

            var preset = durationPresets[index];
            timeManager.SetCustomCycleDuration(preset.durationInSeconds);
            timeManager.SetUseCustomDuration(true);

            Debug.Log($"⏱️ Duración del ciclo establecida: {preset.name} ({preset.durationInSeconds} segundos)");
        }

        #endregion

        #region Context Menu - Saltos de Hora

        [ContextMenu("🌅 Saltar a AMANECER (6:00)")]
        private void JumpToSunrise() => ApplyHourPreset(0);

        [ContextMenu("☀️ Saltar a MEDIODÍA (12:00)")]
        private void JumpToNoon() => ApplyHourPreset(1);

        [ContextMenu("🌆 Saltar a ATARDECER (18:00)")]
        private void JumpToSunset() => ApplyHourPreset(2);

        [ContextMenu("🌙 Saltar a MEDIANOCHE (00:00)")]
        private void JumpToMidnight() => ApplyHourPreset(3);

        [ContextMenu("🌃 Saltar a MADRUGADA (03:00)")]
        private void JumpToLateNight() => ApplyHourPreset(4);

        private void ApplyHourPreset(int index)
        {
            if (timeManager == null || index < 0 || index >= hourPresets.Length) return;

            var preset = hourPresets[index];
            timeManager.SetGameHour(preset.hour);

            Debug.Log($"🕐 Hora establecida: {preset.name} ({preset.hour:F1}h / {timeManager.GetFormattedTime()})");
        }

        #endregion

        #region Context Menu - Velocidad del Tiempo

        [ContextMenu("⏸️ PAUSAR Tiempo")]
        private void PauseTime() => ApplySpeedPreset(0);

        [ContextMenu("🐌 Velocidad MUY LENTA (0.25x)")]
        private void SetVerySlowSpeed() => ApplySpeedPreset(1);

        [ContextMenu("🐢 Velocidad LENTA (0.5x)")]
        private void SetSlowSpeed() => ApplySpeedPreset(2);

        [ContextMenu("▶️ Velocidad NORMAL (1x)")]
        private void SetNormalSpeed() => ApplySpeedPreset(3);

        [ContextMenu("⏩ Velocidad RÁPIDA (2x)")]
        private void SetFastSpeed() => ApplySpeedPreset(4);

        [ContextMenu("⏩⏩ Velocidad MUY RÁPIDA (5x)")]
        private void SetVeryFastSpeed() => ApplySpeedPreset(5);

        [ContextMenu("⚡ Velocidad ULTRA RÁPIDA (10x)")]
        private void SetUltraFastSpeed() => ApplySpeedPreset(6);

        [ContextMenu("🚀 Velocidad EXTREMA (50x)")]
        private void SetExtremeSpeed() => ApplySpeedPreset(7);

        private void ApplySpeedPreset(int index)
        {
            if (timeManager == null || index < 0 || index >= speedPresets.Length) return;

            var preset = speedPresets[index];
            
            if (preset.multiplier == 0f)
            {
                timeManager.SetPaused(true);
                Debug.Log("⏸️ Tiempo PAUSADO");
            }
            else
            {
                timeManager.SetPaused(false);
                timeManager.SetTimeSpeedMultiplier(preset.multiplier);
                Debug.Log($"⚡ Velocidad establecida: {preset.name} ({preset.multiplier}x)");
            }
        }

        #endregion

        #region Context Menu - Utilidades

        [ContextMenu("🔄 RESETEAR Ciclo (volver a mediodía)")]
        private void ResetCycle()
        {
            if (timeManager == null) return;

            timeManager.SetGameHour(12f);
            timeManager.SetTimeSpeedMultiplier(1f);
            timeManager.SetPaused(false);

            Debug.Log("🔄 Ciclo reseteado: Mediodía, velocidad normal, sin pausa");
        }

        [ContextMenu("📊 Mostrar INFORMACIÓN del Sistema")]
        private void ShowSystemInfo()
        {
            if (timeManager == null) return;

            string info = $"📊 INFORMACIÓN DEL SISTEMA DE TIEMPO\n" +
                         $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                         $"Hora actual: {timeManager.GetFormattedTime()} ({timeManager.GetCurrentGameHour():F2}h)\n" +
                         $"Período: {(timeManager.IsDay() ? "☀️ Día" : "🌙 Noche")}\n" +
                         $"Progreso del período: {timeManager.GetCurrentPeriodProgress():P}\n" +
                         $"Progreso del ciclo: {timeManager.GetCurrentCycleTime():P}\n" +
                         $"Duración del ciclo: {timeManager.GetCurrentCycleDuration()}s ({timeManager.GetCurrentCycleDuration() / 60f:F1} min)\n" +
                         $"Velocidad: {timeManager.GetTimeSpeedMultiplier()}x\n" +
                         $"Pausado: {(Time.timeScale == 0f ? "Sí" : "No")}\n" +
                         $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

            Debug.Log(info);
        }

        [ContextMenu("⏭️ Avanzar 1 HORA")]
        private void AdvanceOneHour()
        {
            if (timeManager == null) return;
            timeManager.AdvanceTime(1f);
            Debug.Log($"⏭️ Avanzado 1 hora → Ahora: {timeManager.GetFormattedTime()}");
        }

        [ContextMenu("⏭️ Avanzar 6 HORAS")]
        private void AdvanceSixHours()
        {
            if (timeManager == null) return;
            timeManager.AdvanceTime(6f);
            Debug.Log($"⏭️ Avanzado 6 horas → Ahora: {timeManager.GetFormattedTime()}");
        }

        [ContextMenu("⏭️ Avanzar 12 HORAS (cambiar día/noche)")]
        private void AdvanceTwelveHours()
        {
            if (timeManager == null) return;
            timeManager.AdvanceTime(12f);
            Debug.Log($"⏭️ Avanzado 12 horas → Ahora: {timeManager.GetFormattedTime()} ({(timeManager.IsDay() ? "Día" : "Noche")})");
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Aplica un preset de duración por nombre.
        /// </summary>
        public void ApplyDurationPresetByName(string presetName)
        {
            for (int i = 0; i < durationPresets.Length; i++)
            {
                if (durationPresets[i].name.Contains(presetName))
                {
                    ApplyDurationPreset(i);
                    return;
                }
            }
            Debug.LogWarning($"Preset de duración '{presetName}' no encontrado.");
        }

        /// <summary>
        /// Aplica un preset de hora por nombre.
        /// </summary>
        public void ApplyHourPresetByName(string presetName)
        {
            for (int i = 0; i < hourPresets.Length; i++)
            {
                if (hourPresets[i].name.Contains(presetName))
                {
                    ApplyHourPreset(i);
                    return;
                }
            }
            Debug.LogWarning($"Preset de hora '{presetName}' no encontrado.");
        }

        /// <summary>
        /// Aplica un preset de velocidad por nombre.
        /// </summary>
        public void ApplySpeedPresetByName(string presetName)
        {
            for (int i = 0; i < speedPresets.Length; i++)
            {
                if (speedPresets[i].name.Contains(presetName))
                {
                    ApplySpeedPreset(i);
                    return;
                }
            }
            Debug.LogWarning($"Preset de velocidad '{presetName}' no encontrado.");
        }

        #endregion
    }
}
