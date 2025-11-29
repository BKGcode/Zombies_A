using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using GallinasFelices.Chicken;

namespace GallinasFelices
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ChickenPeckingBehavior : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float peckAngle = 45f;
        [SerializeField] private float peckDuration = 0.3f;
        [SerializeField] private int minPecks = 2;
        [SerializeField] private int maxPecks = 4;

        private NavMeshAgent agent;
        private Transform visualRoot;
        private Chicken.Chicken chicken;
        private bool isPecking;
        private int currentPeckCount;
        private int targetPeckCount;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            chicken = GetComponent<Chicken.Chicken>();
        }

        void Start()
        {
            visualRoot = transform.Find("VisualRoot");
            if (visualRoot == null && transform.childCount > 0)
            {
                visualRoot = transform.GetChild(0);
            }
            
            if (visualRoot == null)
            {
                Debug.LogWarning($"[ChickenPeckingBehavior] No se encontró VisualRoot en {gameObject.name}. Componente desactivado.", this);
                enabled = false;
            }
        }

        void Update()
        {
            if (visualRoot == null) return;
            
            if (chicken.CurrentState == ChickenState.LayingEgg)
            {
                if (isPecking) StopPecking();
                return;
            }
            
            if (agent.velocity.magnitude < 0.2f && !isPecking)
            {
                StartPecking();
            }
            else if (agent.velocity.magnitude >= 0.2f && isPecking)
            {
                StopPecking();
            }
        }

        void StartPecking()
        {
            isPecking = true;
            currentPeckCount = 0;
            targetPeckCount = Random.Range(minPecks, maxPecks + 1);
            Peck();
        }

        void Peck()
        {
            if (!isPecking) return;

            Vector3 currentRotation = visualRoot.localEulerAngles;
            visualRoot.DOLocalRotate(new Vector3(peckAngle, currentRotation.y, 0), peckDuration * 0.6f)
                .OnComplete(() =>
                {
                    currentRotation = visualRoot.localEulerAngles;
                    visualRoot.DOLocalRotate(new Vector3(0, currentRotation.y, 0), peckDuration * 0.4f)
                        .OnComplete(() =>
                        {
                            if (!isPecking) return;

                            currentPeckCount++;
                            if (currentPeckCount >= targetPeckCount)
                            {
                                RotateAndPeckAgain();
                            }
                            else
                            {
                                Peck();
                            }
                        });
                });
        }

        void RotateAndPeckAgain()
        {
            float randomYRotation = Random.Range(30f, 90f) * (Random.value > 0.5f ? 1f : -1f);
            Vector3 currentRotation = visualRoot.localEulerAngles;
            
            visualRoot.DOLocalRotate(new Vector3(0, currentRotation.y + randomYRotation, 0), 0.3f)
                .OnComplete(() =>
                {
                    if (isPecking)
                    {
                        currentPeckCount = 0;
                        targetPeckCount = Random.Range(minPecks, maxPecks + 1);
                        Peck();
                    }
                });
        }

        void StopPecking()
        {
            isPecking = false;
            visualRoot.DOKill();
            visualRoot.localRotation = Quaternion.identity;
        }

        void OnDestroy()
        {
            visualRoot.DOKill();
        }
    }
}
