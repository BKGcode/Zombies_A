using System;
using UnityEngine;
using UnityEngine.Events;
using GallinasFelices.Data;

namespace GallinasFelices.Core
{
    public enum TimeOfDay
    {
        Morning,
        Day,
        Afternoon,
        Night
    }

    public class TimeController : MonoBehaviour
    {
        public static TimeController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameBalanceSO gameBalance;
        [SerializeField] private Light sunLight;

        [Header("Events")]
        public UnityEvent<TimeOfDay> OnTimeOfDayChanged;
        public UnityEvent<float> OnHourChanged;

        public float CurrentHour { get; private set; }
        public TimeOfDay CurrentTimeOfDay { get; private set; }

        private TimeOfDay previousTimeOfDay;
        private float calculatedNightStart;
        private float calculatedMorningStart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            if (gameBalance == null)
            {
                Debug.LogWarning("[TimeController] GameBalanceSO not assigned!");
                CurrentHour = 6f;
                calculatedNightStart = 20f;
                calculatedMorningStart = 6f;
            }
            else
            {
                CurrentHour = gameBalance.startHour;
                CalculateDayNightThresholds();
            }

            UpdateTimeOfDay();
        }

        private void Update()
        {
            if (gameBalance == null) return;
            AdvanceTime();
        }

        private void AdvanceTime()
        {
            CurrentHour += (Time.deltaTime / gameBalance.secondsPerGameHour);

            while (CurrentHour >= 24f)
            {
                CurrentHour -= 24f;
                UpdateTimeOfDay();
            }

            UpdateTimeOfDay();
            UpdateLighting();
            OnHourChanged?.Invoke(CurrentHour);
        }

        private void UpdateTimeOfDay()
        {
            if (gameBalance == null) return;
            
            TimeOfDay newTimeOfDay = GetTimeOfDayFromHour(CurrentHour);

            if (newTimeOfDay != previousTimeOfDay)
            {
                CurrentTimeOfDay = newTimeOfDay;
                previousTimeOfDay = newTimeOfDay;
                OnTimeOfDayChanged?.Invoke(CurrentTimeOfDay);
            }
        }

        private TimeOfDay GetTimeOfDayFromHour(float hour)
        {
            if (gameBalance == null) return TimeOfDay.Day;

            if (hour >= gameBalance.morningStart && hour < gameBalance.dayStart)
            {
                return TimeOfDay.Morning;
            }
            else if (hour >= gameBalance.dayStart && hour < gameBalance.afternoonStart)
            {
                return TimeOfDay.Day;
            }
            else if (hour >= gameBalance.afternoonStart && hour < gameBalance.nightStart)
            {
                return TimeOfDay.Afternoon;
            }
            else
            {
                return TimeOfDay.Night;
            }
        }

        private void UpdateLighting()
        {
            if (sunLight == null || gameBalance == null)
            {
                return;
            }

            float normalizedTime = CurrentHour / 24f;
            float rotationX = (normalizedTime * 360f) - 90f;
            sunLight.transform.rotation = Quaternion.Euler(rotationX, 170f, 0f);

            float lightIntensity = Mathf.Lerp(gameBalance.nightIntensity, gameBalance.dayIntensity, 
                Mathf.Clamp01((Mathf.Sin((normalizedTime - 0.25f) * Mathf.PI * 2f) + 1f) / 2f));
            sunLight.intensity = lightIntensity;

            if (gameBalance.lightColorGradient != null)
            {
                sunLight.color = gameBalance.lightColorGradient.Evaluate(normalizedTime);
            }

            UpdateSkybox(normalizedTime);
        }

        public bool IsNightTime()
        {
            return CurrentTimeOfDay == TimeOfDay.Night;
        }

        public bool IsDayTime()
        {
            return CurrentTimeOfDay == TimeOfDay.Day || CurrentTimeOfDay == TimeOfDay.Afternoon;
        }

        public float GetNormalizedTime()
        {
            return CurrentHour / 24f;
        }

        private void CalculateDayNightThresholds()
        {
            float totalHours = 24f;
            float nightDuration = totalHours / (gameBalance.dayNightRatio + 1f);
            
            calculatedNightStart = 0f;
            calculatedMorningStart = nightDuration;
            
            gameBalance.morningStart = calculatedMorningStart;
            gameBalance.dayStart = calculatedMorningStart + (totalHours - nightDuration) * 0.25f;
            gameBalance.afternoonStart = calculatedMorningStart + (totalHours - nightDuration) * 0.65f;
            gameBalance.nightStart = calculatedNightStart;
        }

        public float GetNightDuration()
        {
            if (gameBalance == null) return 6f;
            return 24f / (gameBalance.dayNightRatio + 1f);
        }

        private void UpdateSkybox(float normalizedTime)
        {
            if (gameBalance.daySkybox == null || gameBalance.nightSkybox == null)
            {
                return;
            }

            float nightWeight = 0f;
            if (CurrentTimeOfDay == TimeOfDay.Night)
            {
                nightWeight = 1f;
            }
            else if (CurrentTimeOfDay == TimeOfDay.Morning)
            {
                float morningProgress = (CurrentHour - calculatedMorningStart) / 2f;
                nightWeight = 1f - Mathf.Clamp01(morningProgress);
            }
            else if (CurrentTimeOfDay == TimeOfDay.Afternoon)
            {
                float afternoonHours = gameBalance.afternoonStart;
                float nightHours = 24f;
                float transitionStart = nightHours - 2f;
                
                if (CurrentHour >= transitionStart)
                {
                    nightWeight = (CurrentHour - transitionStart) / 2f;
                }
            }

            RenderSettings.skybox.Lerp(gameBalance.daySkybox, gameBalance.nightSkybox, nightWeight);
            DynamicGI.UpdateEnvironment();
        }
    }
}
