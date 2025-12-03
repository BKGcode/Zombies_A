using UnityEngine;
using DG.Tweening;
using GallinasFelices.Chicken;

namespace GallinasFelices.VFX
{
    public class ChickenSleepBreathingAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Chicken.Chicken chicken;
        [SerializeField] private Transform visualRoot;

        [Header("Breathing Settings")]
        [SerializeField] private Vector3 breatheScale = new Vector3(0.95f, 1.05f, 0.95f);
        [SerializeField] private float breatheDuration = 2f;
        [SerializeField] private Ease breatheEase = Ease.InOutSine;

        [Header("Timing")]
        [SerializeField] private float minTimeBetweenBreaths = 3f;
        [SerializeField] private float maxTimeBetweenBreaths = 8f;

        private float breathTimer;
        private Sequence currentSequence;

        private void Awake()
        {
            if (chicken == null)
            {
                chicken = GetComponent<Chicken.Chicken>();
            }

            if (visualRoot == null && chicken != null)
            {
                visualRoot = chicken.VisualRoot;
            }

            if (visualRoot == null)
            {
                enabled = false;
            }
        }

        private void Start()
        {
            ResetBreathTimer();
        }

        private void Update()
        {
            if (chicken.CurrentState != ChickenState.Sleeping)
            {
                StopBreathing();
                return;
            }

            breathTimer -= Time.deltaTime;
            if (breathTimer <= 0f)
            {
                PlayBreathAnimation();
                ResetBreathTimer();
            }
        }

        private void PlayBreathAnimation()
        {
            if (visualRoot == null) return;

            visualRoot.DOKill();
            
            currentSequence = DOTween.Sequence();
            currentSequence.Append(visualRoot.DOScale(breatheScale, breatheDuration * 0.5f).SetEase(breatheEase));
            currentSequence.Append(visualRoot.DOScale(Vector3.one, breatheDuration * 0.5f).SetEase(breatheEase));
        }

        private void StopBreathing()
        {
            if (visualRoot != null)
            {
                visualRoot.DOKill();
                visualRoot.localScale = Vector3.one;
            }
        }

        private void ResetBreathTimer()
        {
            breathTimer = Random.Range(minTimeBetweenBreaths, maxTimeBetweenBreaths);
        }

        private void OnDestroy()
        {
            StopBreathing();
        }

        private void OnDisable()
        {
            StopBreathing();
        }
    }
}
