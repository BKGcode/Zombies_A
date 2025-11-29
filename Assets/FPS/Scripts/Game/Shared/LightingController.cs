using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Controlador de iluminación que maneja el ciclo día/noche visual.
    /// Gestiona skybox, luz direccional y ambiente según el tiempo del juego.
    /// </summary>
    [RequireComponent(typeof(TimeManager))]
    [DefaultExecutionOrder(100)] // Ejecutar después del TimeManager
    public class LightingController : MonoBehaviour
    {
        [Header("🌌 Referencias de Skybox")]
        [Tooltip("Material del skybox que se modificará dinámicamente")]
        [SerializeField] public Material skyboxMaterial;

        [Header("☀️ Referencias de Luces")]
        [Tooltip("Luz direccional principal (sol/luna)")]
        [SerializeField] public Light directionalLight;

        [Header("⚙️ Configuración")]
        [Tooltip("ScriptableObject con configuración del ciclo día/noche")]
        [SerializeField] public DayNightCycle dayNightConfig;

        [Header("🌞 Configuración del Arco Solar")]
        [Tooltip("Ángulo máximo del sol al mediodía (90° = vertical, 60° = inclinado)")]
        [Range(30f, 90f)]
        [SerializeField] private float maxSunElevation = 60f;

        [Tooltip("Qué tan bajo pasa el sol por debajo del horizonte durante la noche")]
        [Range(30f, 90f)]
        [SerializeField] private float sunDepthBelowHorizon = 60f;

        [Tooltip("Rotación adicional en Y para ajustar la dirección este-oeste")]
        [Range(-180f, 180f)]
        [SerializeField] private float sunPathRotationOffset = 0f;

        [Header("📊 Debug Info (Solo lectura)")]
        [SerializeField] private float currentSunElevation = 0f;
        [SerializeField] private float currentSunAzimuth = 0f;
        [SerializeField] private string sunPosition = "Este";

        // Componentes cacheados
        private TimeManager timeManager;

        // Estado interno
        private bool isDay = true;
        private float transitionProgress = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateReferences();
            SubscribeToTimeEvents();
        }

        private void Start()
        {
            InitializeLighting();
        }

        private void Update()
        {
            if (dayNightConfig == null || timeManager == null) return;

            UpdateLighting();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTimeEvents();
        }

        #endregion

        #region Inicialización

        private void CacheComponents()
        {
            timeManager = GetComponent<TimeManager>();
            if (timeManager == null)
            {
                timeManager = TimeManager.Instance;
            }
        }

        private void ValidateReferences()
        {
            if (dayNightConfig == null)
            {
                Debug.LogError("LightingController: DayNightCycle no asignado.");
                enabled = false;
                return;
            }

            if (directionalLight == null)
            {
                directionalLight = FindObjectOfType<Light>();
                if (directionalLight == null)
                {
                    Debug.LogWarning("LightingController: No se encontró luz direccional. Crea una en la escena.");
                }
            }

            if (skyboxMaterial == null && RenderSettings.skybox != null)
            {
                skyboxMaterial = RenderSettings.skybox;
            }
        }

        private void SubscribeToTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged += OnDayNightChanged;
                timeManager.OnCycleTimeChanged += OnCycleTimeChanged;
            }
        }

        private void UnsubscribeFromTimeEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnDayNightChanged -= OnDayNightChanged;
                timeManager.OnCycleTimeChanged -= OnCycleTimeChanged;
            }
        }

        private void InitializeLighting()
        {
            if (timeManager == null) return;

            isDay = timeManager.IsDay();
            transitionProgress = 0f;

            // Verificar compatibilidad del skybox antes de aplicar
            if (skyboxMaterial != null)
            {
                string shaderName = skyboxMaterial.shader.name;
                Debug.Log($"LightingController: Skybox detectado con shader '{shaderName}'");
            }

            // Aplicar estado inicial
            UpdateDirectionalLight();
            UpdateSkybox();
            UpdateAmbientLight();
        }

        #endregion

        #region Eventos de Tiempo

        private void OnDayNightChanged(bool day)
        {
            isDay = day;
            transitionProgress = 0f;

            // Actualizar inmediatamente para cambios bruscos si es necesario
            UpdateDirectionalLight();
            UpdateSkybox();
            UpdateAmbientLight();
        }

        private void OnCycleTimeChanged(float cycleTime)
        {
            // Actualización continua para transiciones suaves
            UpdateLighting();
        }

        #endregion

        #region Actualización de Iluminación

        private void UpdateLighting()
        {
            UpdateDirectionalLight();
            UpdateSkybox();
            UpdateAmbientLight();
            UpdateTransitionProgress();
        }

        private void UpdateDirectionalLight()
        {
            if (directionalLight == null) return;

            // Obtener el tiempo normalizado del ciclo completo (0-1)
            float cycleTime = timeManager.GetCurrentCycleTime();

            // Convertir el ciclo (0-1) a un ángulo completo (0-360°)
            // 0.0 = Amanecer (Este, 0°)
            // 0.25 = Mediodía (Sur, 90°)
            // 0.5 = Atardecer (Oeste, 180°)
            // 0.75 = Medianoche (Norte, 270°)
            // 1.0 = Amanecer de nuevo (Este, 360°/0°)
            float sunAngle = cycleTime * 360f;

            // Calcular rotación X (elevación): arco de este a oeste
            // Durante el día (0-0.5): sol sube y baja en arco visible
            // Durante la noche (0.5-1.0): sol pasa por debajo del horizonte
            float rotationX;
            
            if (cycleTime < 0.5f) // DÍA - Sol visible
            {
                // Progreso del día (0 a 1)
                float dayProgress = cycleTime * 2f;
                
                // Arco parabólico: empieza en horizonte (0°), sube a cenit, baja a horizonte
                // Usar una parábola invertida: máximo en el medio
                rotationX = Mathf.Sin(dayProgress * Mathf.PI) * maxSunElevation;
            }
            else // NOCHE - Sol por debajo del horizonte
            {
                // Progreso de la noche (0 a 1)
                float nightProgress = (cycleTime - 0.5f) * 2f;
                
                // El sol pasa por debajo: de 0° a -depth° y vuelve a 0°
                rotationX = -Mathf.Sin(nightProgress * Mathf.PI) * sunDepthBelowHorizon;
            }

            // Calcular rotación Y (azimut): movimiento horizontal de este a oeste
            // 0° = Este, 90° = Sur, 180° = Oeste, 270° = Norte, 360° = Este
            float rotationY = sunAngle + sunPathRotationOffset;

            // Aplicar la rotación completa
            // X = Elevación (arriba/abajo)
            // Y = Azimut (este/oeste/norte/sur)
            // Z = 0 (sin inclinación lateral)
            directionalLight.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

            // Interpolar intensidad y color basado en si es día o noche
            bool isCurrentlyDay = timeManager.IsDay();
            float targetIntensity = isCurrentlyDay ? dayNightConfig.dayLightIntensity : dayNightConfig.nightLightIntensity;
            Color targetColor = isCurrentlyDay ? dayNightConfig.dayLightColor : dayNightConfig.nightLightColor;

            // Suavizado de transición
            float currentIntensity = Mathf.Lerp(directionalLight.intensity, targetIntensity, Time.deltaTime * 2f);
            Color currentColor = Color.Lerp(directionalLight.color, targetColor, Time.deltaTime * 2f);

            directionalLight.intensity = currentIntensity;
            directionalLight.color = currentColor;

            // Actualizar info de debug
            UpdateDebugInfo(rotationX, rotationY, cycleTime);
        }

        private void UpdateDebugInfo(float elevationAngle, float azimuthAngle, float cycleTime)
        {
            currentSunElevation = elevationAngle;
            currentSunAzimuth = azimuthAngle % 360f;

            // Determinar posición cardinal del sol
            float normalizedAzimuth = currentSunAzimuth;
            if (normalizedAzimuth < 0) normalizedAzimuth += 360f;

            if (normalizedAzimuth >= 0 && normalizedAzimuth < 45)
                sunPosition = "🌅 Este";
            else if (normalizedAzimuth >= 45 && normalizedAzimuth < 135)
                sunPosition = "☀️ Sur (Mediodía)";
            else if (normalizedAzimuth >= 135 && normalizedAzimuth < 225)
                sunPosition = "🌇 Oeste";
            else if (normalizedAzimuth >= 225 && normalizedAzimuth < 315)
                sunPosition = "🌙 Norte (Medianoche)";
            else
                sunPosition = "🌅 Este";

            // Añadir info de si está sobre o bajo el horizonte
            if (currentSunElevation < 0)
                sunPosition += " (Bajo horizonte)";
        }

        private void UpdateSkybox()
        {
            if (skyboxMaterial == null) return;

            // Determinar el shader del skybox y actualizar las propiedades correspondientes
            Color targetColor = isDay ? dayNightConfig.daySkyColor : dayNightConfig.nightSkyColor;

            // Skybox/Procedural (shader por defecto de Unity)
            if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                Color currentTint = skyboxMaterial.GetColor("_SkyTint");
                Color newTint = Color.Lerp(currentTint, targetColor, Time.deltaTime * 2f);
                skyboxMaterial.SetColor("_SkyTint", newTint);

                // Ajustar exposición según día/noche
                float targetExposure = isDay ? 1.3f : 0.8f;
                if (skyboxMaterial.HasProperty("_Exposure"))
                {
                    float currentExposure = skyboxMaterial.GetFloat("_Exposure");
                    float newExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);
                    skyboxMaterial.SetFloat("_Exposure", newExposure);
                }

                // Ajustar intensidad atmosférica
                if (skyboxMaterial.HasProperty("_AtmosphereThickness"))
                {
                    float targetThickness = isDay ? 1.0f : 0.5f;
                    float currentThickness = skyboxMaterial.GetFloat("_AtmosphereThickness");
                    float newThickness = Mathf.Lerp(currentThickness, targetThickness, Time.deltaTime * 2f);
                    skyboxMaterial.SetFloat("_AtmosphereThickness", newThickness);
                }
            }
            // Skybox/6 Sided
            else if (skyboxMaterial.HasProperty("_Tint"))
            {
                Color currentTint = skyboxMaterial.GetColor("_Tint");
                Color newTint = Color.Lerp(currentTint, targetColor, Time.deltaTime * 2f);
                skyboxMaterial.SetColor("_Tint", newTint);

                if (skyboxMaterial.HasProperty("_Exposure"))
                {
                    float targetExposure = isDay ? 1.0f : 0.5f;
                    float currentExposure = skyboxMaterial.GetFloat("_Exposure");
                    float newExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);
                    skyboxMaterial.SetFloat("_Exposure", newExposure);
                }
            }
            // Skybox/Cubemap
            else if (skyboxMaterial.HasProperty("_Tint"))
            {
                Color currentTint = skyboxMaterial.GetColor("_Tint");
                Color newTint = Color.Lerp(currentTint, targetColor, Time.deltaTime * 2f);
                skyboxMaterial.SetColor("_Tint", newTint);
            }
            // Shader personalizado con _SkyColor
            else if (skyboxMaterial.HasProperty("_SkyColor"))
            {
                Color currentSkyColor = skyboxMaterial.GetColor("_SkyColor");
                Color newSkyColor = Color.Lerp(currentSkyColor, targetColor, Time.deltaTime * 2f);
                skyboxMaterial.SetColor("_SkyColor", newSkyColor);
            }
            else
            {
                // Fallback: intentar modificar el color del ambiente si no hay propiedades de skybox
                Debug.LogWarning($"LightingController: El shader '{skyboxMaterial.shader.name}' no tiene propiedades compatibles conocidas. Solo se actualizará la luz ambiente.");
            }
        }

        private void UpdateAmbientLight()
        {
            if (directionalLight == null) return;

            // Calcular intensidad ambiente basada en la luz direccional
            float baseIntensity = directionalLight.intensity;
            float ambientIntensity = Mathf.Lerp(
                dayNightConfig.minAmbientIntensity,
                dayNightConfig.maxAmbientIntensity,
                baseIntensity / Mathf.Max(dayNightConfig.dayLightIntensity, dayNightConfig.nightLightIntensity)
            );

            // Color ambiente basado en la luz direccional
            Color ambientColor = Color.Lerp(Color.black, directionalLight.color, ambientIntensity);

            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;
        }

        private void UpdateTransitionProgress()
        {
            // Actualizar progreso de transición para efectos adicionales si es necesario
            transitionProgress = Mathf.Clamp01(transitionProgress + Time.deltaTime);
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Fuerza una actualización inmediata de toda la iluminación.
        /// Útil para cambios bruscos o inicialización.
        /// </summary>
        public void ForceLightingUpdate()
        {
            if (timeManager == null) return;

            isDay = timeManager.IsDay();
            UpdateDirectionalLight();
            UpdateSkybox();
            UpdateAmbientLight();
        }

        /// <summary>
        /// Obtiene el progreso actual de la transición día/noche (0-1).
        /// </summary>
        public float GetTransitionProgress()
        {
            return transitionProgress;
        }

        /// <summary>
        /// Verifica si el skybox actual es compatible con el sistema.
        /// </summary>
        public bool IsSkyboxCompatible()
        {
            if (skyboxMaterial == null) return false;

            return skyboxMaterial.HasProperty("_SkyTint") ||
                   skyboxMaterial.HasProperty("_Tint") ||
                   skyboxMaterial.HasProperty("_SkyColor") ||
                   skyboxMaterial.HasProperty("_Exposure");
        }

        /// <summary>
        /// Obtiene información sobre el skybox actual.
        /// </summary>
        public string GetSkyboxInfo()
        {
            if (skyboxMaterial == null) return "Sin skybox asignado";

            string shaderName = skyboxMaterial.shader.name;
            bool compatible = IsSkyboxCompatible();
            
            string info = $"Shader: {shaderName}\n";
            info += $"Compatible: {(compatible ? "✅ Sí" : "⚠️ No")}\n";
            
            if (skyboxMaterial.HasProperty("_SkyTint"))
                info += "Propiedades: Skybox/Procedural (_SkyTint, _Exposure, _AtmosphereThickness)\n";
            else if (skyboxMaterial.HasProperty("_Tint"))
                info += "Propiedades: Skybox/6 Sided o Cubemap (_Tint, _Exposure)\n";
            else if (skyboxMaterial.HasProperty("_SkyColor"))
                info += "Propiedades: Shader personalizado (_SkyColor)\n";
            
            return info;
        }

        #endregion
    }
}
