using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JuanTools
{
    /// <summary>
    /// An editor tool to generate a pre-configured scene with a neon fog aesthetic.
    /// </summary>
    public class NeonFogSceneTool : EditorWindow
    {
        [MenuItem("Tools/JuanTools/Neon Fog Scene Generator")]
        public static void ShowWindow()
        {
            GetWindow<NeonFogSceneTool>("Neon Fog Scene");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Generate Scene"))
            {
                GenerateScene();
            }
        }

        /// <summary>
        /// Generates the entire scene content and configuration.
        /// </summary>
        private void GenerateScene()
        {
            CleanScene();
            ConfigureLightingSettings();
            CreateSceneObjects();
            CreateLights();
            CreatePostProcessing();
            CreateCamera();

            Debug.Log("Neon Fog Scene generated and Lighting configured!");
        }

        /// <summary>
        /// Removes all GameObjects from the current scene.
        /// </summary>
        private void CleanScene()
        {
            foreach (var obj in FindObjectsOfType<GameObject>())
            {
                DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Configures the scene's global lighting and fog settings.
        /// </summary>
        private void ConfigureLightingSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.05f, 0.12f, 0.1f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.2f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.04f, 0.03f);
            RenderSettings.reflectionIntensity = 0.2f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.1f, 0.25f, 0.2f);
            RenderSettings.fogDensity = 0.05f;
        }

        /// <summary>
        /// Creates the basic geometry for the scene (ground, building, car).
        /// </summary>
        private void CreateSceneObjects()
        {
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(30, 0.1f, 30);
            ground.GetComponent<MeshRenderer>().sharedMaterial = CreateMat(new Color(0.08f, 0.1f, 0.09f), 0.8f);

            // Building
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "Building";
            building.transform.localScale = new Vector3(12, 8, 10);
            building.transform.position = new Vector3(0, 4, 0);
            building.GetComponent<MeshRenderer>().sharedMaterial = CreateMat(new Color(0.15f, 0.18f, 0.17f), 0.3f);

            // Car placeholder
            GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = "Car_Placeholder";
            car.transform.localScale = new Vector3(2, 1, 4);
            car.transform.position = new Vector3(0, 0.5f, -6);
            car.GetComponent<MeshRenderer>().sharedMaterial = CreateMat(new Color(0.18f, 0.22f, 0.2f), 0.5f);
        }

        /// <summary>
        /// Creates the lights for the scene.
        /// </summary>
        private void CreateLights()
        {
            // Green ambient light
            GameObject greenLightObj = new GameObject("GreenLight");
            Light greenLight = greenLightObj.AddComponent<Light>();
            greenLight.type = LightType.Point;
            greenLight.color = new Color(0.4f, 1f, 0.8f);
            greenLight.intensity = 8f;
            greenLight.range = 15f;
            greenLight.transform.position = new Vector3(-6, 3, 0);

            // Orange light from sign
            GameObject orangeLightObj = new GameObject("OrangeLight");
            Light orangeLight = orangeLightObj.AddComponent<Light>();
            orangeLight.type = LightType.Point;
            orangeLight.color = new Color(1f, 0.6f, 0.3f);
            orangeLight.intensity = 10f;
            orangeLight.range = 20f;
            orangeLight.transform.position = new Vector3(5, 5, 5);
            
            // Directional Light (very subtle)
            GameObject dirLightObj = new GameObject("Directional Light");
            Light dirLight = dirLightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.25f, 0.6f, 0.5f);
            dirLight.intensity = 0.05f;
            dirLight.transform.rotation = Quaternion.Euler(20, 60, 0);
        }

        /// <summary>
        /// Creates the Global Volume and configures post-processing effects.
        /// </summary>
        private void CreatePostProcessing()
        {
            GameObject volumeObj = new GameObject("Global Volume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "NeonFog_Profile";

            // --- Guardar el perfil como un asset persistente ---
            // Esto evita NullReferenceExceptions en el editor al perderse la referencia en memoria.
            string generatedAssetsPath = "Assets/Generated";
            if (!Directory.Exists(generatedAssetsPath))
            {
                Directory.CreateDirectory(generatedAssetsPath);
            }
            string profilePath = Path.Combine(generatedAssetsPath, profile.name + ".asset");
            AssetDatabase.CreateAsset(profile, profilePath);
            // --- Fin de la modificación ---

            volume.profile = profile;

            var tonemapping = volume.profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.None);

            var bloom = volume.profile.Add<Bloom>(true);
            bloom.intensity.Override(1.0f);
            bloom.threshold.Override(1.1f);

            // var colorAdj = volume.profile.Add<ColorAdjustments>(true);
            // colorAdj.contrast.Override(25f);
            // colorAdj.postExposure.Override(-0.4f);
            // colorAdj.colorFilter.Override(new Color(0.2f, 0.35f, 0.3f));

            var vignette = volume.profile.Add<Vignette>(true);
            vignette.intensity.Override(0.35f);
            vignette.smoothness.Override(0.5f);

            var grain = volume.profile.Add<FilmGrain>(true);
            grain.intensity.Override(0.15f);
        }

        /// <summary>
        /// Creates and configures the main camera.
        /// </summary>
        private void CreateCamera()
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.transform.position = new Vector3(-8, 3, -10);
            camObj.transform.LookAt(new Vector3(0, 2, 0));
            cam.fieldOfView = 38;

            var camData = camObj.AddComponent<UniversalAdditionalCameraData>();
            // The line below is disabled to prevent a URP crash in projects with a misconfigured Renderer Asset (missing Color Grading LUT).
            // To see post-processing effects, you must fix the project's URP configuration first.
            // camData.renderPostProcessing = true;

            Selection.activeGameObject = camObj;
        }

        private Material CreateMat(Color color, float smoothness)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }
    }
}
