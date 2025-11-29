using UnityEngine;
using DG.Tweening;

namespace GallinasFelices.VFX
{
    public class ClickFeedback : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float squashAmount = 0.85f;
        [SerializeField] private float stretchAmount = 1.15f;
        [SerializeField] private float duration = 0.2f;

        private Transform visualRoot;

        void Awake()
        {
            visualRoot = transform.Find("VisualRoot");
            
            if (visualRoot == null && transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        visualRoot = child;
                        break;
                    }
                }
            }

            if (visualRoot == null)
            {
                if (GetComponent<MeshRenderer>() != null || GetComponent<SkinnedMeshRenderer>() != null)
                {
                    visualRoot = transform;
                }
            }

            if (visualRoot == null)
            {
                Debug.LogWarning($"[ClickFeedback] No se encontró visual root en {gameObject.name}. Usando transform raíz.", this);
                visualRoot = transform;
            }
        }

        public void PlayFeedback()
        {
            visualRoot.DOKill();
            
            Sequence seq = DOTween.Sequence();
            seq.Append(visualRoot.DOScale(new Vector3(stretchAmount, squashAmount, stretchAmount), duration * 0.3f));
            seq.Append(visualRoot.DOScale(new Vector3(squashAmount, stretchAmount, squashAmount), duration * 0.3f));
            seq.Append(visualRoot.DOScale(Vector3.one, duration * 0.4f));
        }
    }
}
