using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.FPS.AI;
using Unity.FPS.Game;
using FPS.Game.Shared;

namespace FPS.Spawning
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Reglas de spawn (SO)")]
        public SpawnRuleSet RuleSet;

        [Tooltip("Gestor de enemigos existente en la escena")]
        public EnemyManager EnemyManager;

        [Tooltip("Referencia al jugador (para distancias y LOS)")]
        public Transform Player;

        [Header("Configuración")]
        [Tooltip("Usar NavMesh.SamplePosition para posicionar spawns")]
        public bool UseNavMesh = false;

        [Tooltip("Capa para el raycast de línea de visión (obstáculos)")]
        public LayerMask ObstacleLayers = ~0;

        [Tooltip("Intervalo de actualización del manager (segundos reales)")]
        [Min(0.1f)] public float TickSeconds = 0.5f;

        [Header("Debug")]
        public bool LogSpawns = false;
        public bool DebugSpawns = false;
        
    [Header("Aleatoriedad (Opcional)")]
    [Tooltip("Evitar que el mismo punto se repita inmediatamente")]
    public bool AvoidImmediateRepeatPoint = true;

    [Tooltip("Evitar que el mismo tipo de enemigo se repita inmediatamente")]
    public bool AvoidImmediateRepeatEnemy = false;

    [Tooltip("Variación porcentual adicional para el peso de los puntos (0-1)")]
    [Range(0f, 1f)] public float PointWeightJitterPercent = 0f;

        // Interno
        private readonly List<EnemySpawnPoint> _points = new();
        private TimeManager _time;
        private float _dripAccumulator;
        private float _lastTickTime;
        private int _currentDayIndex = 0; // simple: día 0,1,2... si hace falta
        private int _dailyBudgetRemaining;
    private System.Random _rng;
        private EnemySpawnPoint _lastPoint;
        private GameObject _lastEnemyPrefab;

        void Awake()
        {
            _time = FindObjectOfType<TimeManager>();
            FindAllSpawnPoints();
            InitRandom();
            ResetDailyBudget();
        }

        void OnEnable()
        {
            if (_time != null)
            {
                _time.OnDayNightChanged += OnDayNightChanged;
                _time.OnHourChanged += OnHourChanged;
            }
        }

        void OnDisable()
        {
            if (_time != null)
            {
                _time.OnDayNightChanged -= OnDayNightChanged;
                _time.OnHourChanged -= OnHourChanged;
            }
        }

        void Update()
        {
            if (RuleSet == null || EnemyManager == null || Player == null || _time == null) return;

            if (Time.time - _lastTickTime < TickSeconds) return;
            _lastTickTime = Time.time;

            TickDrip();
            TickWaves();
        }

        void FindAllSpawnPoints()
        {
            _points.Clear();
            _points.AddRange(FindObjectsOfType<EnemySpawnPoint>());
        }

        void InitRandom()
        {
            if (RuleSet != null && RuleSet.UseCustomSeed)
            {
                _rng = new System.Random(RuleSet.RandomSeed);
            }
            else
            {
                _rng = new System.Random();
            }
        }

        void ResetDailyBudget()
        {
            if (RuleSet == null) return;
            int baseBudget = Mathf.Max(0, RuleSet.DailyBudget);
            float scale = 1f + Mathf.Max(0f, RuleSet.DailyScalePercent) / 100f * _currentDayIndex;
            _dailyBudgetRemaining = baseBudget == 0 ? int.MaxValue : Mathf.CeilToInt(baseBudget * scale);
        }

        void OnDayNightChanged(bool isDay)
        {
            // No acción obligatoria, pero podríamos limpiar acumuladores si se desea
        }

        void OnHourChanged(float hour)
        {
            // Si pasamos de 23->0, reseteo de presupuesto diario (suponiendo OnHourChanged a medianoche)
            // El TimeManager llama OnHourChanged cuando coincide con eventHours. Asegura tener 0 en DayNightCycle.
            int h = Mathf.FloorToInt(hour);
            if (h == 0)
            {
                _currentDayIndex++;
                ResetDailyBudget();
            }
        }

        void TickDrip()
        {
            // goteo basado en enemigos por minuto de juego
            bool isDay = _time.IsDay();
            float perMinute = isDay ? RuleSet.DripPerMinuteDay : RuleSet.DripPerMinuteNight;
            if (perMinute <= 0f) return;

            // Convertimos tickSeconds (tiempo real) a minutos de juego: usamos GetCurrentCycleTime->24h
            // timeSpeedMultiplier ya afecta a TimeManager; aquí solo sumamos por frame
            float gameMinutesPerSecond = 24f * 60f / _time.GetCurrentCycleDuration();
            float gameMinutesThisTick = gameMinutesPerSecond * TickSeconds;
            _dripAccumulator += perMinute * gameMinutesThisTick;

            int toSpawn = Mathf.FloorToInt(_dripAccumulator);
            if (toSpawn <= 0) return;
            _dripAccumulator -= toSpawn;

            SpawnBatch(toSpawn, isDay, isWave:false);
        }

        void TickWaves()
        {
            if (RuleSet.WaveWindows == null || RuleSet.WaveWindows.Count == 0) return;

            float hour = _time.GetCurrentGameHour();
            bool isDay = _time.IsDay();

            foreach (var win in RuleSet.WaveWindows)
            {
                if (!IsHourInWindow(hour, win.StartHour, win.EndHour)) continue;
                if (win.PeriodAllowed != SpawnPeriodAllowed.Any)
                {
                    if (win.PeriodAllowed == SpawnPeriodAllowed.DayOnly && !isDay) continue;
                    if (win.PeriodAllowed == SpawnPeriodAllowed.NightOnly && isDay) continue;
                }

                // Usamos una temporización por ventana simple: derivamos de OnHourChanged + TickSeconds acumulado
                // Para KISS: lanzamos una oleada cuando el minuto exacto múltiplo de IntervalMinutes ocurre.
                float minutes = (hour - Mathf.Floor(hour)) * 60f;
                if (Mathf.Abs((minutes % win.IntervalMinutes)) < (TickSeconds * 60f / _time.GetCurrentCycleDuration() * 24f))
                {
                    int size = _rng.Next(win.MinWaveSize, win.MaxWaveSize + 1);
                    SpawnBatch(size, isDay, isWave:true);
                }
            }
        }

        bool IsHourInWindow(float hour, float start, float end)
        {
            if (Mathf.Approximately(start, end)) return true; // todo el día
            if (start < end) return hour >= start && hour < end;
            // cruza medianoche
            return hour >= start || hour < end;
        }

        void SpawnBatch(int count, bool isDay, bool isWave)
        {
            if (count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                if (!CanSpawnMoreGlobal()) break;
                if (_dailyBudgetRemaining <= 0) break;

                var point = PickValidPoint(isDay);
                if (point == null) break;

                var enemyOption = PickEnemyForPeriod(isDay);
                if (enemyOption == null) break;

                var enemyPrefab = enemyOption.Prefab;

                if (!TryGetSpawnPosition(point, out Vector3 pos))
                {
                    continue;
                }

                if (!ValidatePointWithPlayer(point, pos))
                {
                    continue;
                }

                var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
                var tracker = go.AddComponent<SpawnedEnemyTracker>();
                tracker.Manager = this;
                tracker.SourcePoint = point;

                // Asignar EnemyType desde el EnemyOption
                var enemyBrain = go.GetComponent<Unity.FPS.AI.EnemyBrain>();
                if (enemyBrain != null)
                {
                    enemyBrain.enemyType = enemyOption.EnemyType;
                    if (point.AssignedPatrolPath != null)
                    {
                        enemyBrain.patrolPath = point.AssignedPatrolPath;
                    }
                }

                point.AliveCount++;
                point.BeginCooldown(this);
                _dailyBudgetRemaining = Mathf.Max(0, _dailyBudgetRemaining - 1);

                if (LogSpawns)
                {
                    Debug.Log($"[Spawn] {(isWave ? "Wave" : "Drip")} {go.name} at {point.name}  BudgetLeft={_dailyBudgetRemaining}");
                }

                _lastPoint = point;
                _lastEnemyPrefab = enemyPrefab;
            }
        }

        bool CanSpawnMoreGlobal()
        {
            if (RuleSet.MaxConcurrentGlobal <= 0) return true;
            int current = EnemyManager != null ? EnemyManager.NumberOfEnemiesRemaining : 0;
            return current < RuleSet.MaxConcurrentGlobal;
        }

        EnemySpawnPoint PickValidPoint(bool isDay)
        {
            var candidates = _points.Where(p => p != null
                                                && !p.IsCoolingDown
                                                && p.CanSpawnForPeriod(isDay)
                                                && !ReachedPointAliveLimit(p)).ToList();
            if (candidates.Count == 0) return null;

            // Filtrar por distancia si MaxDistanceToPlayer > 0
            candidates = candidates.Where(p => WithinOptionalMaxDistance(p.transform.position)).ToList();
            if (candidates.Count == 0) return null;

            // Aplicar jitter de puntos y evitar repetición inmediata si está habilitado
            float totalWeight = 0f;
            var weights = new Dictionary<EnemySpawnPoint, float>(candidates.Count);
            foreach (var p in candidates)
            {
                if (AvoidImmediateRepeatPoint && p == _lastPoint && candidates.Count > 1)
                {
                    weights[p] = 0f; // desprioriza totalmente solo si hay otras opciones
                    continue;
                }
                float w = Mathf.Max(0.0001f, p.Weight);
                if (PointWeightJitterPercent > 0f)
                {
                    float jitter = (float)(_rng.NextDouble() * 2.0 - 1.0); // [-1,1]
                    w *= Mathf.Clamp01(1f + jitter * PointWeightJitterPercent);
                }
                weights[p] = w;
                totalWeight += w;
            }
            if (totalWeight <= 0f)
            {
                return candidates[0];
            }
            float roll = (float)(_rng.NextDouble()) * totalWeight;
            float acc = 0f;
            foreach (var p in candidates)
            {
                acc += weights[p];
                if (roll <= acc) return p;
            }
            return candidates[candidates.Count - 1];
        }

        bool ReachedPointAliveLimit(EnemySpawnPoint p)
        {
            if (p.MaxAliveFromThisPoint <= 0) return false;
            return p.AliveCount >= p.MaxAliveFromThisPoint;
        }

        bool WithinOptionalMaxDistance(Vector3 pos)
        {
            if (Player == null) return true;
            // Si algún punto define MaxDistanceToPlayer, se valida puntualmente durante ValidatePointWithPlayer
            return true;
        }

        EnemyOption PickEnemyForPeriod(bool isDay)
        {
            if (RuleSet.Enemies == null || RuleSet.Enemies.Count == 0) return null;
            var list = RuleSet.Enemies.Where(e => e != null && e.Prefab != null &&
                                                  (e.AllowedPeriod == SpawnPeriodAllowed.Any ||
                                                   (e.AllowedPeriod == SpawnPeriodAllowed.DayOnly && isDay) ||
                                                   (e.AllowedPeriod == SpawnPeriodAllowed.NightOnly && !isDay)))
                                       .ToList();
            if (list.Count == 0) return null;
            // Evitar repetición inmediata si aplica
            if (AvoidImmediateRepeatEnemy && _lastEnemyPrefab != null && list.Count > 1)
            {
                list = list.Where(e => e.Prefab != _lastEnemyPrefab).ToList();
                if (list.Count == 0) return null;
            }

            float total = 0f;
            var weights = new Dictionary<EnemyOption, float>(list.Count);
            foreach (var e in list)
            {
                float w = Mathf.Max(0.0001f, e.Weight);
                if (e.WeightJitterPercent > 0f)
                {
                    float jitter = (float)(_rng.NextDouble() * 2.0 - 1.0); // [-1,1]
                    w *= Mathf.Clamp01(1f + jitter * e.WeightJitterPercent);
                }
                weights[e] = w;
                total += w;
            }
            float roll = (float)(_rng.NextDouble()) * total;
            float acc = 0f;
            foreach (var e in list)
            {
                acc += weights[e];
                if (roll <= acc) return e;
            }
            return list[list.Count - 1];
        }

        bool TryGetSpawnPosition(EnemySpawnPoint p, out Vector3 position)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 randomPoint = p.transform.position + UnityEngine.Random.insideUnitSphere * p.SpawnRadius;
                
                if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out var hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    if (DebugSpawns) 
                    {
                        Debug.Log($"[SpawnDebug] Intento {i+1}/10 para '{p.name}': Punto aleatorio {randomPoint} -> ÉXITO. Posición final en NavMesh: {hit.position}");
                        Debug.DrawLine(randomPoint, hit.position, Color.green, 2f);
                    }
                    position = hit.position;
                    return true;
                }
                else
                {
                    if (DebugSpawns)
                    {
                        Debug.Log($"[SpawnDebug] Intento {i+1}/10 para '{p.name}': Punto aleatorio {randomPoint} -> FALLO. No se encontró NavMesh cercano.");
                        Debug.DrawRay(randomPoint, Vector3.up, Color.red, 2f);
                    }
                }
            }

            if (LogSpawns) 
            {
                Debug.LogWarning($"[Spawn] No se pudo encontrar una posición válida en el NavMesh cerca de {p.name} después de 10 intentos.");
            }
            
            position = Vector3.zero;
            return false;
        }

        bool ValidatePointWithPlayer(EnemySpawnPoint p, Vector3 pos)
        {
            if (Player == null) return true;
            float dist = Vector3.Distance(Player.position, pos);
            if (dist < p.MinDistanceToPlayer) return false;
            if (p.MaxDistanceToPlayer > 0f && dist > p.MaxDistanceToPlayer) return false;

            if (p.AvoidPlayerLineOfSight)
            {
                Vector3 dir = (Player.position - pos).normalized;
                if (!Physics.Raycast(pos + Vector3.up * 1.0f, dir, out RaycastHit hit, Mathf.Max(1f, dist), ObstacleLayers))
                {
                    // No impactó obstáculo: potencialmente en LOS directa. Rechaza para evitar pops.
                    return false;
                }
            }
            return true;
        }

        // Llamado por SpawnedEnemyTracker cuando muere un enemigo
        public void OnEnemyFromPointDied(EnemySpawnPoint point)
        {
            if (point != null)
            {
                point.AliveCount = Mathf.Max(0, point.AliveCount - 1);
            }
        }
    }
}

/*
Metadata
ScriptRole: Manager central de spawn: goteo por minuto y oleadas por ventana horaria; aplica límites globales y por punto.
RelatedScripts: EnemySpawnPoint, SpawnRuleSet, SpawnedEnemyTracker, TimeManager, EnemyManager
UsesSO: SpawnRuleSet
ReceivesFrom / SendsTo: Recibe OnDayNightChanged/OnHourChanged (TimeManager). No emite eventos globales.
Setup: Añadir a un GameObject vacío en escena (p.ej., _Spawning). Asignar RuleSet, EnemyManager, Player (Transform). Colocar varios EnemySpawnPoint en la escena.
*/