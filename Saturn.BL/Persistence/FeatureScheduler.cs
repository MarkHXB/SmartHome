using Newtonsoft.Json;
using Saturn.Shared;
using Saturn.Shared.Models;
using System.Text;

namespace Saturn.BL.Persistence
{
    public static class FeatureScheduler
    {
        private static DateTime startTime = DateTime.Now;
        private static bool firstStart = true;

        #region Public methods

        /// <summary>
        /// Load all the feature's names should run, this method can run every minute
        /// </summary>
        /// <returns>List of all features name's which should run</returns>
        public static List<string> LoadShouldRunFeatures()
        {
            var schedules = LoadAll();
            List<string> features = new List<string>();
            List<ScheduleStruct> schedulesShouldUpdate = new List<ScheduleStruct>();
            DateTime now = DateTime.Now;

            foreach (var feature in schedules)
            {
                if (!feature.RunWhenList.Any())
                {
                    continue;
                }

                foreach (var schedule in feature.RunWhenList)
                {
                    if (schedule.IsCompleted)
                    {
                        continue;
                    }

                    if (ShouldRun(schedule.RunOn, now))
                    {
                        schedule.IsCompleted = true;
                        features.Add(feature.FeatureName);
                        schedulesShouldUpdate.Add(feature);
                        break;
                    }
                }
            }

            UpdateSchedules(schedulesShouldUpdate);

            return features;
        }

        /// <summary>
        /// Daily refresh for daemon
        /// </summary>
        /// <param name="features">Features list</param>
        public static void RefreshScheduling(IEnumerable<Feature?> features)
        {
            if (!ShouldRefresh(features))
            {
                return;
            }
            if (features is null)
            {
                return;
            }
            if (!features.Any())
            {
                return;
            }
            
            DateTime now = DateTime.Now;
            List<ScheduleStruct> schedules = new List<ScheduleStruct>();

            foreach (var feature in features)
            {
                if (feature?.AdvancedConfig is null)
                {
                    continue;
                }

                List<RunWhenStruct> runs = new List<RunWhenStruct>();

                int countOfRuns = feature.AdvancedConfig.Scheduling.RunPer24Hour;
                int hoursReamining = HoursRemainingToday();

                // if 23:50+ time
                if(hoursReamining < countOfRuns)
                {
                    if (now.AddMinutes(5).Day == now.Day + 1)
                    {
                        return;
                    }
                    RunWhenStruct run = new RunWhenStruct();
                    run.RunOn = now;
                    runs.Add(run);
                    return;
                }

                // if anytime
                int gapNumber = hoursReamining / countOfRuns;
                int currentGapHour = now.Hour;
                for (int i = 0; i < countOfRuns && i < AppInfo.MaxCountOfRunAFeaturePerDay; i++)
                {
                    RunWhenStruct run = new RunWhenStruct();
                    if (i == 0)
                    {
                        run.RunOn = now.AddMinutes(1);
                        runs.Add(run);
                        continue;
                    }

                    run.RunOn = now.AddHours(gapNumber * i);
                    runs.Add(run);
                }

                schedules.Add(new ScheduleStruct
                {
                    FeatureName = feature.Name,
                    RegisteredDateTime = now,
                    RunWhenList = runs,
                });
            }

            SaveAll(schedules);
        }

        #endregion

        #region Private methods

        private static void SaveAll(List<ScheduleStruct> schedules)
        {
            if (!schedules.Any())
            {
                return;
            }

            var json = JsonConvert.SerializeObject(schedules);

            if (File.Exists(AppInfo.ScheduledFeaturesFilePath))
            {
                File.Delete(AppInfo.ScheduledFeaturesFilePath);
            }

            File.WriteAllText(AppInfo.ScheduledFeaturesFilePath, json, Encoding.UTF8);
        }

        private static List<ScheduleStruct> LoadAll(bool orderedByAscDateTime = true)
        {
            if (!File.Exists(AppInfo.ScheduledFeaturesFilePath))
            {
                File.Create(AppInfo.ScheduledFeaturesFilePath).Close();
                return new List<ScheduleStruct>();
            }

            var json = File.ReadAllText(AppInfo.ScheduledFeaturesFilePath, Encoding.UTF8);
            var schedules = JsonConvert.DeserializeObject<List<ScheduleStruct>>(json) ?? new List<ScheduleStruct>();

            if (!schedules.Any())
            {
                return new List<ScheduleStruct>();
            }

            schedules = schedules.Where(sc => sc.RunWhenList.Any(rw => !rw.IsCompleted)).ToList();

            if (orderedByAscDateTime)
            {
                schedules = schedules.OrderBy(sc => sc.RegisteredDateTime).ToList();
            }
            else
            {
                schedules = schedules.OrderByDescending(sc => sc.RegisteredDateTime).ToList();
            }

            return schedules;
        }

        private static int HoursRemainingToday()
        {
            DateTime now = DateTime.Now;
            DateTime endOfDay = now.Date.AddDays(1).AddTicks(-1);
            TimeSpan remainingTime = endOfDay - now;
            return (int)remainingTime.TotalHours;
        }

        private static void UpdateSchedules(List<ScheduleStruct> schedulesShouldUpdate)
        {
            if (!schedulesShouldUpdate.Any())
            {
                return;

            }
            var schedules = LoadAll().ToList();

            for (int i = 0; i < schedules.Count; i++)
            {
                for (int j = 0; j < schedulesShouldUpdate.Count; j++)
                {
                    if (schedules[i].FeatureName.Equals(schedulesShouldUpdate[j].FeatureName, StringComparison.OrdinalIgnoreCase))
                    {
                        schedules[i] = schedulesShouldUpdate[j];
                    }
                }
            }

            SaveAll(schedules);
        }

        private static bool ShouldRun(DateTime x, DateTime y)
        {
            return x < y;
        }

        private static bool ShouldRefresh(IEnumerable<Feature?> features)
        {
            if (firstStart)
            {
                firstStart = false;
                if (features.Any(f => !f?.AdvancedConfig?.IsNull() ?? false))
                { 
                    return true;
                }
            }

            if(startTime.AddHours(23) < DateTime.Now)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
