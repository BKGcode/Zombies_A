using UnityEngine;

namespace GallinasFelices.Chicken
{
    public class ChickenIdleFidget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Chicken chicken;
        [SerializeField] private Transform rotationTarget;

        private float targetAngle;
        private float currentAngle;
        private float fidgetTimer;
        private bool isInitialized;

        private void Awake()
        {
            if (chicken == null)
            {
                chicken = GetComponent<Chicken>();
            }

            if (rotationTarget == null)
            {
                rotationTarget = transform;
            }
        }

        private void Start()
        {
            InitializeFidget();
        }

        private void Update()
        {
            if (!isInitialized || chicken == null || chicken.Personality == null)
            {
                return;
            }

            if (ShouldFidget())
            {
                UpdateFidget();
            }
        }

        private void InitializeFidget()
        {
            if (chicken != null && chicken.Personality != null)
            {
                isInitialized = true;
                PickNewTargetAngle();
            }
        }

        private bool ShouldFidget()
        {
            ChickenState state = chicken.CurrentState;
            
            return state == ChickenState.Idle ||
                   state == ChickenState.Eating ||
                   state == ChickenState.Drinking ||
                   state == ChickenState.LayingEgg;
        }

        private void UpdateFidget()
        {
            fidgetTimer -= Time.deltaTime;

            if (fidgetTimer <= 0f)
            {
                PickNewTargetAngle();
            }

            float rotationSpeed = chicken.Personality.fidgetRotationSpeed;
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.Euler(0f, currentAngle, 0f);
            rotationTarget.localRotation = Quaternion.Slerp(rotationTarget.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        private void PickNewTargetAngle()
        {
            float maxAngle = chicken.Personality.maxFidgetAngle;
            targetAngle = Random.Range(-maxAngle, maxAngle);
            
            float minTime = chicken.Personality.minFidgetChangeTime;
            float maxTime = chicken.Personality.maxFidgetChangeTime;
            fidgetTimer = Random.Range(minTime, maxTime);
        }

        public void SetRotationTarget(Transform target)
        {
            rotationTarget = target;
        }
    }
}
