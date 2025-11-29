using UnityEngine;
using DG.Tweening;
using GallinasFelices.Data;

namespace GallinasFelices.VFX
{
    public class SpawnEffect : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SpawnEffectPresetSO preset;
        [SerializeField] private bool playOnStart = true;

        [Header("References (Optional)")]
        [SerializeField] private Transform visualRoot;
        [Tooltip("Si está asignado, anima solo este transform. Si está vacío, anima el GameObject raíz.")]

        private float duration;
        private float bounceHeight;
        private float scaleOvershoot;
        private bool useRotation;
        private float rotationAmount;
        private bool useSquashStretch;
        private float squashAmount;
        private float squashDuration;
        private bool usePunchScale;
        private float punchStrength;
        private int punchVibrato;
        private bool spawnParticles;
        private GameObject particlePrefab;
        private int particleCount;
        private float particleSpeed;
        private Color particleColor;
        private bool playSound;
        private AudioClip spawnSound;
        private float soundVolume;

        private Vector3 originalScale;
        private Vector3 startPosition;
        private Sequence mainSequence;

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }
        }

        private void Start()
        {
            if (!playOnStart) return;

            if (preset == null)
            {
                Debug.LogWarning($"[SpawnEffect] No preset assigned on {gameObject.name}. Effect will not play.", this);
                return;
            }

            LoadFromPreset(preset);
            Play();
        }

        private void LoadFromPreset(SpawnEffectPresetSO presetSO)
        {
            duration = presetSO.duration;
            bounceHeight = presetSO.bounceHeight;
            scaleOvershoot = presetSO.scaleOvershoot;
            useRotation = presetSO.useRotation;
            rotationAmount = presetSO.rotationAmount;
            useSquashStretch = presetSO.useSquashStretch;
            squashAmount = presetSO.squashAmount;
            squashDuration = presetSO.squashDuration;
            usePunchScale = presetSO.usePunchScale;
            punchStrength = presetSO.punchStrength;
            punchVibrato = presetSO.punchVibrato;
            spawnParticles = presetSO.spawnParticles;
            particlePrefab = presetSO.particlePrefab;
            particleCount = presetSO.particleCount;
            particleSpeed = presetSO.particleSpeed;
            particleColor = presetSO.particleColor;
            playSound = presetSO.playSound;
            spawnSound = presetSO.spawnSound;
            soundVolume = presetSO.soundVolume;
        }

        public void Play()
        {
            originalScale = visualRoot.localScale;
            startPosition = transform.position;

            visualRoot.localScale = Vector3.zero;

            mainSequence = DOTween.Sequence();

            AnimateScale();
            AnimateBounce();

            if (useRotation)
            {
                AnimateRotation();
            }

            if (useSquashStretch)
            {
                AnimateSquashStretch();
            }

            if (spawnParticles)
            {
                SpawnParticlesBurst();
            }

            if (playSound && spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, transform.position, soundVolume);
            }

            mainSequence.OnComplete(() => {
                if (usePunchScale)
                {
                    visualRoot.DOPunchScale(Vector3.one * punchStrength, 0.3f, punchVibrato);
                }
                Destroy(this);
            });
        }

        public void PlayAndDestroy(float destroyDelay = 0f)
        {
            Play();
            if (destroyDelay > 0f)
            {
                Destroy(gameObject, duration + destroyDelay);
            }
        }

        private void AnimateScale()
        {
            Tween scaleTween = visualRoot.DOScale(originalScale * scaleOvershoot, duration * 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    visualRoot.DOScale(originalScale, duration * 0.3f).SetEase(Ease.OutQuad);
                });

            mainSequence.Join(scaleTween);
        }

        private void AnimateBounce()
        {
            Vector3 targetPos = startPosition + Vector3.up * bounceHeight;
            
            Tween bounceTween = transform.DOMove(targetPos, duration * 0.4f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    transform.DOMove(startPosition, duration * 0.6f).SetEase(Ease.OutBounce);
                });

            mainSequence.Join(bounceTween);
        }

        private void AnimateRotation()
        {
            float randomRotation = Random.Range(rotationAmount * 0.8f, rotationAmount * 1.2f);

            Tween rotationTween = visualRoot.DORotate(
                new Vector3(0f, randomRotation, 0f),
                duration,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutQuad);

            mainSequence.Join(rotationTween);
        }

        private void AnimateSquashStretch()
        {
            float landTime = duration * 0.4f + duration * 0.3f;

            DOVirtual.DelayedCall(landTime, () => {
                Vector3 squashScale = new Vector3(
                    originalScale.x * (1f + squashAmount),
                    originalScale.y * (1f - squashAmount),
                    originalScale.z * (1f + squashAmount)
                );

                visualRoot.DOScale(squashScale, squashDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        visualRoot.DOScale(originalScale, squashDuration * 0.5f)
                            .SetEase(Ease.OutElastic);
                    });
            });
        }

        private void SpawnParticlesBurst()
        {
            if (particlePrefab != null)
            {
                GameObject particleObj = Instantiate(particlePrefab, startPosition, Quaternion.identity);
                ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
                
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = particleColor;
                    ps.Play();
                    Destroy(particleObj, main.duration + main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(particleObj, 2f);
                }
            }
            else
            {
                CreateSimpleParticles();
            }
        }

        private void CreateSimpleParticles()
        {
            GameObject particleContainer = new GameObject("SpawnParticles");
            particleContainer.transform.position = startPosition;

            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.SetParent(particleContainer.transform);
                particle.transform.position = startPosition + Vector3.up * 0.2f;
                particle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);

                Renderer renderer = particle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                    mat.color = particleColor;
                    renderer.material = mat;
                }

                Destroy(particle.GetComponent<Collider>());

                float angle = (360f / particleCount) * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                direction += Vector3.up * Random.Range(0.3f, 0.8f);
                direction = direction.normalized;

                Vector3 targetPos = startPosition + direction * Random.Range(0.5f, 1.2f);

                particle.transform.DOMove(targetPos, 0.5f)
                    .SetEase(Ease.OutQuad);

                particle.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InQuad);

                renderer.material.DOFade(0f, 0.5f);
            }

            Destroy(particleContainer, 1f);
        }

        public void SetVisualRoot(Transform root)
        {
            visualRoot = root;
        }

        public void SetPreset(SpawnEffectPresetSO presetSO)
        {
            preset = presetSO;
            if (presetSO != null)
            {
                LoadFromPreset(presetSO);
            }
        }

        private void OnDestroy()
        {
            mainSequence?.Kill();
        }
    }

    public enum SpawnEffectPreset
    {
        Subtle,
        Normal,
        Flashy
    }
}
