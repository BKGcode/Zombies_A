using System.Collections;
using UnityEngine;
using Unity.FPS.AI;

namespace FPS.Spawning
{
    public enum SpawnPointState
    {
        Idle,
        CoolingDown,
    }

    public class EnemySpawnPoint : MonoBehaviour
    {
        [Header("Condiciones de tiempo")]
        [Tooltip("Cuándo puede generar enemigos este punto")]
        public SpawnPeriodAllowed AllowedPeriod = SpawnPeriodAllowed.Any;

        [Header("Selección y validación")]
        [Tooltip("Peso relativo para elegir este punto (mayor = más probable)")]
        [Min(0f)] public float Weight = 1f;

        [Tooltip("Radio en el que se puede ubicar el spawn (NavMesh opcional)")]
        [Min(0f)] public float SpawnRadius = 2f;

        [Tooltip("Distancia mínima al jugador para permitir spawn")]
        [Min(0f)] public float MinDistanceToPlayer = 15f;

        [Tooltip("Distancia máxima al jugador para priorizar este punto (0 = sin límite)")]
        [Min(0f)] public float MaxDistanceToPlayer = 0f;

        [Tooltip("Evitar línea de visión directa con el jugador (raycast)")]
        public bool AvoidPlayerLineOfSight = true;

    [Header("Patrullaje")]
    [Tooltip("Ruta de patrulla que seguirán los enemigos generados en este punto. Si es nulo, no patrullarán.")]
    public PatrolPath AssignedPatrolPath;

    [Header("Límites por punto")]
    [Tooltip("Cooldown mínimo entre spawns de este punto (segundos de juego real)")]
    [Min(0f)] public float MinCooldownSeconds = 3f;

    [Tooltip("Cooldown máximo entre spawns de este punto (segundos de juego real)")]
    [Min(0f)] public float MaxCooldownSeconds = 7f;

        [Tooltip("Máximo de enemigos vivos asociados a este punto (0 = ilimitado)")]
        [Min(0)] public int MaxAliveFromThisPoint = 0;

        [Header("Debug")]
        public Color GizmoColorDay = new Color(1f, 0.9f, 0.2f, 0.5f);
        public Color GizmoColorNight = new Color(0.2f, 0.5f, 1f, 0.5f);
        public Color GizmoColorAny = new Color(0.4f, 1f, 0.4f, 0.5f);

        // Estado
        [HideInInspector] public SpawnPointState State = SpawnPointState.Idle;
        [HideInInspector] public int AliveCount = 0;

        private Coroutine cooldownRoutine;

        public bool IsCoolingDown => State == SpawnPointState.CoolingDown;

        public void BeginCooldown(MonoBehaviour host)
        {
            float min = Mathf.Max(0f, MinCooldownSeconds);
            float max = Mathf.Max(min, MaxCooldownSeconds);
            if (max <= 0f) return;
            float cooldown = UnityEngine.Random.Range(min, max);
            if (cooldownRoutine != null) host.StopCoroutine(cooldownRoutine);
            cooldownRoutine = host.StartCoroutine(CooldownCo(cooldown));
        }

        IEnumerator CooldownCo(float seconds)
        {
            State = SpawnPointState.CoolingDown;
            yield return new WaitForSeconds(seconds);
            State = SpawnPointState.Idle;
        }

        public bool CanSpawnForPeriod(bool isDay)
        {
            switch (AllowedPeriod)
            {
                case SpawnPeriodAllowed.Any: return true;
                case SpawnPeriodAllowed.DayOnly: return isDay;
                case SpawnPeriodAllowed.NightOnly: return !isDay;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Color c = AllowedPeriod == SpawnPeriodAllowed.Any ? GizmoColorAny :
                (AllowedPeriod == SpawnPeriodAllowed.DayOnly ? GizmoColorDay : GizmoColorNight);
            Gizmos.color = c;
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }
}

/*
Metadata
ScriptRole: Punto de spawn duplicable con validaciones locales y cooldown.
RelatedScripts: EnemySpawnManager, SpawnedEnemyTracker
UsesSO: N/A
ReceivesFrom / SendsTo: Recibe llamadas de EnemySpawnManager. No emite eventos.
Setup: Crear GameObject vacío, añadir EnemySpawnPoint. Duplicar en escena. Ajustar AllowedPeriod, Weight, SpawnRadius, distancias y cooldown.
*/
