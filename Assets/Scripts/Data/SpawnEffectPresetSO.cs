using UnityEngine;

namespace GallinasFelices.Data
{
    [CreateAssetMenu(fileName = "SpawnEffectPreset", menuName = "Gallinas Felices/VFX/Spawn Effect Preset")]
    public class SpawnEffectPresetSO : ScriptableObject
    {
        [Header("Animation")]
        public float duration = 0.7f;
        public float bounceHeight = 0.8f;
        public float scaleOvershoot = 1.3f;

        [Header("Rotation")]
        public bool useRotation = true;
        public float rotationAmount = 360f;

        [Header("Squash & Stretch")]
        public bool useSquashStretch = true;
        public float squashAmount = 0.15f;
        public float squashDuration = 0.15f;

        [Header("Secondary Effects")]
        public bool usePunchScale = false;
        public float punchStrength = 0.3f;
        public int punchVibrato = 10;

        [Header("Particles")]
        public bool spawnParticles = true;
        public GameObject particlePrefab;
        public int particleCount = 12;
        public float particleSpeed = 3f;
        public Color particleColor = Color.yellow;

        [Header("Audio")]
        public bool playSound = true;
        public AudioClip spawnSound;
        [Range(0f, 1f)] public float soundVolume = 0.7f;

        public void ApplyToEffect(VFX.SpawnEffect effect)
        {
            if (effect == null) return;

            var type = effect.GetType();

            SetField(type, effect, "duration", duration);
            SetField(type, effect, "bounceHeight", bounceHeight);
            SetField(type, effect, "scaleOvershoot", scaleOvershoot);

            SetField(type, effect, "useRotation", useRotation);
            SetField(type, effect, "rotationAmount", rotationAmount);

            SetField(type, effect, "useSquashStretch", useSquashStretch);
            SetField(type, effect, "squashAmount", squashAmount);
            SetField(type, effect, "squashDuration", squashDuration);

            SetField(type, effect, "usePunchScale", usePunchScale);
            SetField(type, effect, "punchStrength", punchStrength);
            SetField(type, effect, "punchVibrato", punchVibrato);

            SetField(type, effect, "spawnParticles", spawnParticles);
            SetField(type, effect, "particlePrefab", particlePrefab);
            SetField(type, effect, "particleCount", particleCount);
            SetField(type, effect, "particleSpeed", particleSpeed);
            SetField(type, effect, "particleColor", particleColor);

            SetField(type, effect, "playSound", playSound);
            SetField(type, effect, "spawnSound", spawnSound);
            SetField(type, effect, "soundVolume", soundVolume);
        }

        private void SetField(System.Type type, object instance, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
            }
        }
    }
}
