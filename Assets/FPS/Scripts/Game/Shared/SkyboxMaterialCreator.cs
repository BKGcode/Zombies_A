using UnityEngine;

namespace FPS.Game.Shared
{
    /// <summary>
    /// Utilidad para crear materiales básicos de skybox para el sistema día/noche.
    /// Crea materiales procedurales simples para testing y desarrollo rápido.
    /// </summary>
    public static class SkyboxMaterialCreator
    {
        /// <summary>
        /// Crea un material de skybox simple con gradiente día/noche.
        /// </summary>
        public static Material CreateDayNightSkyboxMaterial(string materialName = "DayNightSkybox")
        {
            // Crear material básico si no existe shader personalizado
            Material skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"))
            {
                name = materialName
            };

            // Configurar colores básicos para día/noche
            // Nota: En un proyecto real usarías texturas de skybox más elaboradas
            SetupBasicSkyboxTextures(skyboxMaterial);

            return skyboxMaterial;
        }

        private static void SetupBasicSkyboxTextures(Material material)
        {
            // Crear texturas procedurales básicas (colores sólidos)
            // Frente, Derecha, Atrás, Izquierda, Arriba, Abajo

            Color[] dayColors = {
                new Color(0.47f, 0.76f, 1f),    // Frente - Azul cielo
                new Color(0.47f, 0.76f, 1f),    // Derecha - Azul cielo
                new Color(0.47f, 0.76f, 1f),    // Atrás - Azul cielo
                new Color(0.47f, 0.76f, 1f),    // Izquierda - Azul cielo
                new Color(0.8f, 0.9f, 1f),      // Arriba - Azul claro
                new Color(0.3f, 0.5f, 0.8f)     // Abajo - Azul más oscuro
            };

            Color[] nightColors = {
                new Color(0.05f, 0.05f, 0.15f), // Frente - Azul oscuro
                new Color(0.05f, 0.05f, 0.15f), // Derecha - Azul oscuro
                new Color(0.05f, 0.05f, 0.15f), // Atrás - Azul oscuro
                new Color(0.05f, 0.05f, 0.15f), // Izquierda - Azul oscuro
                new Color(0.02f, 0.02f, 0.08f),  // Arriba - Negro azulado
                new Color(0.02f, 0.02f, 0.05f)   // Abajo - Negro azulado más oscuro
            };

            // Crear texturas 1x1 con colores sólidos
            for (int i = 0; i < 6; i++)
            {
                Texture2D dayTexture = CreateSolidColorTexture(dayColors[i]);
                Texture2D nightTexture = CreateSolidColorTexture(nightColors[i]);

                material.SetTexture("_FrontTex", dayTexture);
                material.SetTexture("_BackTex", dayTexture);
                material.SetTexture("_LeftTex", dayTexture);
                material.SetTexture("_RightTex", dayTexture);
                material.SetTexture("_UpTex", dayTexture);
                material.SetTexture("_DownTex", dayTexture);

                // Guardar texturas como assets para persistencia
                SaveTextureAsAsset(dayTexture, $"Day_Skybox_Face_{i}.png");
                SaveTextureAsAsset(nightTexture, $"Night_Skybox_Face_{i}.png");
            }
        }

        private static Texture2D CreateSolidColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }

        private static void SaveTextureAsAsset(Texture2D texture, string fileName)
        {
            // Nota: En el editor, esto guardaría como asset
            // En runtime, solo configura la textura para uso inmediato
#if UNITY_EDITOR
            byte[] bytes = texture.EncodeToPNG();
            string path = $"Assets/Textures/Skybox/{fileName}";
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            System.IO.File.WriteAllBytes(path, bytes);
            UnityEditor.AssetDatabase.ImportAsset(path);
#endif
        }
    }
}
