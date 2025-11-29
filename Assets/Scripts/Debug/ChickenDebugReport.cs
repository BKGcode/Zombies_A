using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HappyChickens.Debug
{
    public static class ChickenDebugReport
    {
        public static void GenerateReport()
        {
            StringBuilder report = new StringBuilder();
            
            report.AppendLine("=================================================");
            report.AppendLine("       CHICKEN DEBUG REPORT");
            report.AppendLine("=================================================");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Game Time: {Time.time:F2}s");
            report.AppendLine();
            
            GenerateSummarySection(report);
            GenerateStateDistributionSection(report);
            GenerateAnomaliesSection(report);
            GenerateDetailedChickenList(report);
            GenerateTransitionHistorySection(report);
            
            string filePath = SaveReport(report.ToString());
            UnityEngine.Debug.Log($"[ChickenMonitor] Report saved to: {filePath}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(filePath);
            #endif
        }

        private static void GenerateSummarySection(StringBuilder report)
        {
            var chickens = ChickenMonitorManager.Instance.RegisteredChickens;
            
            report.AppendLine("--- SUMMARY ---");
            report.AppendLine($"Total Chickens: {chickens.Count}");
            report.AppendLine($"Chickens with Critical Needs: {ChickenMonitorManager.Instance.GetChickensWithCriticalNeeds().Count}");
            report.AppendLine($"Stuck Chickens: {ChickenMonitorManager.Instance.GetStuckChickens().Count}");
            
            if (chickens.Count > 0)
            {
                float avgHunger = chickens.Average(c => c.Hunger);
                float avgThirst = chickens.Average(c => c.Thirst);
                float avgTiredness = chickens.Average(c => c.Tiredness);
                float avgEnergy = chickens.Average(c => c.Energy);
                
                report.AppendLine($"Average Hunger: {avgHunger:F1}%");
                report.AppendLine($"Average Thirst: {avgThirst:F1}%");
                report.AppendLine($"Average Tiredness: {avgTiredness:F1}%");
                report.AppendLine($"Average Energy: {avgEnergy:F1}%");
            }
            
            report.AppendLine();
        }

        private static void GenerateStateDistributionSection(StringBuilder report)
        {
            var distribution = ChickenMonitorManager.Instance.GetStateDistribution();
            int total = ChickenMonitorManager.Instance.RegisteredChickens.Count;
            
            report.AppendLine("--- STATE DISTRIBUTION ---");
            
            foreach (var kvp in distribution.OrderByDescending(x => x.Value))
            {
                float percentage = total > 0 ? (kvp.Value / (float)total) * 100f : 0f;
                string bar = GenerateBar(percentage);
                report.AppendLine($"{kvp.Key,-25} {kvp.Value,3} ({percentage:F1}%) {bar}");
            }
            
            report.AppendLine();
        }

        private static void GenerateAnomaliesSection(StringBuilder report)
        {
            var criticalChickens = ChickenMonitorManager.Instance.GetChickensWithCriticalNeeds();
            var stuckChickens = ChickenMonitorManager.Instance.GetStuckChickens();
            
            report.AppendLine("--- DETECTED ANOMALIES ---");
            
            if (criticalChickens.Count == 0 && stuckChickens.Count == 0)
            {
                report.AppendLine("No anomalies detected.");
            }
            else
            {
                if (criticalChickens.Count > 0)
                {
                    report.AppendLine($"⚠ CRITICAL NEEDS ({criticalChickens.Count}):");
                    foreach (var chicken in criticalChickens)
                    {
                        report.AppendLine($"  - {chicken.ChickenID}: H={chicken.Hunger:F0} T={chicken.Thirst:F0} Tr={chicken.Tiredness:F0} E={chicken.Energy:F0}");
                    }
                }
                
                if (stuckChickens.Count > 0)
                {
                    report.AppendLine($"⚠ STUCK CHICKENS ({stuckChickens.Count}):");
                    foreach (var chicken in stuckChickens)
                    {
                        report.AppendLine($"  - {chicken.ChickenID}: Pos={chicken.CurrentPosition}, State={chicken.CurrentState}, Time={chicken.TimeInCurrentState:F1}s");
                    }
                }
            }
            
            report.AppendLine();
            
            DetectSynchronizedBehavior(report);
        }

        private static void DetectSynchronizedBehavior(StringBuilder report)
        {
            var transitions = ChickenMonitorManager.Instance.TransitionHistory.ToList();
            
            if (transitions.Count < 5)
            {
                return;
            }
            
            report.AppendLine("--- SYNCHRONIZED BEHAVIOR ANALYSIS ---");
            
            var recentTransitions = transitions.TakeLast(50).ToList();
            var groupedByState = recentTransitions.GroupBy(t => t.ToState);
            
            foreach (var group in groupedByState)
            {
                var timeWindow = group.Select(t => t.Timestamp).ToList();
                if (timeWindow.Count >= 3)
                {
                    float timeSpan = timeWindow.Max() - timeWindow.Min();
                    if (timeSpan < 5f)
                    {
                        report.AppendLine($"⚠ {group.Count()} chickens transitioned to {group.Key} within {timeSpan:F1}s");
                    }
                }
            }
            
            report.AppendLine();
        }

        private static void GenerateDetailedChickenList(StringBuilder report)
        {
            var chickens = ChickenMonitorManager.Instance.RegisteredChickens.OrderBy(c => c.ChickenID).ToList();
            
            report.AppendLine("--- DETAILED CHICKEN LIST ---");
            report.AppendLine($"{"ID",-20} {"State",-25} {"Time",8} {"H",5} {"T",5} {"Tr",5} {"E",5} {"Position",20}");
            report.AppendLine(new string('-', 100));
            
            foreach (var chicken in chickens)
            {
                string posStr = $"({chicken.CurrentPosition.x:F1},{chicken.CurrentPosition.y:F1},{chicken.CurrentPosition.z:F1})";
                report.AppendLine($"{chicken.ChickenID,-20} {chicken.CurrentState,-25} {chicken.TimeInCurrentState,7:F1}s {chicken.Hunger,4:F0}% {chicken.Thirst,4:F0}% {chicken.Tiredness,4:F0}% {chicken.Energy,4:F0}% {posStr,20}");
            }
            
            report.AppendLine();
        }

        private static void GenerateTransitionHistorySection(StringBuilder report)
        {
            var transitions = ChickenMonitorManager.Instance.TransitionHistory.TakeLast(50).ToList();
            
            report.AppendLine("--- RECENT TRANSITION HISTORY (Last 50) ---");
            report.AppendLine($"{"Time",-12} {"Chicken ID",-20} {"From State",-25} {"To State",-25}");
            report.AppendLine(new string('-', 85));
            
            foreach (var transition in transitions)
            {
                report.AppendLine($"{transition.TimeOfDay,-12} {transition.ChickenID,-20} {transition.FromState,-25} {transition.ToState,-25}");
            }
            
            report.AppendLine();
        }

        private static string GenerateBar(float percentage)
        {
            int barLength = 20;
            int filledLength = Mathf.RoundToInt((percentage / 100f) * barLength);
            return "[" + new string('█', filledLength) + new string('░', barLength - filledLength) + "]";
        }

        private static string SaveReport(string content)
        {
            string directory = Path.Combine(Application.dataPath, "..", "ChickenMonitorReports");
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string fileName = $"ChickenReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(directory, fileName);
            
            File.WriteAllText(filePath, content, Encoding.UTF8);
            
            return filePath;
        }
    }
}
