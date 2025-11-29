#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FPS.Spawning.Editor
{
    public static class SpawnRuleSetPresetCreator
    {
        private const string DefaultFolder = "Assets/ModAssets/Spawning/SpawnRuleSets";
        private const string DefaultAssetName = "DefaultSpawnRuleSet.asset";

        [MenuItem("Tools/Spawning/Create Default Spawn Rule Set", priority = 10)]
        public static void CreateDefaultSpawnRuleSet()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/ModAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "ModAssets");
            }
            if (!AssetDatabase.IsValidFolder("Assets/ModAssets/Spawning"))
            {
                AssetDatabase.CreateFolder("Assets/ModAssets", "Spawning");
            }
            if (!AssetDatabase.IsValidFolder("Assets/ModAssets/Spawning/SpawnRuleSets"))
            {
                AssetDatabase.CreateFolder("Assets/ModAssets/Spawning", "SpawnRuleSets");
            }

            string assetPath = Path.Combine(DefaultFolder, DefaultAssetName).Replace("\\", "/");

            var rules = ScriptableObject.CreateInstance<SpawnRuleSet>();

            // Recommended defaults
            rules.DripPerMinuteDay = 0.75f;   // goteo suave de día
            rules.DripPerMinuteNight = 4.0f;  // más actividad de noche
            rules.MaxConcurrentGlobal = 40;
            rules.DailyBudget = 120;
            rules.DailyScalePercent = 15f;    // +15% por día
            rules.UseCustomSeed = true;
            rules.RandomSeed = 12345;

            // Ventanas de oleadas
            rules.WaveWindows.Clear();
            rules.WaveWindows.Add(new WaveWindow
            {
                Name = "Night Waves",
                StartHour = 20f,
                EndHour = 5f,
                MinWaveSize = 4,
                MaxWaveSize = 8,
                IntervalMinutes = 8f,
                PeriodAllowed = SpawnPeriodAllowed.NightOnly
            });
            rules.WaveWindows.Add(new WaveWindow
            {
                Name = "Dusk Push",
                StartHour = 18f,
                EndHour = 20f,
                MinWaveSize = 2,
                MaxWaveSize = 4,
                IntervalMinutes = 15f,
                PeriodAllowed = SpawnPeriodAllowed.Any
            });

            AssetDatabase.CreateAsset(rules, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Spawn Rule Set", $"Asset creado en:\n{assetPath}\n\nAñade prefabs de enemigos en la lista 'Enemies' desde el Inspector.", "OK");
            Selection.activeObject = rules;
            EditorGUIUtility.PingObject(rules);
        }
    }
}

/*
Metadata
ScriptRole: Utilidad de Editor para crear un SpawnRuleSet con valores por defecto y ventanas de oleadas.
RelatedScripts: SpawnRuleSet, EnemySpawnManager
UsesSO: SpawnRuleSet
ReceivesFrom / SendsTo: N/A (Editor-only)
Setup: Menú Tools → Spawning → Create Default Spawn Rule Set. Se crea en Assets/ModAssets/Spawning/SpawnRuleSets/DefaultSpawnRuleSet.asset.
*/
#endif
