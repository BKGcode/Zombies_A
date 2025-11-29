using UnityEngine;
using System.Collections;

namespace FPS.Game.Shared
{
    /// <summary>
    /// EJEMPLO DE INTEGRACIÓN - ADAPTA A TU SISTEMA ESPECÍFICO
    /// Este script muestra CÓMO integrar el sistema de día/noche con enemigos, luces y audio.
    /// Los métodos específicos (cómo se modifica un enemigo) dependen de tu propia implementación.
    /// </summary>
    public class TimeEventExample : MonoBehaviour
    {
        [Header("🤖 Control de Enemigos")]
        [Tooltip("Lista de GameObjects de enemigos que cambiarán su comportamiento según la hora.")]
        [SerializeField] private GameObject[] enemyReferences;

        [Header("💡 Control de Luces Ambientales")]
        [Tooltip("Luces adicionales que se encienden/apagan o cambian de intensidad según la hora.")]
        [SerializeField] private Light[] ambientLights;

        [Header("🎵 Control de Audio")]
        [Tooltip("Fuentes de audio ambiental que cambian de volumen o clip según la hora.")]
        [SerializeField] private AudioSource[] ambientAudioSources;

        [Header("⚙️ Configuración de Comportamiento")]
        [Tooltip("Multiplicador de velocidad de enemigos durante el día.")]
        [Range(0.1f, 3f)]
        [SerializeField] private float daySpeedMultiplier = 1f;

        [Tooltip("Multiplicador de velocidad de enemigos durante la noche.")]
        [Range(0.1f, 3f)]
        [SerializeField] private float nightSpeedMultiplier = 1.5f;

        [Tooltip("Multiplicador de daño de enemigos durante la noche.")]
        [Range(0.5f, 3f)]
        [SerializeField] private float nightDamageMultiplier = 1.2f;

        // Estado interno
        private TimeManager timeManager;

        #region Unity Lifecycle

        private void Awake()
        {
            timeManager = TimeManager.Instance;
        }

        private void Start()
        {
            SubscribeToTimeEvents();
            // Forzar actualización inicial
            OnDayNightChanged(timeManager.IsDay());
        }

        private void OnDestroy()
        {
            UnsubscribeFromTimeEvents();
        }

        #endregion

        #region Eventos de Tiempo

        private void SubscribeToTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged += OnDayNightChanged;
                timeManager.OnHourChanged += OnHourChanged;
            }
        }

        private void UnsubscribeFromTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged -= OnDayNightChanged;
                timeManager.OnHourChanged -= OnHourChanged;
            }
        }

        private void OnDayNightChanged(bool isDay)
        {
            Debug.Log($"[TimeEventExample] Ha cambiado el periodo. Es de día: {isDay}");
            UpdateEnemyBehavior();
            UpdateAmbientLighting();
            UpdateAmbientAudio();
        }

        private void OnHourChanged(float hour)
        {
            HandleSpecificHourEvents(hour);
        }

        #endregion

        #region Comportamiento de Enemigos

        private void UpdateEnemyBehavior()
        {
            if (enemyReferences == null || timeManager == null) return;

            bool isNight = timeManager.IsNight();
            float speedMultiplier = isNight ? nightSpeedMultiplier : daySpeedMultiplier;
            float damageMultiplier = isNight ? nightDamageMultiplier : 1f; // Daño normal de día

            foreach (var enemyObject in enemyReferences)
            {
                if (enemyObject != null)
                {
                    // --- EJEMPLO DE INTEGRACIÓN - ADAPTA ESTO A TU SISTEMA ---
                    // Debes obtener el componente de tu enemigo y llamar a sus métodos.
                    // Por ejemplo, si tu script de enemigo se llama "EnemyAI":
                    
                    /*
                    EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
                    if (enemyAI != null)
                    {
                        enemyAI.SetSpeedMultiplier(speedMultiplier);
                        enemyAI.SetDamageMultiplier(damageMultiplier);
                    }
                    */
                }
            }
        }

        #endregion

        #region Iluminación Ambiental

        private void UpdateAmbientLighting()
        {
            if (ambientLights == null || timeManager == null) return;

            bool isNight = timeManager.IsNight();
            // Ejemplo: luces encendidas de noche, apagadas de día
            float targetIntensity = isNight ? 1f : 0f; 

            foreach (Light light in ambientLights)
            {
                if (light != null)
                {
                    // Usamos una corutina para una transición suave
                    StartCoroutine(FadeLightIntensity(light, targetIntensity, 2f));
                }
            }
        }

        #endregion

        #region Audio Ambiental

        private void UpdateAmbientAudio()
        {
            if (ambientAudioSources == null || timeManager == null) return;

            bool isNight = timeManager.IsNight();

            foreach (AudioSource audio in ambientAudioSources)
            {
                if (audio != null)
                {
                    // Ejemplo: sonido de grillos de noche, pájaros de día
                    // Aquí podrías cambiar el audio.clip o simplemente el volumen.
                    audio.volume = isNight ? 0.7f : 0.4f;
                }
            }
        }

        #endregion

        #region Eventos por Hora Específica

        private void HandleSpecificHourEvents(float hour)
        {
            // Usamos un umbral pequeño para comparar floats
            if (Mathf.Abs(hour - 6f) < 0.01f) // 6:00 AM - Amanecer
            {
                Debug.Log("🌅 Amanecer: Los enemigos deberían volverse menos agresivos.");
            }
            else if (Mathf.Abs(hour - 18f) < 0.01f) // 6:00 PM - Atardecer
            {
                Debug.Log("🌙 Atardecer: Los enemigos deberían volverse más agresivos.");
            }
            else if (Mathf.Abs(hour - 0f) < 0.01f) // 12:00 AM - Medianoche
            {
                Debug.Log("🕛 Medianoche: Pico de actividad nocturna.");
            }
        }

        #endregion

        #region Utilidades

        private IEnumerator FadeLightIntensity(Light light, float targetIntensity, float duration)
        {
            float startIntensity = light.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
                yield return null;
            }

            light.intensity = targetIntensity;
        }

        #endregion
        
        /*
        ScriptRole: Example implementation for time-based events.
        RelatedScripts: TimeManager, EnemyAI (hypothetical).
        UsesSO: -
        ReceivesFrom: TimeManager (OnDayNightChanged, OnHourChanged).
        SendsTo: EnemyAI (hypothetical methods).
        Setup:
        - Attach to a manager GameObject in the scene (e.g., "EventManager").
        - Assign enemy GameObjects, lights, and audio sources in the Inspector.
        */
    }
}