#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.FPS.AI;
using Unity.FPS.Game;

namespace Unity.FPS.EditorExt
{
    /// <summary>
    /// Editor tool to generate a complete enemy prefab with all necessary components and configuration.
    /// Access via: Tools > FPS > Enemy Prefab Generator
    /// </summary>
    public class EnemyPrefabGenerator : EditorWindow
    {
        // Enemy configuration
        private string enemyName = "NewEnemy";
        private GameObject modelPrefab;
        private GameObject weaponPrefab;
        
        // Visual settings
        private bool createVisualEffects = true;
        private bool createAudioEffects = true;
        private bool createAnimations = true;
        private bool createLootSystem = false;
        
        // Material references for VFX
        private Material eyeMaterial;
        private Material bodyMaterial;
        private GameObject deathVFXPrefab;
        
        // Audio clips
        private AudioClip damageSFX;
        private AudioClip detectionSFX;
        private AudioClip movementSFX;
        
        // Loot settings
        private GameObject lootPrefab;
        private float lootDropRate = 0.5f;
        
        // ScriptableObject settings
        private bool createNewEnemyTypeSO = true;
        private EnemyTypeSO existingEnemyTypeSO;
        
        // Enemy stats
        private float maxHealth = 100f;
        private float moveSpeed = 3.5f;
        private float visionRange = 20f;
        private float visionAngle = 120f;
        private float attackRange = 10f;
        private float waitTimeMin = 1f;
        private float waitTimeMax = 3f;
        
        // Output settings
        private string savePath = "Assets/FPS/Prefabs/Enemies";
        private string soSavePath = "Assets/FPS/ScriptableObjects/Enemies";
        
        private Vector2 scrollPosition;

        [MenuItem("Tools/FPS/Enemy Prefab Generator")]
        public static void ShowWindow()
        {
            EnemyPrefabGenerator window = GetWindow<EnemyPrefabGenerator>("Enemy Generator");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("ENEMY PREFAB GENERATOR", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tool creates a complete enemy prefab with all necessary components, ScriptableObject configuration, and optional VFX/Audio/Animation systems.", MessageType.Info);
            
            GUILayout.Space(10);
            
            // === BASIC CONFIGURATION ===
            DrawSection("BASIC CONFIGURATION", () =>
            {
                enemyName = EditorGUILayout.TextField("Enemy Name", enemyName);
                modelPrefab = (GameObject)EditorGUILayout.ObjectField("Model Prefab", modelPrefab, typeof(GameObject), false);
                weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab", weaponPrefab, typeof(GameObject), false);
            });
            
            // === STATS CONFIGURATION ===
            DrawSection("ENEMY STATS", () =>
            {
                maxHealth = EditorGUILayout.FloatField("Max Health", maxHealth);
                moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
                visionRange = EditorGUILayout.FloatField("Vision Range", visionRange);
                visionAngle = EditorGUILayout.Slider("Vision Angle", visionAngle, 0f, 360f);
                attackRange = EditorGUILayout.FloatField("Attack Range", attackRange);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Patrol Wait Times", EditorStyles.miniBoldLabel);
                waitTimeMin = EditorGUILayout.FloatField("  Min Wait Time", waitTimeMin);
                waitTimeMax = EditorGUILayout.FloatField("  Max Wait Time", waitTimeMax);
            });
            
            // === SCRIPTABLE OBJECT ===
            DrawSection("SCRIPTABLE OBJECT", () =>
            {
                createNewEnemyTypeSO = EditorGUILayout.Toggle("Create New EnemyTypeSO", createNewEnemyTypeSO);
                
                if (!createNewEnemyTypeSO)
                {
                    existingEnemyTypeSO = (EnemyTypeSO)EditorGUILayout.ObjectField("Existing EnemyTypeSO", existingEnemyTypeSO, typeof(EnemyTypeSO), false);
                }
                else
                {
                    soSavePath = EditorGUILayout.TextField("SO Save Path", soSavePath);
                }
            });
            
            // === OPTIONAL SYSTEMS ===
            DrawSection("OPTIONAL SYSTEMS", () =>
            {
                createVisualEffects = EditorGUILayout.Toggle("Create Visual Effects (EnemyVFX)", createVisualEffects);
                createAudioEffects = EditorGUILayout.Toggle("Create Audio Effects (EnemyAudio)", createAudioEffects);
                createAnimations = EditorGUILayout.Toggle("Create Animations (EnemyAnimation)", createAnimations);
                createLootSystem = EditorGUILayout.Toggle("Create Loot System (EnemyLoot)", createLootSystem);
            });
            
            // === VFX SETTINGS ===
            if (createVisualEffects)
            {
                DrawSection("VFX SETTINGS", () =>
                {
                    eyeMaterial = (Material)EditorGUILayout.ObjectField("Eye Material (Emissive)", eyeMaterial, typeof(Material), false);
                    bodyMaterial = (Material)EditorGUILayout.ObjectField("Body Material (Emissive)", bodyMaterial, typeof(Material), false);
                    deathVFXPrefab = (GameObject)EditorGUILayout.ObjectField("Death VFX Prefab", deathVFXPrefab, typeof(GameObject), false);
                    EditorGUILayout.HelpBox("Materials must use URP/Lit shader with Emission enabled.", MessageType.None);
                });
            }
            
            // === AUDIO SETTINGS ===
            if (createAudioEffects)
            {
                DrawSection("AUDIO SETTINGS", () =>
                {
                    damageSFX = (AudioClip)EditorGUILayout.ObjectField("Damage SFX", damageSFX, typeof(AudioClip), false);
                    detectionSFX = (AudioClip)EditorGUILayout.ObjectField("Detection SFX", detectionSFX, typeof(AudioClip), false);
                    movementSFX = (AudioClip)EditorGUILayout.ObjectField("Movement SFX", movementSFX, typeof(AudioClip), false);
                });
            }
            
            // === LOOT SETTINGS ===
            if (createLootSystem)
            {
                DrawSection("LOOT SETTINGS", () =>
                {
                    lootPrefab = (GameObject)EditorGUILayout.ObjectField("Loot Prefab", lootPrefab, typeof(GameObject), false);
                    lootDropRate = EditorGUILayout.Slider("Drop Rate", lootDropRate, 0f, 1f);
                });
            }
            
            // === OUTPUT SETTINGS ===
            DrawSection("OUTPUT SETTINGS", () =>
            {
                savePath = EditorGUILayout.TextField("Prefab Save Path", savePath);
            });
            
            GUILayout.Space(20);
            
            // === GENERATE BUTTON ===
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("GENERATE ENEMY PREFAB", GUILayout.Height(40)))
            {
                GenerateEnemyPrefab();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        void DrawSection(string title, System.Action content)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            content?.Invoke();
            EditorGUI.indentLevel--;
        }

        void GenerateEnemyPrefab()
        {
            if (string.IsNullOrEmpty(enemyName))
            {
                EditorUtility.DisplayDialog("Error", "Enemy name cannot be empty!", "OK");
                return;
            }
            
            // Create root GameObject
            GameObject enemyRoot = new GameObject(enemyName);
            
            // Add model if provided
            if (modelPrefab != null)
            {
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, enemyRoot.transform);
                model.name = "Model";
            }
            
            // === CORE COMPONENTS (REQUIRED) ===
            
            // 1. NavMeshAgent
            NavMeshAgent navAgent = enemyRoot.AddComponent<NavMeshAgent>();
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = 120f;
            navAgent.acceleration = 8f;
            navAgent.stoppingDistance = attackRange * 0.8f;
            navAgent.radius = 0.5f;
            navAgent.height = 2f;
            
            // 2. Health
            Health health = enemyRoot.AddComponent<Health>();
            health.MaxHealth = maxHealth;
            
            // 3. Actor
            Actor actor = enemyRoot.AddComponent<Actor>();
            actor.Affiliation = 1; // Enemy affiliation
            
            // Create AimPoint
            GameObject aimPoint = new GameObject("AimPoint");
            aimPoint.transform.SetParent(enemyRoot.transform);
            aimPoint.transform.localPosition = new Vector3(0, 1.5f, 0);
            actor.AimPoint = aimPoint.transform;
            
            // 4. Damageable
            Damageable damageable = enemyRoot.AddComponent<Damageable>();
            damageable.DamageMultiplier = 1f;
            
            // 5. DetectionModule
            DetectionModule detection = enemyRoot.AddComponent<DetectionModule>();
            detection.AttackRange = attackRange;
            detection.KnownTargetTimeout = 4f;
            
            // Create DetectionSourcePoint
            GameObject detectionPoint = new GameObject("DetectionSourcePoint");
            detectionPoint.transform.SetParent(enemyRoot.transform);
            detectionPoint.transform.localPosition = new Vector3(0, 1.7f, 0);
            detection.DetectionSourcePoint = detectionPoint.transform;
            
            // 6. EnemyBrain
            EnemyBrain brain = enemyRoot.AddComponent<EnemyBrain>();
            
            // Create or assign EnemyTypeSO
            EnemyTypeSO enemyTypeSO = null;
            if (createNewEnemyTypeSO)
            {
                enemyTypeSO = CreateEnemyTypeSO();
            }
            else if (existingEnemyTypeSO != null)
            {
                enemyTypeSO = existingEnemyTypeSO;
            }
            
            if (enemyTypeSO != null)
            {
                // Use SerializedObject to properly set the reference
                SerializedObject serializedBrain = new SerializedObject(brain);
                SerializedProperty enemyTypeProp = serializedBrain.FindProperty("enemyType");
                enemyTypeProp.objectReferenceValue = enemyTypeSO;
                serializedBrain.ApplyModifiedProperties();
            }
            
            // 7. Add Weapon
            if (weaponPrefab != null)
            {
                GameObject weaponHolder = new GameObject("WeaponHolder");
                weaponHolder.transform.SetParent(enemyRoot.transform);
                weaponHolder.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
                
                GameObject weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab, weaponHolder.transform);
                weapon.name = "Weapon";
            }
            
            // 8. Add Collider
            CapsuleCollider collider = enemyRoot.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 1f, 0);
            collider.radius = 0.5f;
            collider.height = 2f;
            
            // === OPTIONAL COMPONENTS ===
            
            // EnemyAnimation
            if (createAnimations)
            {
                EnemyAnimation animation = enemyRoot.AddComponent<EnemyAnimation>();
                // Animator will be auto-detected if present in children
            }
            
            // EnemyAudio
            if (createAudioEffects)
            {
                AudioSource audioSource = enemyRoot.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.minDistance = 5f;
                audioSource.maxDistance = 30f;
                
                EnemyAudio audio = enemyRoot.AddComponent<EnemyAudio>();
                
                if (damageSFX != null || detectionSFX != null || movementSFX != null)
                {
                    SerializedObject serializedAudio = new SerializedObject(audio);
                    if (damageSFX) serializedAudio.FindProperty("damageSfx").objectReferenceValue = damageSFX;
                    if (detectionSFX) serializedAudio.FindProperty("detectionSfx").objectReferenceValue = detectionSFX;
                    if (movementSFX) serializedAudio.FindProperty("movementSfx").objectReferenceValue = movementSFX;
                    serializedAudio.ApplyModifiedProperties();
                }
            }
            
            // EnemyVFX
            if (createVisualEffects)
            {
                EnemyVFX vfx = enemyRoot.AddComponent<EnemyVFX>();
                
                SerializedObject serializedVFX = new SerializedObject(vfx);
                if (eyeMaterial) serializedVFX.FindProperty("eyeColorMaterial").objectReferenceValue = eyeMaterial;
                if (bodyMaterial) serializedVFX.FindProperty("bodyMaterial").objectReferenceValue = bodyMaterial;
                if (deathVFXPrefab) serializedVFX.FindProperty("deathVfx").objectReferenceValue = deathVFXPrefab;
                
                // Set default eye colors
                serializedVFX.FindProperty("defaultEyeColor").colorValue = Color.blue;
                serializedVFX.FindProperty("attackEyeColor").colorValue = Color.red;
                
                // Set death VFX spawn point
                if (deathVFXPrefab != null)
                {
                    serializedVFX.FindProperty("deathVfxSpawnPoint").objectReferenceValue = enemyRoot.transform;
                }
                
                serializedVFX.ApplyModifiedProperties();
            }
            
            // EnemyLoot
            if (createLootSystem && lootPrefab != null)
            {
                EnemyLoot loot = enemyRoot.AddComponent<EnemyLoot>();
                
                SerializedObject serializedLoot = new SerializedObject(loot);
                serializedLoot.FindProperty("lootPrefab").objectReferenceValue = lootPrefab;
                serializedLoot.FindProperty("dropRate").floatValue = lootDropRate;
                serializedLoot.ApplyModifiedProperties();
            }
            
            // === SAVE AS PREFAB ===
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                string[] folders = savePath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            string prefabPath = $"{savePath}/{enemyName}.prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemyRoot, prefabPath);
            DestroyImmediate(enemyRoot);
            
            // Select and ping the created prefab
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", 
                $"Enemy prefab '{enemyName}' created successfully!\n\nLocation: {prefabPath}\n\n" +
                "Remember to assign a PatrolPath in the scene if needed.", "OK");
        }

        EnemyTypeSO CreateEnemyTypeSO()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(soSavePath))
            {
                string[] folders = soSavePath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            EnemyTypeSO enemyTypeSO = ScriptableObject.CreateInstance<EnemyTypeSO>();
            
            enemyTypeSO.EnemyName = enemyName;
            enemyTypeSO.MaxHealth = maxHealth;
            enemyTypeSO.MoveSpeed = moveSpeed;
            enemyTypeSO.WaitTimeMin = waitTimeMin;
            enemyTypeSO.WaitTimeMax = waitTimeMax;
            enemyTypeSO.VisionRange = visionRange;
            enemyTypeSO.VisionAngle = visionAngle;
            enemyTypeSO.AttackRange = attackRange;
            
            string soPath = $"{soSavePath}/{enemyName}Type.asset";
            soPath = AssetDatabase.GenerateUniqueAssetPath(soPath);
            
            AssetDatabase.CreateAsset(enemyTypeSO, soPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"EnemyTypeSO created at: {soPath}");
            
            return enemyTypeSO;
        }
    }
}
#endif

/* 
# METADATA
ScriptRole: Editor tool to generate complete enemy prefabs with all required and optional components.
RelatedScripts: EnemyBrain, DetectionModule, EnemyAnimation, EnemyAudio, EnemyVFX, EnemyLoot, EnemyTypeSO.
UsesSO: EnemyTypeSO (creates or uses existing).
ReceivesFrom: User input via Editor Window.
SendsTo: Creates prefabs and ScriptableObjects in the project.
Setup:
- Access via: Tools > FPS > Enemy Prefab Generator
- Configure all settings in the window
- Click "Generate Enemy Prefab" to create the complete prefab
- The tool creates all necessary components, configures them, and saves as a prefab asset
*/
