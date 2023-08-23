using Saturn.BL;
using Saturn.Shared;

class Program
{
    static async Task Main(string[] args)
    {
        string tempRunMode = args.FirstOrDefault() ?? string.Empty;
        var tempList = args.ToList();
        tempList.Remove(tempRunMode);
        args = tempList.ToArray();
        RunMode runMode = RunMode.CLI;

        try
        {
            runMode = (RunMode)Enum.Parse(typeof(RunMode), tempRunMode, ignoreCase: true);
        }
        catch (Exception)
        {
            runMode = RunMode.CLI;
        }

        var vmBox = new VirtualBox(args, runMode);
        await vmBox.Run();
    }
}