using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Utilidad para ayudar con la configuración y compatibilidad de skybox.
    /// Detecta el tipo de shader y proporciona información útil.
    /// </summary>
    public class SkyboxHelper : MonoBehaviour
    {
        [Header("🔍 Información")]
        [Tooltip("Mostrar información del skybox en consola al iniciar")]
        [SerializeField] private bool logInfoOnStart = true;

        [Header("🎨 Auto-Configuración")]
        [Tooltip("Intentar crear un skybox compatible automáticamente si falta")]
        [SerializeField] private bool autoCreateSkybox = false;

        [Tooltip("Tipo de shader a usar para auto-creación")]
        [SerializeField] private SkyboxShaderType preferredShaderType = SkyboxShaderType.Procedural;

        public enum SkyboxShaderType
        {
            Procedural,  // Skybox/Procedural (default Unity)
            SixSided,    // Skybox/6 Sided
            Cubemap      // Skybox/Cubemap
        }

        private void Start()
        {
            if (logInfoOnStart)
            {
                LogSkyboxInfo();
            }

            if (autoCreateSkybox && RenderSettings.skybox == null)
            {
                CreateCompatibleSkybox();
            }
        }

        /// <summary>
        /// Muestra información detallada del skybox actual.
        /// </summary>
        public void LogSkyboxInfo()
        {
            Material skybox = RenderSettings.skybox;

            if (skybox == null)
            {
                Debug.LogWarning("⚠️ SkyboxHelper: No hay skybox asignado en RenderSettings.");
                Debug.Log("💡 Solución: Asigna un material de skybox en Window > Rendering > Lighting > Environment > Skybox Material");
                return;
            }

            string shaderName = skybox.shader.name;
            Debug.Log($"🌌 Skybox Info:\n" +
                     $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                     $"Shader: {shaderName}\n" +
                     $"Material: {skybox.name}\n" +
                     $"━━━━━━━━━━━━━━━━━━━━━━");

            // Detectar tipo y propiedades
            DetectSkyboxType(skybox);
        }

        private void DetectSkyboxType(Material skybox)
        {
            string shaderName = skybox.shader.name.ToLower();

            if (shaderName.Contains("procedural"))
            {
                Debug.Log("📋 Tipo: Skybox/Procedural (Default Unity)\n" +
                         "Propiedades disponibles:\n" +
                         "  • _SkyTint (Color)\n" +
                         "  • _Exposure (Float)\n" +
                         "  • _AtmosphereThickness (Float)\n" +
                         "  • _SunSize (Float)\n" +
                         "✅ Compatible con LightingController");
            }
            else if (shaderName.Contains("6 sided"))
            {
                Debug.Log("📋 Tipo: Skybox/6 Sided\n" +
                         "Propiedades disponibles:\n" +
                         "  • _Tint (Color)\n" +
                         "  • _Exposure (Float)\n" +
                         "  • _Rotation (Float)\n" +
                         "✅ Compatible con LightingController");
            }
            else if (shaderName.Contains("cubemap"))
            {
                Debug.Log("📋 Tipo: Skybox/Cubemap\n" +
                         "Propiedades disponibles:\n" +
                         "  • _Tint (Color)\n" +
                         "  • _Exposure (Float)\n" +
                         "  • _Rotation (Float)\n" +
                         "✅ Compatible con LightingController");
            }
            else if (skybox.HasProperty("_SkyColor"))
            {
                Debug.Log("📋 Tipo: Shader Personalizado\n" +
                         "Propiedades detectadas:\n" +
                         "  • _SkyColor (Color)\n" +
                         "✅ Compatible con LightingController");
            }
            else
            {
                Debug.LogWarning("⚠️ Tipo: Shader Desconocido\n" +
                                "El shader podría no ser compatible.\n" +
                                "💡 Considera usar Skybox/Procedural para mejor compatibilidad.");
                
                // Listar propiedades disponibles
                ListAvailableProperties(skybox);
            }
        }

        private void ListAvailableProperties(Material material)
        {
            Debug.Log("🔍 Propiedades disponibles en este material:");
            
            Shader shader = material.shader;
            int propertyCount = shader.GetPropertyCount();
            
            for (int i = 0; i < propertyCount; i++)
            {
                string propName = shader.GetPropertyName(i);
                var propType = shader.GetPropertyType(i);
                Debug.Log($"  • {propName} ({propType})");
            }
        }

        /// <summary>
        /// Crea un skybox compatible automáticamente.
        /// </summary>
        public Material CreateCompatibleSkybox()
        {
            Material newSkybox = null;

            switch (preferredShaderType)
            {
                case SkyboxShaderType.Procedural:
                    newSkybox = CreateProceduralSkybox();
                    break;
                case SkyboxShaderType.SixSided:
                    newSkybox = CreateSixSidedSkybox();
                    break;
                case SkyboxShaderType.Cubemap:
                    Debug.LogWarning("Cubemap skybox requiere una textura cubemap. Creando Procedural en su lugar.");
                    newSkybox = CreateProceduralSkybox();
                    break;
            }

            if (newSkybox != null)
            {
                RenderSettings.skybox = newSkybox;
                Debug.Log($"✅ Skybox '{newSkybox.name}' creado y asignado automáticamente.");
            }

            return newSkybox;
        }

        private Material CreateProceduralSkybox()
        {
            Material skybox = new Material(Shader.Find("Skybox/Procedural"))
            {
                name = "AutoGenerated_ProceduralSkybox"
            };

            // Configuración inicial para día
            skybox.SetColor("_SkyTint", new Color(0.5f, 0.7f, 1f));
            skybox.SetFloat("_Exposure", 1.3f);
            skybox.SetFloat("_AtmosphereThickness", 1.0f);
            skybox.SetFloat("_SunSize", 0.04f);
            skybox.SetFloat("_SunSizeConvergence", 5f);

            return skybox;
        }

        private Material CreateSixSidedSkybox()
        {
            Material skybox = new Material(Shader.Find("Skybox/6 Sided"))
            {
                name = "AutoGenerated_6SidedSkybox"
            };

            // Configuración inicial
            skybox.SetColor("_Tint", new Color(0.5f, 0.7f, 1f));
            skybox.SetFloat("_Exposure", 1.0f);
            skybox.SetFloat("_Rotation", 0f);

            // Crear texturas simples para las 6 caras
            Color dayColor = new Color(0.47f, 0.76f, 1f);
            Texture2D texture = CreateSolidTexture(dayColor);

            skybox.SetTexture("_FrontTex", texture);
            skybox.SetTexture("_BackTex", texture);
            skybox.SetTexture("_LeftTex", texture);
            skybox.SetTexture("_RightTex", texture);
            skybox.SetTexture("_UpTex", CreateSolidTexture(new Color(0.8f, 0.9f, 1f)));
            skybox.SetTexture("_DownTex", CreateSolidTexture(new Color(0.3f, 0.5f, 0.8f)));

            return skybox;
        }

        private Texture2D CreateSolidTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Valida si el skybox actual es compatible con el LightingController.
        /// </summary>
        public bool ValidateSkyboxCompatibility()
        {
            Material skybox = RenderSettings.skybox;
            
            if (skybox == null)
            {
                Debug.LogError("❌ No hay skybox asignado.");
                return false;
            }

            bool compatible = skybox.HasProperty("_SkyTint") ||
                            skybox.HasProperty("_Tint") ||
                            skybox.HasProperty("_SkyColor");

            if (compatible)
            {
                Debug.Log("✅ Skybox compatible con LightingController.");
            }
            else
            {
                Debug.LogWarning($"⚠️ Skybox '{skybox.shader.name}' podría no ser compatible.\n" +
                               "Propiedades requeridas: _SkyTint, _Tint, o _SkyColor");
            }

            return compatible;
        }

        #region Editor Buttons (Solo visible en Inspector con custom editor)

        [ContextMenu("Log Skybox Info")]
        private void ContextMenuLogInfo()
        {
            LogSkyboxInfo();
        }

        [ContextMenu("Validate Compatibility")]
        private void ContextMenuValidate()
        {
            ValidateSkyboxCompatibility();
        }

        [ContextMenu("Create Procedural Skybox")]
        private void ContextMenuCreateProcedural()
        {
            preferredShaderType = SkyboxShaderType.Procedural;
            CreateCompatibleSkybox();
        }

        [ContextMenu("Create 6 Sided Skybox")]
        private void ContextMenuCreate6Sided()
        {
            preferredShaderType = SkyboxShaderType.SixSided;
            CreateCompatibleSkybox();
        }

        #endregion
    }
}
