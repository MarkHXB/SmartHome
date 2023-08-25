using System.Numerics;

namespace Saturn.Shared
{
    public struct Scheduling
    {
        public int RunPer24Hour { get; set; }
    }
    public class AdvancedConfig
    {
        public Scheduling Scheduling { get; set; }

        public bool IsNull() => Scheduling.RunPer24Hour == 0;
    }
}
