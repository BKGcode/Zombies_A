#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace GallinasFelices.Editor
{
    public class ChickenPrefabGenerator : EditorWindow
    {
        [Header("Settings")]
        private List<GameObject> selectedMeshes = new List<GameObject>();
        private string prefabOutputPath = "Assets/Prefabs/Chickens";
        private string materialOutputPath = "Assets/ART/Materials/Chickens";
        private Vector2 scrollPosition;
        private Shader defaultShader;

        [MenuItem("Gallinas Felices/Chicken Prefab Generator")]
        public static void ShowWindow()
        {
            ChickenPrefabGenerator window = GetWindow<ChickenPrefabGenerator>("Chicken Prefab Generator");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnEnable()
        {
            defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Chicken Prefab Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Arrastra meshes con prefijo MSH_ para generar prefabs automáticamente.", MessageType.Info);
            
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
            prefabOutputPath = EditorGUILayout.TextField("Prefab Output Path", prefabOutputPath);
            materialOutputPath = EditorGUILayout.TextField("Material Output Path", materialOutputPath);
            defaultShader = EditorGUILayout.ObjectField("Default Shader", defaultShader, typeof(Shader), false) as Shader;

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Selected Meshes", EditorStyles.boldLabel);
            
            Rect dropArea = GUILayoutUtility.GetRect(0f, 100f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Meshes Here");
            
            HandleDragAndDrop(dropArea);

            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            
            for (int i = selectedMeshes.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                
                selectedMeshes[i] = EditorGUILayout.ObjectField(selectedMeshes[i], typeof(GameObject), false) as GameObject;
                
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    selectedMeshes.RemoveAt(i);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear All"))
            {
                selectedMeshes.Clear();
            }
            
            if (GUILayout.Button("Add Selected Objects"))
            {
                AddSelectedObjects();
            }
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUI.enabled = selectedMeshes.Count > 0;
            
            if (GUILayout.Button("Generate Prefabs", GUILayout.Height(40)))
            {
                GeneratePrefabs();
            }
            
            GUI.enabled = true;

            GUILayout.Space(10);
        }

        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (!dropArea.Contains(evt.mousePosition))
                {
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            if (!selectedMeshes.Contains(go))
                            {
                                selectedMeshes.Add(go);
                            }
                        }
                    }
                }
                
                evt.Use();
            }
        }

        private void AddSelectedObjects()
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is GameObject go)
                {
                    if (!selectedMeshes.Contains(go))
                    {
                        selectedMeshes.Add(go);
                    }
                }
            }
        }

        private void GeneratePrefabs()
        {
            if (selectedMeshes.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No meshes selected!", "OK");
                return;
            }

            if (!Directory.Exists(prefabOutputPath))
            {
                Directory.CreateDirectory(prefabOutputPath);
            }

            if (!Directory.Exists(materialOutputPath))
            {
                Directory.CreateDirectory(materialOutputPath);
            }

            int successCount = 0;
            int errorCount = 0;
            List<string> errors = new List<string>();

            foreach (GameObject mesh in selectedMeshes)
            {
                if (mesh == null)
                {
                    continue;
                }

                try
                {
                    string meshName = mesh.name;
                    
                    if (!meshName.StartsWith("MSH_"))
                    {
                        errors.Add($"{meshName}: No tiene prefijo MSH_");
                        errorCount++;
                        continue;
                    }

                    string baseName = meshName.Substring(4);
                    string prefabName = "PFB_" + baseName;
                    string materialName = "MAT_" + baseName;

                    Material newMaterial = new Material(defaultShader != null ? defaultShader : Shader.Find("Universal Render Pipeline/Lit"));
                    newMaterial.name = materialName;
                    
                    string materialPath = Path.Combine(materialOutputPath, materialName + ".mat");
                    AssetDatabase.CreateAsset(newMaterial, materialPath);

                    GameObject prefabInstance = Instantiate(mesh);
                    prefabInstance.name = prefabName;

                    MeshRenderer[] renderers = prefabInstance.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in renderers)
                    {
                        renderer.sharedMaterial = newMaterial;
                    }

                    string prefabPath = Path.Combine(prefabOutputPath, prefabName + ".prefab");
                    
                    GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                    
                    DestroyImmediate(prefabInstance);

                    if (savedPrefab != null)
                    {
                        successCount++;
                        Debug.Log($"[ChickenPrefabGenerator] Created: {prefabName} with material {materialName}");
                    }
                    else
                    {
                        errors.Add($"{meshName}: Error al guardar prefab");
                        errorCount++;
                    }
                }
                catch (System.Exception e)
                {
                    errors.Add($"{mesh.name}: {e.Message}");
                    errorCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Prefabs generados: {successCount}\nErrores: {errorCount}";
            
            if (errors.Count > 0)
            {
                message += "\n\nErrores:\n" + string.Join("\n", errors);
            }

            EditorUtility.DisplayDialog("Generación Completada", message, "OK");

            if (successCount > 0)
            {
                selectedMeshes.Clear();
            }
        }
    }
}
#endif
