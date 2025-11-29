using UnityEngine;

namespace Unity.FPS.AI
{
    [CreateAssetMenu(menuName = "FPS/Enemy/EnemyType", fileName = "EnemyType")]
    public class EnemyTypeSO : ScriptableObject
    {
        [Header("Atributos básicos")]
        public string EnemyName;
        public GameObject Prefab;
        public float MaxHealth = 100f;
        public float MoveSpeed = 3.5f;

        [Header("Patrulla (Ruta)")]
        [Tooltip("Tiempo mínimo de espera en cada punto de la ruta de patrulla")]
        public float WaitTimeMin = 1f;
        [Tooltip("Tiempo máximo de espera en cada punto de la ruta de patrulla")]
        public float WaitTimeMax = 3f;

        [Header("Visión")]
        [Tooltip("Rango de visión del enemigo")]
        public float VisionRange = 20f;
        [Tooltip("Ángulo del cono de visión del enemigo (en grados)")]
        [Range(0, 360)]
        public float VisionAngle = 120f;

        [Header("Combate")]
        [Tooltip("Rango de ataque del enemigo")]
        public float AttackRange = 10f;
    }
}
