using System;
using System.Collections.Generic;
using UnityEngine;

namespace HappyChickens.Debug
{
    public class ChickenMonitorManager : MonoBehaviour
    {
        private static ChickenMonitorManager instance;
        public static ChickenMonitorManager Instance
        {
            get
            {
                if (instance == null && Application.isPlaying)
                {
                    GameObject managerObject = new GameObject("ChickenMonitorManager");
                    instance = managerObject.AddComponent<ChickenMonitorManager>();
                }
                return instance;
            }
        }

        private List<ChickenDebugger> registeredChickens = new List<ChickenDebugger>();
        private List<TransitionLog> transitionHistory = new List<TransitionLog>();
        private List<EventLog> eventHistory = new List<EventLog>();
        private const int MaxTransitionHistory = 100;
        private const int MaxEventHistory = 200;

        public IReadOnlyList<ChickenDebugger> RegisteredChickens => registeredChickens;
        public IReadOnlyList<TransitionLog> TransitionHistory => transitionHistory;
        public IReadOnlyList<EventLog> EventHistory => eventHistory;

        public event Action<ChickenDebugger> OnChickenRegistered;
        public event Action<ChickenDebugger> OnChickenUnregistered;
        public event Action<TransitionLog> OnTransitionLogged;
        public event Action<EventLog> OnEventLogged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                registeredChickens.Clear();
                transitionHistory.Clear();
                eventHistory.Clear();
                OnChickenRegistered = null;
                OnChickenUnregistered = null;
                OnTransitionLogged = null;
                OnEventLogged = null;
                instance = null;
            }
        }

        public void RegisterChicken(ChickenDebugger debugger)
        {
            if (!registeredChickens.Contains(debugger))
            {
                registeredChickens.Add(debugger);
                OnChickenRegistered?.Invoke(debugger);
            }
        }

        public void UnregisterChicken(ChickenDebugger debugger)
        {
            if (registeredChickens.Remove(debugger))
            {
                OnChickenUnregistered?.Invoke(debugger);
            }
        }

        public void LogTransition(string chickenID, string fromState, string toState)
        {
            TransitionLog log = new TransitionLog
            {
                ChickenID = chickenID,
                FromState = fromState,
                ToState = toState,
                Timestamp = Time.time,
                TimeOfDay = DateTime.Now.ToString("HH:mm:ss")
            };
            
            transitionHistory.Add(log);
            
            if (transitionHistory.Count > MaxTransitionHistory)
            {
                transitionHistory.RemoveAt(0);
            }
            
            OnTransitionLogged?.Invoke(log);
            
            DetectAnomalies(log);
        }

        public void LogEvent(string chickenID, string eventType, string message, EventSeverity severity = EventSeverity.Info)
        {
            EventLog log = new EventLog
            {
                ChickenID = chickenID,
                EventType = eventType,
                Message = message,
                Severity = severity,
                Timestamp = Time.time,
                TimeOfDay = DateTime.Now.ToString("HH:mm:ss")
            };
            
            eventHistory.Add(log);
            
            if (eventHistory.Count > MaxEventHistory)
            {
                eventHistory.RemoveAt(0);
            }
            
            OnEventLogged?.Invoke(log);
        }

        private void DetectAnomalies(TransitionLog newLog)
        {
            int simultaneousTransitions = 0;
            float timeWindow = 2f;
            
            foreach (var log in transitionHistory)
            {
                if (Mathf.Abs(log.Timestamp - newLog.Timestamp) < timeWindow && 
                    log.ToState == newLog.ToState)
                {
                    simultaneousTransitions++;
                }
            }
            
            if (simultaneousTransitions >= 5)
            {
                UnityEngine.Debug.LogWarning($"[ChickenMonitor] ANOMALY: {simultaneousTransitions} chickens transitioned to {newLog.ToState} within {timeWindow}s");
            }
        }

        public int GetChickensInState(string stateName)
        {
            int count = 0;
            foreach (var chicken in registeredChickens)
            {
                if (chicken.CurrentState == stateName)
                {
                    count++;
                }
            }
            return count;
        }

        public List<ChickenDebugger> GetChickensWithCriticalNeeds()
        {
            List<ChickenDebugger> critical = new List<ChickenDebugger>();
            foreach (var chicken in registeredChickens)
            {
                if (chicken.HasCriticalNeed)
                {
                    critical.Add(chicken);
                }
            }
            return critical;
        }

        public List<ChickenDebugger> GetStuckChickens()
        {
            List<ChickenDebugger> stuck = new List<ChickenDebugger>();
            foreach (var chicken in registeredChickens)
            {
                if (chicken.IsStuck)
                {
                    stuck.Add(chicken);
                }
            }
            return stuck;
        }

        public Dictionary<string, int> GetStateDistribution()
        {
            Dictionary<string, int> distribution = new Dictionary<string, int>();
            
            foreach (var chicken in registeredChickens)
            {
                if (chicken == null) continue;
                
                string state = chicken.CurrentState;
                if (string.IsNullOrEmpty(state))
                {
                    state = "Unknown";
                }
                
                if (distribution.ContainsKey(state))
                {
                    distribution[state]++;
                }
                else
                {
                    distribution[state] = 1;
                }
            }
            
            return distribution;
        }

        public void ClearHistory()
        {
            transitionHistory.Clear();
            eventHistory.Clear();
        }
    }

    [Serializable]
    public class TransitionLog
    {
        public string ChickenID;
        public string FromState;
        public string ToState;
        public float Timestamp;
        public string TimeOfDay;
    }

    [Serializable]
    public class EventLog
    {
        public string ChickenID;
        public string EventType;
        public string Message;
        public EventSeverity Severity;
        public float Timestamp;
        public string TimeOfDay;
    }

    public enum EventSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}
