using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPS.Spawning
{
    public enum SpawnPeriodAllowed
    {
        Any,
        DayOnly,
        NightOnly
    }

    [Serializable]
    public class EnemyOption
    {
        [Tooltip("Prefab del enemigo a spawnear")]
        public GameObject Prefab;

        [Tooltip("ScriptableObject que define el tipo de enemigo (salud, velocidad, etc.)")]
        public Unity.FPS.AI.EnemyTypeSO EnemyType;

        [Tooltip("Peso relativo para selección aleatoria (mayor = más frecuente)")]
        [Min(0f)] public float Weight = 1f;

        [Tooltip("Variación porcentual del peso por selección (0-1). 0.2 = ±20% de jitter")]
        [Range(0f, 1f)] public float WeightJitterPercent = 0f;

        [Tooltip("Periodo permitido para este enemigo")]
        public SpawnPeriodAllowed AllowedPeriod = SpawnPeriodAllowed.Any;
    }

    [Serializable]
    public class WaveWindow
    {
        [Tooltip("Nombre descriptivo de la ventana de oleadas")]
        public string Name = "Noche - Oleadas";

        [Tooltip("Hora de inicio (0-24)")]
        [Range(0f, 24f)] public float StartHour = 20f;

        [Tooltip("Hora de fin (0-24). Si es menor a StartHour, se asume cruce de medianoche.")]
        [Range(0f, 24f)] public float EndHour = 5f;

        [Tooltip("Tamaño mínimo de cada oleada")]
        [Min(0)] public int MinWaveSize = 3;

        [Tooltip("Tamaño máximo de cada oleada")]
        [Min(0)] public int MaxWaveSize = 6;

        [Tooltip("Intervalo entre oleadas en minutos de juego")]
        [Min(0.1f)] public float IntervalMinutes = 10f;

        [Tooltip("Periodo permitido adicional (opcional)")]
        public SpawnPeriodAllowed PeriodAllowed = SpawnPeriodAllowed.Any;
    }

    [CreateAssetMenu(menuName = "FPS/Spawning/Spawn Rule Set", fileName = "SpawnRuleSet")]
    public class SpawnRuleSet : ScriptableObject
    {
        [Header("Tasas de goteo (enemigos/minuto de juego)")]
        [Tooltip("Tasa base durante el día")]
        [Min(0f)] public float DripPerMinuteDay = 1f;

        [Tooltip("Tasa base durante la noche")]
        [Min(0f)] public float DripPerMinuteNight = 4f;

        [Header("Límites globales")]
        [Tooltip("Máximo de enemigos simultáneos en escena (0 = ilimitado)")]
        [Min(0)] public int MaxConcurrentGlobal = 40;

        [Tooltip("Presupuesto diario de spawns (0 = ilimitado). Se reinicia a medianoche")]
        [Min(0)] public int DailyBudget = 120;

        [Tooltip("Escalado del presupuesto diario por día de juego (porcentaje). 10 = +10% cada día")]
        public float DailyScalePercent = 15f;

        [Header("Oleadas (ventanas)")]
        public List<WaveWindow> WaveWindows = new List<WaveWindow>
        {
            new WaveWindow()
        };

        [Header("Catálogo de enemigos (selección ponderada)")]
        public List<EnemyOption> Enemies = new List<EnemyOption>();

        [Header("Aleatoriedad")]
        [Tooltip("Usar semilla personalizada para reproducibilidad")]
        public bool UseCustomSeed = false;

        [Tooltip("Semilla aleatoria (solo si UseCustomSeed=true)")]
        public int RandomSeed = 12345;
    }
}

/*
Metadata
ScriptRole: Define las reglas de spawn: tasas por periodo, ventanas de oleadas, límites y catálogo de enemigos.
RelatedScripts: EnemySpawnManager, EnemySpawnPoint, SpawnedEnemyTracker
UsesSO: SpawnRuleSet (este script)
ReceivesFrom / SendsTo: N/A (datos puros)
Setup: Crear asset en Project: Create → FPS/Spawning/Spawn Rule Set. Asignar en EnemySpawnManager.
*/
