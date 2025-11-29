using UnityEditor;
using UnityEngine;
using System.IO;

namespace FPS.Game.Shared.Editor
{
    [CustomEditor(typeof(DayNightProfile))]
    public class DayNightProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generar Texturas de Skybox"))
            {
                GenerateSkyboxTextures((DayNightProfile)target);
            }
        }

        private void GenerateSkyboxTextures(DayNightProfile profile)
        {
            // Get path of the ScriptableObject asset
            string profilePath = AssetDatabase.GetAssetPath(profile);
            string profileDir = Path.GetDirectoryName(profilePath);
            string texturesDir = Path.Combine(profileDir, profile.name + "_Textures");

            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }

            Debug.Log($"Generando texturas en: {texturesDir}");

            // Day Textures
            SaveTextureAsAsset(CreateSolidColorTexture(profile.dayTopColor), Path.Combine(texturesDir, "Day_Up.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.dayHorizonColor), Path.Combine(texturesDir, "Day_Front.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.dayHorizonColor), Path.Combine(texturesDir, "Day_Back.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.dayHorizonColor), Path.Combine(texturesDir, "Day_Left.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.dayHorizonColor), Path.Combine(texturesDir, "Day_Right.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.dayBottomColor), Path.Combine(texturesDir, "Day_Down.png"));

            // Night Textures
            SaveTextureAsAsset(CreateSolidColorTexture(profile.nightTopColor), Path.Combine(texturesDir, "Night_Up.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.nightHorizonColor), Path.Combine(texturesDir, "Night_Front.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.nightHorizonColor), Path.Combine(texturesDir, "Night_Back.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.nightHorizonColor), Path.Combine(texturesDir, "Night_Left.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.nightHorizonColor), Path.Combine(texturesDir, "Night_Right.png"));
            SaveTextureAsAsset(CreateSolidColorTexture(profile.nightBottomColor), Path.Combine(texturesDir, "Night_Down.png"));

            AssetDatabase.Refresh();
            Debug.Log("¡Texturas generadas y guardadas!");
        }

        private Texture2D CreateSolidColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void SaveTextureAsAsset(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }
    }
}
