using UnityEngine;
using UnityEngine.AI;

namespace GallinasFelices.Chicken
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ChickenMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float arrivalDistance = 0.8f;
        [SerializeField] private float maxNavigationTime = 20f;

        private NavMeshAgent agent;
        private float navigationTimer;
        private Vector3 currentDestination;
        private bool hasDestination;

        public bool IsMoving => agent != null && agent.hasPath && agent.remainingDistance > arrivalDistance;
        public bool HasArrived => hasDestination && !agent.pathPending && agent.remainingDistance <= arrivalDistance;
        public Vector3 CurrentDestination => currentDestination;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public bool GoTo(Vector3 destination)
        {
            if (agent == null || !agent.isOnNavMesh)
            {
                return false;
            }

            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                currentDestination = hit.position;
                hasDestination = true;
                navigationTimer = 0f;
                return true;
            }

            return false;
        }

        public void StopMoving()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                hasDestination = false;
                navigationTimer = 0f;
            }
        }

        public bool IsStuck()
        {
            if (!hasDestination)
            {
                return false;
            }

            navigationTimer += Time.deltaTime;
            return navigationTimer > maxNavigationTime;
        }

        public void ResetStuckTimer()
        {
            navigationTimer = 0f;
        }

        private void OnDestroy()
        {
            StopMoving();
        }
    }
}
