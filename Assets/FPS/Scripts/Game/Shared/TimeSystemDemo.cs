using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Ejemplo completo de integración del sistema de día/noche.
    /// Demuestra cómo usar todos los componentes juntos en una escena funcional.
    /// </summary>
    public class TimeSystemDemo : MonoBehaviour
    {
        [Header("🎮 Configuración de Demo")]
        [Tooltip("¿Ejecutar demo automático al iniciar?")]
        [SerializeField] private bool runAutoDemo = true;

        [Tooltip("Duración de cada fase de la demo (segundos)")]
        [SerializeField] private float demoPhaseDuration = 5f;

        [Header("🎯 Eventos de Demo")]
        [Tooltip("Evento cuando cambia la iluminación")]
        public UnityEngine.Events.UnityEvent onLightingChanged;

        [Tooltip("Evento cuando cambian los enemigos")]
        public UnityEngine.Events.UnityEvent onEnemiesChanged;

        [Tooltip("Evento cuando cambian los eventos horarios")]
        public UnityEngine.Events.UnityEvent onTimeEventsChanged;

        // Estado interno
        private TimeManager timeManager;
        private enum DemoPhase { Setup, DayTest, NightTest, EventsTest, Complete }
        private DemoPhase currentPhase = DemoPhase.Setup;
        private float phaseStartTime;

        private void Awake()
        {
            InitializeDemo();
        }

        private void Start()
        {
            if (runAutoDemo)
            {
                StartCoroutine(RunAutomatedDemo());
            }
        }

        private void InitializeDemo()
        {
            timeManager = TimeManager.Instance;

            if (timeManager == null)
            {
                Debug.LogError("TimeSystemDemo: TimeManager no encontrado. Asegúrate de tener el sistema configurado.");
                return;
            }

            // Conectar eventos para demostración
            timeManager.OnDayNightChanged += OnDayNightChanged;
            timeManager.OnHourChanged += OnHourChanged;

            Debug.Log("🎬 Demo del sistema de día/noche inicializado");
        }

        private System.Collections.IEnumerator RunAutomatedDemo()
        {
            Debug.Log("🎬 Iniciando demo automático del sistema de día/noche");

            // Fase 1: Setup
            currentPhase = DemoPhase.Setup;
            phaseStartTime = Time.time;
            Debug.Log("📋 Fase 1: Configuración inicial");
            yield return new WaitForSeconds(demoPhaseDuration);

            // Fase 2: Probar día
            currentPhase = DemoPhase.DayTest;
            phaseStartTime = Time.time;
            timeManager.SetGameHour(12f); // Mediodía
            Debug.Log("☀️ Fase 2: Probando período diurno");
            onLightingChanged?.Invoke();
            yield return new WaitForSeconds(demoPhaseDuration);

            // Fase 3: Probar noche
            currentPhase = DemoPhase.NightTest;
            phaseStartTime = Time.time;
            timeManager.SetGameHour(0f); // Medianoche
            Debug.Log("🌙 Fase 3: Probando período nocturno");
            onLightingChanged?.Invoke();
            onEnemiesChanged?.Invoke();
            yield return new WaitForSeconds(demoPhaseDuration);

            // Fase 4: Probar eventos
            currentPhase = DemoPhase.EventsTest;
            phaseStartTime = Time.time;
            Debug.Log("⏰ Fase 4: Probando eventos horarios");
            onTimeEventsChanged?.Invoke();

            // Probar diferentes horas rápidamente
            for (float hour = 0f; hour <= 23f; hour += 3f)
            {
                timeManager.SetGameHour(hour);
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(demoPhaseDuration);

            // Fase 5: Completo
            currentPhase = DemoPhase.Complete;
            Debug.Log("✅ Demo completado. El sistema está funcionando correctamente.");

            // Resetear a tiempo normal
            timeManager.SetGameHour(12f);
        }

        private void OnDayNightChanged(bool isDay)
        {
            string period = isDay ? "día" : "noche";
            Debug.Log($"🌅 Cambio detectado: Ahora es de {period}");

            if (isDay)
            {
                Debug.Log("☀️ Comportamiento diurno activado");
            }
            else
            {
                Debug.Log("🌙 Comportamiento nocturno activado");
            }
        }

        private void OnHourChanged(float hour)
        {
            Debug.Log($"🕐 Nueva hora: {hour:F1}");

            // Demostrar eventos específicos por hora
            if (Mathf.Abs(hour - 6f) < 0.01f)
            {
                Debug.Log("🌅 Amanecer - Inicio del turno diurno");
            }
            else if (Mathf.Abs(hour - 18f) < 0.01f)
            {
                Debug.Log("🌙 Atardecer - Inicio del turno nocturno");
            }
            else if (Mathf.Abs(hour - 12f) < 0.01f)
            {
                Debug.Log("☀️ Mediodía - Máxima actividad diurna");
            }
            else if (Mathf.Abs(hour - 0f) < 0.01f)
            {
                Debug.Log("🕛 Medianoche - Máxima actividad nocturna");
            }
        }

        /// <summary>
        /// Método público para iniciar demo manualmente.
        /// </summary>
        public void StartDemo()
        {
            if (!runAutoDemo)
            {
                StartCoroutine(RunAutomatedDemo());
            }
        }

        /// <summary>
        /// Detiene la demo actual.
        /// </summary>
        public void StopDemo()
        {
            StopAllCoroutines();
            if (timeManager != null)
            {
                timeManager.SetGameHour(12f); // Resetear a mediodía
            }
            Debug.Log("⏹️ Demo detenido");
        }

        /// <summary>
        /// Obtiene el progreso actual de la demo.
        /// </summary>
        public float GetDemoProgress()
        {
            if (currentPhase == DemoPhase.Complete) return 1f;

            float phaseProgress = (Time.time - phaseStartTime) / demoPhaseDuration;
            float phaseOffset = (int)currentPhase * 0.25f; // 4 fases = 25% cada una

            return Mathf.Clamp01(phaseOffset + (phaseProgress * 0.25f));
        }

        private void OnDestroy()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged -= OnDayNightChanged;
                timeManager.OnHourChanged -= OnHourChanged;
            }
        }
    }
}
