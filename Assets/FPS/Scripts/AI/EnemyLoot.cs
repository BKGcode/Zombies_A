
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(Health))]
    public class EnemyLoot : MonoBehaviour
    {
        [Header("Loot Settings")]
        [Tooltip("The object this enemy can drop when dying")]
        [SerializeField] private GameObject lootPrefab;

        [Tooltip("The chance the object has to drop (0 = never, 1 = always)")]
        [Range(0, 1)]
        [SerializeField] private float dropRate = 1f;

        private Health m_Health;

        void Awake()
        {
            m_Health = GetComponent<Health>();
        }

        void OnEnable()
        {
            m_Health.OnDie += OnDie;
        }

        void OnDisable()
        {
            m_Health.OnDie -= OnDie;
        }

        private void OnDie()
        {
            if (lootPrefab != null && (dropRate >= 1f || Random.value <= dropRate))
            {
                Instantiate(lootPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}

/* 
# METADATA
ScriptRole: Handles the dropping of loot when the enemy dies.
RelatedScripts: Health.
UsesSO: None.
ReceivesFrom: Health (OnDie).
SendsTo: None.
Setup:
- Attach to the root of the enemy GameObject.
- Assign the 'LootPrefab' and set the 'DropRate'.
- Requires a Health component.
*/
