using UnityEditor;
using UnityEngine;
using System.IO;

namespace FPS.Game.Shared.Editor
{
    public class DayNightSetupWindow : EditorWindow
    {
        private DayNightCycle dayNightConfig;
        private DayNightProfile dayNightProfile;
        private Material skyboxMaterial;

        [MenuItem("Tools/Sistema Día-Noche/Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<DayNightSetupWindow>("Day-Night Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Asistente de Configuración del Sistema Día/Noche", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Section for assets
            EditorGUILayout.LabelField("1. Configuración de Assets", EditorStyles.boldLabel);
            dayNightConfig = (DayNightCycle)EditorGUILayout.ObjectField("DayNightCycle Config", dayNightConfig, typeof(DayNightCycle), false);
            dayNightProfile = (DayNightProfile)EditorGUILayout.ObjectField("DayNight Profile", dayNightProfile, typeof(DayNightProfile), false);
            skyboxMaterial = (Material)EditorGUILayout.ObjectField("Skybox Material", skyboxMaterial, typeof(Material), false);

            if (GUILayout.Button("Crear Assets Faltantes"))
            {
                CreateMissingAssets();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Asigna los assets o créalos. El 'Skybox Material' debe usar el shader 'Skybox/6 Sided'.", MessageType.Info);
            EditorGUILayout.Space();


            // Section for scene setup
            EditorGUILayout.LabelField("2. Configuración de la Escena", EditorStyles.boldLabel);
            if (GUILayout.Button("Configurar Escena Actual"))
            {
                SetupScene();
            }
            EditorGUILayout.HelpBox("Esto creará un objeto '[TimeSystem]' y le asignará los componentes y assets necesarios.", MessageType.Info);

        }

        private void CreateMissingAssets()
        {
            if (dayNightConfig == null)
            {
                dayNightConfig = CreateAsset<DayNightCycle>("Assets/FPS/Game/Shared", "DefaultDayNightCycle.asset");
            }

            if (dayNightProfile == null)
            {
                dayNightProfile = CreateAsset<DayNightProfile>("Assets/FPS/Game/Shared", "DefaultDayNightProfile.asset");
            }

            if (skyboxMaterial == null)
            {
                skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"));
                AssetDatabase.CreateAsset(skyboxMaterial, "Assets/FPS/Game/Shared/AutoDayNightSkybox.mat");
                Debug.Log("Creado material de Skybox en 'Assets/FPS/Game/Shared/AutoDayNightSkybox.mat'");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private T CreateAsset<T>(string path, string name) where T : ScriptableObject
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            T asset = ScriptableObject.CreateInstance<T>();
            string assetPath = Path.Combine(path, name);
            AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log($"Creado asset en: {assetPath}");
            return asset;
        }

        private void SetupScene()
        {
            if (dayNightConfig == null || dayNightProfile == null || skyboxMaterial == null)
            {
                EditorUtility.DisplayDialog("Error", "Por favor, asigna o crea todos los assets necesarios antes de configurar la escena.", "OK");
                return;
            }

            // Create TimeSystem GameObject
            GameObject timeSystemGO = new GameObject("[TimeSystem]");

            // Add and configure TimeManager
            TimeManager timeManager = timeSystemGO.AddComponent<TimeManager>();
            timeManager.dayNightConfig = dayNightConfig;

            // Add and configure LightingController
            LightingController lightingController = timeSystemGO.AddComponent<LightingController>();
            lightingController.dayNightConfig = dayNightConfig;
            lightingController.skyboxMaterial = skyboxMaterial;

            // Find or create directional light
            Light sun = FindObjectOfType<Light>();
            if (sun == null || sun.type != LightType.Directional)
            {
                GameObject sunGO = new GameObject("Sun");
                sun = sunGO.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            lightingController.directionalLight = sun;

            // Assign skybox to render settings
            RenderSettings.skybox = skyboxMaterial;

            EditorUtility.DisplayDialog("Éxito", "La escena ha sido configurada con el sistema Día/Noche.", "OK");
        }
    }
}