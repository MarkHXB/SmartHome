using Newtonsoft.Json;

namespace Saturn.Shared.Models
{
    public class RunWhenStruct
    {
        public DateTime RunOn { get; set; }
        public bool IsCompleted { get; set; }
    }
    public struct ScheduleStruct
    {
        public string FeatureName { get; set; }
        public List<RunWhenStruct> RunWhenList { get; set; }
        public DateTime RegisteredDateTime { get; set; }
    }
}
