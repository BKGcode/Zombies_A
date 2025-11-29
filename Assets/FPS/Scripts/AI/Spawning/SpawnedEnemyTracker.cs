using UnityEngine;
using Unity.FPS.Game;

namespace FPS.Spawning
{
    [RequireComponent(typeof(Health))]
    public class SpawnedEnemyTracker : MonoBehaviour
    {
        [HideInInspector] public EnemySpawnManager Manager;
        [HideInInspector] public EnemySpawnPoint SourcePoint;

        private Health _health;

        void Awake()
        {
            _health = GetComponent<Health>();
            if (_health != null)
            {
                _health.OnDie += OnDie;
            }
        }

        void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDie -= OnDie;
            }
        }

        void OnDie()
        {
            if (Manager != null)
            {
                Manager.OnEnemyFromPointDied(SourcePoint);
            }
        }
    }
}

/*
Metadata
ScriptRole: Vincula enemigos instanciados con su punto de spawn y notifica al manager al morir.
RelatedScripts: EnemySpawnManager, EnemySpawnPoint, Health
UsesSO: N/A
ReceivesFrom / SendsTo: Recibe OnDie de Health; llama Manager.OnEnemyFromPointDied.
Setup: Se añade automáticamente por el EnemySpawnManager al instanciar. Requiere Health en el prefab enemigo.
*/
