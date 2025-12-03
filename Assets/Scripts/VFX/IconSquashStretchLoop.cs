using UnityEngine;
using DG.Tweening;

namespace GallinasFelices.VFX
{
    public class IconSquashStretchLoop : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Vector3 squashScale = new Vector3(0.95f, 1.05f, 1f);
        [SerializeField] private Vector3 stretchScale = new Vector3(1.05f, 0.95f, 1f);
        [SerializeField] private float cycleDuration = 1f;
        [SerializeField] private Ease easeType = Ease.InOutSine;

        [Header("Loop Settings")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private float delayBeforeStart = 0f;

        private Sequence currentSequence;

        private void Start()
        {
            if (playOnStart)
            {
                if (delayBeforeStart > 0f)
                {
                    Invoke(nameof(StartAnimation), delayBeforeStart);
                }
                else
                {
                    StartAnimation();
                }
            }
        }

        public void StartAnimation()
        {
            StopAnimation();
            
            currentSequence = DOTween.Sequence();
            currentSequence.Append(transform.DOScale(squashScale, cycleDuration * 0.5f).SetEase(easeType));
            currentSequence.Append(transform.DOScale(stretchScale, cycleDuration * 0.5f).SetEase(easeType));
            currentSequence.SetLoops(-1, LoopType.Yoyo);
        }

        public void StopAnimation()
        {
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
            }
            
            transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        private void OnDisable()
        {
            StopAnimation();
        }
    }
}
