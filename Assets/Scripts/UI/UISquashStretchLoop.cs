using UnityEngine;
using DG.Tweening;

namespace GallinasFelices.UI
{
    public class UISquashStretchLoop : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Vector3 squashScale = new Vector3(0.95f, 1.05f, 1f);
        [SerializeField] private Vector3 stretchScale = new Vector3(1.05f, 0.95f, 1f);
        [SerializeField] private float cycleDuration = 1f;
        [SerializeField] private Ease easeType = Ease.InOutSine;

        [Header("Loop Settings")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private float delayBeforeStart = 0f;

        private RectTransform rectTransform;
        private Sequence currentSequence;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            if (rectTransform == null)
            {
                enabled = false;
            }
        }

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
            currentSequence.Append(rectTransform.DOScale(squashScale, cycleDuration * 0.5f).SetEase(easeType));
            currentSequence.Append(rectTransform.DOScale(stretchScale, cycleDuration * 0.5f).SetEase(easeType));
            currentSequence.SetLoops(-1, LoopType.Yoyo);
        }

        public void StopAnimation()
        {
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
            }
            
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
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
