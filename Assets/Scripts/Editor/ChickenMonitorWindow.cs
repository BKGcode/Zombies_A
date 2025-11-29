#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HappyChickens.Debug
{
    public class ChickenMonitorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private string stateFilter = "All";
        private bool showOnlyCritical = false;
        private bool showOnlyStuck = false;
        private float refreshInterval = 1f;
        private double lastRefreshTime;
        
        private enum SortColumn { ID, State, TimeInState, Hunger, Thirst, Tiredness, Energy }
        private SortColumn currentSortColumn = SortColumn.State;
        private bool sortAscending = true;

        [MenuItem("Tools/Chicken Monitor")]
        public static void ShowWindow()
        {
            ChickenMonitorWindow window = GetWindow<ChickenMonitorWindow>("Chicken Monitor");
            window.minSize = new Vector2(1000f, 400f);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
            {
                lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Chicken Monitor only works in Play Mode", MessageType.Info);
                return;
            }

            DrawToolbar();
            DrawStatsSummary();
            EditorGUILayout.Space(5f);
            DrawChickenTable();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("Search:", GUILayout.Width(50f));
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarTextField, GUILayout.Width(150f));
            
            GUILayout.Space(10f);
            GUILayout.Label("State:", GUILayout.Width(40f));
            stateFilter = EditorGUILayout.TextField(stateFilter, EditorStyles.toolbarTextField, GUILayout.Width(100f));
            
            GUILayout.Space(10f);
            showOnlyCritical = GUILayout.Toggle(showOnlyCritical, "Critical Needs", EditorStyles.toolbarButton);
            showOnlyStuck = GUILayout.Toggle(showOnlyStuck, "Stuck", EditorStyles.toolbarButton);
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label("Refresh:", GUILayout.Width(55f));
            refreshInterval = EditorGUILayout.Slider(refreshInterval, 0.1f, 5f, GUILayout.Width(100f));
            
            if (GUILayout.Button("Clear History", EditorStyles.toolbarButton, GUILayout.Width(90f)))
            {
                ChickenMonitorManager.Instance.ClearHistory();
            }
            
            if (GUILayout.Button("Generate Report", EditorStyles.toolbarButton, GUILayout.Width(110f)))
            {
                ChickenDebugReport.GenerateReport();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatsSummary()
        {
            var chickens = ChickenMonitorManager.Instance.RegisteredChickens;
            var distribution = ChickenMonitorManager.Instance.GetStateDistribution();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            GUILayout.Label($"Total Chickens: {chickens.Count}", EditorStyles.boldLabel);
            GUILayout.Space(20f);
            
            foreach (var kvp in distribution.OrderByDescending(x => x.Value))
            {
                Color color = GetStateColor(kvp.Key);
                GUI.color = color;
                GUILayout.Label($"{kvp.Key}: {kvp.Value}", EditorStyles.miniButton, GUILayout.Width(100f));
                GUI.color = Color.white;
            }
            
            GUILayout.FlexibleSpace();
            
            int criticalCount = ChickenMonitorManager.Instance.GetChickensWithCriticalNeeds().Count;
            int stuckCount = ChickenMonitorManager.Instance.GetStuckChickens().Count;
            
            if (criticalCount > 0)
            {
                GUI.color = Color.yellow;
                GUILayout.Label($"⚠ Critical: {criticalCount}", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            
            if (stuckCount > 0)
            {
                GUI.color = Color.red;
                GUILayout.Label($"⚠ Stuck: {stuckCount}", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawChickenTable()
        {
            var chickens = GetFilteredChickens();
            
            EditorGUILayout.BeginVertical();
            
            DrawTableHeader();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var chicken in chickens)
            {
                DrawChickenRow(chicken);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("ID", EditorStyles.toolbarButton, GUILayout.Width(150f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.ID);
            
            if (GUILayout.Button("State", EditorStyles.toolbarButton, GUILayout.Width(100f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.State);
            
            if (GUILayout.Button("Time", EditorStyles.toolbarButton, GUILayout.Width(50f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.TimeInState);
            
            if (GUILayout.Button("H", EditorStyles.toolbarButton, GUILayout.Width(65f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.Hunger);
            
            if (GUILayout.Button("T", EditorStyles.toolbarButton, GUILayout.Width(65f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.Thirst);
            
            if (GUILayout.Button("Tr", EditorStyles.toolbarButton, GUILayout.Width(65f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.Tiredness);
            
            if (GUILayout.Button("E", EditorStyles.toolbarButton, GUILayout.Width(65f), GUILayout.ExpandWidth(false)))
                SetSortColumn(SortColumn.Energy);
            
            GUILayout.Label("Position", EditorStyles.toolbarButton, GUILayout.Width(90f), GUILayout.ExpandWidth(false));
            GUILayout.Label("Nav", EditorStyles.toolbarButton, GUILayout.Width(70f), GUILayout.ExpandWidth(false));
            GUILayout.Label("Actions", EditorStyles.toolbarButton, GUILayout.Width(110f), GUILayout.ExpandWidth(false));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawChickenRow(ChickenDebugger chicken)
        {
            EditorGUILayout.BeginHorizontal();
            
            Color stateColor = GetStateColor(chicken.CurrentState);
            GUI.color = chicken.HasCriticalNeed ? Color.yellow : (chicken.IsStuck ? Color.red : stateColor);
            
            if (GUILayout.Button(chicken.ChickenID, EditorStyles.label, GUILayout.Width(150f), GUILayout.ExpandWidth(false)))
            {
                Selection.activeGameObject = chicken.gameObject;
                SceneView.FrameLastActiveSceneView();
            }
            
            GUI.color = stateColor;
            GUILayout.Label(chicken.CurrentState, EditorStyles.miniButton, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            GUI.color = Color.white;
            
            GUILayout.Label($"{chicken.TimeInCurrentState:F1}s", GUILayout.Width(50f), GUILayout.ExpandWidth(false));
            
            DrawNeedBar(chicken.Hunger, 65f);
            DrawNeedBar(chicken.Thirst, 65f);
            DrawNeedBar(chicken.Tiredness, 65f);
            DrawNeedBar(chicken.Energy, 65f, true);
            
            GUILayout.Label($"{chicken.CurrentPosition.x:F0},{chicken.CurrentPosition.z:F0}", GUILayout.Width(90f), GUILayout.ExpandWidth(false));
            
            string navStatus = chicken.IsMoving ? "Moving" : (chicken.HasNavMeshPath ? "Idle" : "NoPath");
            GUILayout.Label(navStatus, GUILayout.Width(70f), GUILayout.ExpandWidth(false));
            
            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(52f), GUILayout.ExpandWidth(false)))
            {
                Selection.activeGameObject = chicken.gameObject;
            }
            if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(52f), GUILayout.ExpandWidth(false)))
            {
                EditorGUIUtility.PingObject(chicken.gameObject);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNeedBar(float value, float width, bool inverse = false)
        {
            Rect rect = GUILayoutUtility.GetRect(width, 16f);
            
            float normalized = inverse ? value / 100f : 1f - (value / 100f);
            Color barColor = normalized > 0.5f ? Color.green : (normalized > 0.2f ? Color.yellow : Color.red);
            
            EditorGUI.DrawRect(rect, Color.black);
            
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * normalized, rect.height);
            EditorGUI.DrawRect(fillRect, barColor);
            
            EditorGUI.LabelField(rect, $"{value:F0}", EditorStyles.centeredGreyMiniLabel);
        }

        private List<ChickenDebugger> GetFilteredChickens()
        {
            var chickens = ChickenMonitorManager.Instance.RegisteredChickens.ToList();
            
            if (!string.IsNullOrEmpty(searchFilter))
            {
                chickens = chickens.Where(c => c.ChickenID.ToLower().Contains(searchFilter.ToLower())).ToList();
            }
            
            if (stateFilter != "All" && !string.IsNullOrEmpty(stateFilter))
            {
                chickens = chickens.Where(c => c.CurrentState == stateFilter).ToList();
            }
            
            if (showOnlyCritical)
            {
                chickens = chickens.Where(c => c.HasCriticalNeed).ToList();
            }
            
            if (showOnlyStuck)
            {
                chickens = chickens.Where(c => c.IsStuck).ToList();
            }
            
            return SortChickens(chickens);
        }

        private List<ChickenDebugger> SortChickens(List<ChickenDebugger> chickens)
        {
            switch (currentSortColumn)
            {
                case SortColumn.ID:
                    return sortAscending ? chickens.OrderBy(c => c.ChickenID).ToList() : chickens.OrderByDescending(c => c.ChickenID).ToList();
                case SortColumn.State:
                    return sortAscending ? chickens.OrderBy(c => c.CurrentState).ToList() : chickens.OrderByDescending(c => c.CurrentState).ToList();
                case SortColumn.TimeInState:
                    return sortAscending ? chickens.OrderBy(c => c.TimeInCurrentState).ToList() : chickens.OrderByDescending(c => c.TimeInCurrentState).ToList();
                case SortColumn.Hunger:
                    return sortAscending ? chickens.OrderBy(c => c.Hunger).ToList() : chickens.OrderByDescending(c => c.Hunger).ToList();
                case SortColumn.Thirst:
                    return sortAscending ? chickens.OrderBy(c => c.Thirst).ToList() : chickens.OrderByDescending(c => c.Thirst).ToList();
                case SortColumn.Tiredness:
                    return sortAscending ? chickens.OrderBy(c => c.Tiredness).ToList() : chickens.OrderByDescending(c => c.Tiredness).ToList();
                case SortColumn.Energy:
                    return sortAscending ? chickens.OrderBy(c => c.Energy).ToList() : chickens.OrderByDescending(c => c.Energy).ToList();
                default:
                    return chickens;
            }
        }

        private void SetSortColumn(SortColumn column)
        {
            if (currentSortColumn == column)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = column;
                sortAscending = true;
            }
        }

        private Color GetStateColor(string state)
        {
            switch (state)
            {
                case "ChickenIdleState": return new Color(0.5f, 0.8f, 0.5f);
                case "ChickenEatingState": return new Color(0.8f, 0.6f, 0.3f);
                case "ChickenDrinkingState": return new Color(0.3f, 0.6f, 0.9f);
                case "ChickenSleepingState": return new Color(0.4f, 0.4f, 0.7f);
                case "ChickenNappingState": return new Color(0.6f, 0.5f, 0.7f);
                case "ChickenWanderingState": return new Color(0.7f, 0.7f, 0.5f);
                case "ChickenSeekingFoodState": return new Color(0.9f, 0.5f, 0.3f);
                case "ChickenSeekingWaterState": return new Color(0.3f, 0.5f, 0.9f);
                default: return Color.gray;
            }
        }
    }
}
#endif
