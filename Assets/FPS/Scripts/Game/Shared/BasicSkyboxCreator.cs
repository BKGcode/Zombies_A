using UnityEngine;
using System.IO;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Herramienta para crear materiales básicos de skybox proceduralmente.
    /// Crea materiales simples con colores sólidos para desarrollo y testing rápido.
    /// </summary>
    public class BasicSkyboxCreator : MonoBehaviour
    {
        [Header("🎨 Configuración de Colores")]
        [Tooltip("Color del cielo durante el día")]
        public Color daySkyColor = new Color(0.47f, 0.76f, 1f);

        [Tooltip("Color del cielo durante la noche")]
        public Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f);

        [Header("💾 Configuración de Guardado")]
        [Tooltip("Nombre del material a crear")]
        [SerializeField] private string materialName = "BasicDayNightSkybox";

        [Tooltip("¿Guardar materiales como assets permanentes?")]
        [SerializeField] private bool saveAsAssets = false;

        private void Start()
        {
            CreateBasicSkybox();
        }

        /// <summary>
        /// Crea un material de skybox básico con colores día/noche.
        /// </summary>
        public Material CreateBasicSkybox()
        {
            Material skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"))
            {
                name = materialName
            };

            // Crear texturas básicas
            CreateSkyboxTextures(skyboxMaterial);

            // Aplicar como skybox actual
            RenderSettings.skybox = skyboxMaterial;

            Debug.Log($"✅ Skybox básico creado: {materialName}");

            return skyboxMaterial;
        }

        private void CreateSkyboxTextures(Material material)
        {
            // Crear colores para cada cara del cubo del skybox
            Color[] dayColors = {
                daySkyColor,    // Frente
                daySkyColor,    // Derecha
                daySkyColor,    // Atrás
                daySkyColor,    // Izquierda
                new Color(0.8f, 0.9f, 1f), // Arriba (más claro)
                new Color(0.3f, 0.5f, 0.8f)  // Abajo (más oscuro)
            };

            Color[] nightColors = {
                nightSkyColor,    // Frente
                nightSkyColor,    // Derecha
                nightSkyColor,    // Atrás
                nightSkyColor,    // Izquierda
                new Color(0.02f, 0.02f, 0.08f), // Arriba (casi negro)
                new Color(0.02f, 0.02f, 0.05f)  // Abajo (negro azulado)
            };

            // Crear y asignar texturas para cada cara
            for (int i = 0; i < 6; i++)
            {
                Texture2D dayTexture = CreateSolidColorTexture(dayColors[i]);
                Texture2D nightTexture = CreateSolidColorTexture(nightColors[i]);

                // Asignar texturas al material
                switch (i)
                {
                    case 0: // Frente
                        material.SetTexture("_FrontTex", dayTexture);
                        break;
                    case 1: // Derecha
                        material.SetTexture("_BackTex", dayTexture);
                        break;
                    case 2: // Atrás
                        material.SetTexture("_LeftTex", dayTexture);
                        break;
                    case 3: // Izquierda
                        material.SetTexture("_RightTex", dayTexture);
                        break;
                    case 4: // Arriba
                        material.SetTexture("_UpTex", dayTexture);
                        break;
                    case 5: // Abajo
                        material.SetTexture("_DownTex", dayTexture);
                        break;
                }

                // Guardar como assets si está habilitado
                if (saveAsAssets)
                {
                    SaveTextureAsAsset(dayTexture, $"Day_Skybox_Face_{i}.png");
                    SaveTextureAsAsset(nightTexture, $"Night_Skybox_Face_{i}.png");
                }
            }
        }

        private Texture2D CreateSolidColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }

        private void SaveTextureAsAsset(Texture2D texture, string fileName)
        {
#if UNITY_EDITOR
            string folderPath = "Assets/Textures/Skybox";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string path = $"{folderPath}/{fileName}";
            UnityEditor.AssetDatabase.CreateAsset(texture, path);
            Debug.Log($"💾 Textura guardada: {path}");
#endif
        }

        /// <summary>
        /// Método público para crear skybox desde otros scripts.
        /// </summary>
        public static Material CreateDefaultSkybox()
        {
            return new BasicSkyboxCreator().CreateBasicSkybox();
        }

        /// <summary>
        /// Aplica colores personalizados al skybox actual.
        /// </summary>
        public void UpdateSkyboxColors(Color newDayColor, Color newNightColor)
        {
            daySkyColor = newDayColor;
            nightSkyColor = newNightColor;

            CreateBasicSkybox();
        }
    }
}
