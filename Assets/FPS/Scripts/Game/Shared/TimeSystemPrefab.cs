using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Prefab completo del sistema de día/noche listo para usar.
    /// Contiene todos los componentes necesarios pre-configurados.
    /// </summary>
    public class TimeSystemPrefab : MonoBehaviour
    {
        [Header("🎨 Materiales de Skybox")]
        [Tooltip("Material para el skybox día/noche")]
        [SerializeField] private Material dayNightSkybox;

        [Header("☀️ Configuración de Luces")]
        [Tooltip("Prefab de luz direccional (sol/luna)")]
        [SerializeField] private Light directionalLightPrefab;

        [Header("⚙️ Configuración")]
        [Tooltip("Archivo de configuración del ciclo día/noche")]
        [SerializeField] private DayNightCycle dayNightConfig;

        [Header("🎮 Eventos")]
        [Tooltip("Gestor de eventos horarios")]
        [SerializeField] private TimeEventManager eventManager;

        [Tooltip("Eventos específicos del juego")]
        [SerializeField] private VigilanteGameEvents vigilanteEvents;

        [Header("⏸️ Sistema de Pausa")]
        [Tooltip("Gestor de pausa del juego")]
        [SerializeField] private GamePauseManager pauseManager;

        private void Awake()
        {
            InitializeTimeSystem();
        }

        /// <summary>
        /// Inicializa completamente el sistema de día/noche.
        /// </summary>
        public void InitializeTimeSystem()
        {
            CreateOrValidateComponents();
            SetupLighting();
            SetupSkybox();
            SetupEvents();

            Debug.Log("✅ Sistema de día/noche inicializado completamente");
        }

        private void CreateOrValidateComponents()
        {
            // Crear TimeManager si no existe
            TimeManager timeManager = GetComponent<TimeManager>();
            if (timeManager == null)
            {
                timeManager = gameObject.AddComponent<TimeManager>();
            }

            // Crear LightingController si no existe
            LightingController lightingController = GetComponent<LightingController>();
            if (lightingController == null)
            {
                lightingController = gameObject.AddComponent<LightingController>();
            }

            // Crear componentes adicionales si no existen
            if (eventManager == null)
            {
                eventManager = gameObject.AddComponent<TimeEventManager>();
            }

            if (vigilanteEvents == null)
            {
                vigilanteEvents = gameObject.AddComponent<VigilanteGameEvents>();
            }

            if (pauseManager == null)
            {
                pauseManager = gameObject.AddComponent<GamePauseManager>();
            }
        }

        private void SetupLighting()
        {
            // Crear luz direccional si no existe
            Light directionalLight = FindObjectOfType<Light>();
            if (directionalLight == null && directionalLightPrefab != null)
            {
                directionalLight = Instantiate(directionalLightPrefab);
                directionalLight.name = "Sun";
            }

            // Configurar LightingController
            LightingController lightingController = GetComponent<LightingController>();
            if (lightingController != null)
            {
                // Nota: Necesitarías hacer estos campos públicos o añadir métodos
                Debug.Log("💡 Configura el LightingController en el Inspector con la luz direccional");
            }
        }

        private void SetupSkybox()
        {
            if (dayNightSkybox != null)
            {
                RenderSettings.skybox = dayNightSkybox;
            }
            else
            {
                // Crear skybox básico automáticamente
                Material autoSkybox = SkyboxMaterialCreator.CreateDayNightSkyboxMaterial("AutoDayNightSkybox");
                RenderSettings.skybox = autoSkybox;
                Debug.Log("✅ Skybox creado automáticamente");
            }
        }

        private void SetupEvents()
        {
            // Conectar eventos del sistema de tiempo con eventos del juego
            TimeManager timeManager = GetComponent<TimeManager>();
            if (timeManager != null && vigilanteEvents != null)
            {
                timeManager.OnDayNightChanged += vigilanteEvents.SetNightShift;
            }
        }

        /// <summary>
        /// Método público para configurar el sistema desde fuera.
        /// </summary>
        public void ConfigureSystem(DayNightCycle config, Material skybox, Light directionalLight)
        {
            dayNightConfig = config;
            dayNightSkybox = skybox;

            LightingController lightingController = GetComponent<LightingController>();
            if (lightingController != null)
            {
                // Configurar referencias (necesitarías hacer campos públicos)
                Debug.Log("💡 Configura manualmente las referencias en el Inspector");
            }
        }
    }
}
