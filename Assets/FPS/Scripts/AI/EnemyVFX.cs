
using UnityEngine;
using System.Collections.Generic;
using Unity.FPS.Game;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyBrain), typeof(Health))]
    public class EnemyVFX : MonoBehaviour
    {
        [Header("Eye Color")]
        [Tooltip("Material for the eye color. The material must use the URP/Lit shader with Emission enabled.")]
        [SerializeField] private Material eyeColorMaterial;

        [Tooltip("The default color of the bot's eye")]
        [ColorUsage(true, true)]
        [SerializeField] private Color defaultEyeColor;

        [Tooltip("The attack color of the bot's eye")]
        [ColorUsage(true, true)]
        [SerializeField] private Color attackEyeColor;

        [Header("Flash On Hit")]
        [Tooltip("The material used for the body of the enemy. The material must use the URP/Lit shader with Emission enabled.")]
        [SerializeField] private Material bodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")]
        [GradientUsage(true)]
        [SerializeField] private Gradient onHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        [SerializeField] private float flashOnHitDuration = 0.5f;

        [Header("VFX Prefabs")]
        [Tooltip("The VFX prefab spawned when the enemy dies")]
        [SerializeField] private GameObject deathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        [SerializeField] private Transform deathVfxSpawnPoint;
        
        [Tooltip("VFX to play when the enemy detects a target")]
        [SerializeField] private ParticleSystem[] onDetectVfx;

        [Tooltip("VFX to play when the enemy is damaged")]
        [SerializeField] private ParticleSystem[] onDamagedVfx;

        private EnemyBrain m_EnemyBrain;
        private Health m_Health;
        
        private List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
        private MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
        private float m_LastTimeDamaged = float.NegativeInfinity;

        private RendererIndexData m_EyeRendererData;
        private MaterialPropertyBlock m_EyeColorMaterialPropertyBlock;

        private struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                Renderer = renderer;
                MaterialIndex = index;
            }
        }

        void Awake()
        {
            m_EnemyBrain = GetComponent<EnemyBrain>();
            m_Health = GetComponent<Health>();
            
            InitializeMaterials();
        }

        void OnEnable()
        {
            m_EnemyBrain.OnDetectedTarget += OnDetectedTarget;
            m_EnemyBrain.OnLostTarget += OnLostTarget;
            m_Health.OnDamaged += OnDamaged;
            m_Health.OnDie += OnDie;
        }

        void OnDisable()
        {
            m_EnemyBrain.OnDetectedTarget -= OnDetectedTarget;
            m_EnemyBrain.OnLostTarget -= OnLostTarget;
            m_Health.OnDamaged -= OnDamaged;
            m_Health.OnDie -= OnDie;
        }

        void Update()
        {
            UpdateHitFlash();
        }

        private void OnDetectedTarget()
        {
            SetEyeColor(attackEyeColor);
            if(onDetectVfx != null)
            {
                foreach(var vfx in onDetectVfx) vfx.Play();
            }
        }

        private void OnLostTarget()
        {
            SetEyeColor(defaultEyeColor);
            if(onDetectVfx != null)
            {
                foreach(var vfx in onDetectVfx) vfx.Stop();
            }
        }

        private void OnDamaged(float damage, GameObject damageSource)
        {
            m_LastTimeDamaged = Time.time;
            if (onDamagedVfx != null && onDamagedVfx.Length > 0)
            {
                int n = Random.Range(0, onDamagedVfx.Length);
                onDamagedVfx[n].Play();
            }
        }

        private void OnDie()
        {
            if (deathVfx != null)
            {
                var vfx = Instantiate(deathVfx, deathVfxSpawnPoint.position, Quaternion.identity);
                Destroy(vfx, 5f);
            }
        }

        private void InitializeMaterials()
        {
            if (eyeColorMaterial != null)
            {
                m_EyeRendererData = new RendererIndexData();
                foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (renderer.sharedMaterials[i] == eyeColorMaterial)
                        {
                            m_EyeRendererData = new RendererIndexData(renderer, i);
                            break;
                        }
                    }
                    if(m_EyeRendererData.Renderer != null) break;
                }
                m_EyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                SetEyeColor(defaultEyeColor);
            }

            if (bodyMaterial != null)
            {
                m_BodyRenderers = new List<RendererIndexData>();
                foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (renderer.sharedMaterials[i] == bodyMaterial)
                        {
                            m_BodyRenderers.Add(new RendererIndexData(renderer, i));
                        }
                    }
                }
                m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();
            }
        }

        private void SetEyeColor(Color color)
        {
            if (m_EyeRendererData.Renderer != null)
            {
                m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", color);
                m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.MaterialIndex);
            }
        }

        private void UpdateHitFlash()
        {
            if (bodyMaterial == null || onHitBodyGradient == null) return;
            
            float flashValue = (Time.time - m_LastTimeDamaged) / flashOnHitDuration;
            Color currentColor = onHitBodyGradient.Evaluate(flashValue);
            m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            
            foreach (var data in m_BodyRenderers)
            {
                if(data.Renderer) data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);
            }
        }
    }
}

/* 
# METADATA
ScriptRole: Manages all visual effects for an enemy, including eye color, hit flashes, and particle effects for detection, damage, and death.
RelatedScripts: EnemyBrain, Health.
UsesSO: None.
ReceivesFrom: EnemyBrain (OnDetectedTarget, OnLostTarget), Health (OnDamaged, OnDie).
SendsTo: None.
Setup:
- Attach to the root of the enemy GameObject.
- Assign materials for eyes and body if dynamic color changes are desired.
- Assign VFX prefabs and particle systems in the Inspector.
- Requires EnemyBrain and Health components on the same GameObject.
*/
