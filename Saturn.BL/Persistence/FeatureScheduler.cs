using Newtonsoft.Json;
using Saturn.Shared;
using Saturn.Shared.Models;
using System.Text;

namespace Saturn.BL.Persistence
{
    public static class FeatureScheduler
    {
        #region Public methods

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

                    if (ShouldRun(now, schedule.RunOn))
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

        public static void RefreshScheduling(List<Feature?>? features)
        {
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

                int countOfRuns = feature.AdvancedConfig.Scheduling.RunPer24Hour;
                int hoursReamining = HoursRemainingToday();
                int gapNumber = hoursReamining / countOfRuns;
                List<RunWhenStruct> runs = new List<RunWhenStruct>();
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

                    int currentHour = currentGapHour + gapNumber;
                    run.RunOn = now.AddHours(currentHour);
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

        #endregion
    }
}
