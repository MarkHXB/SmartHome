﻿using Saturn.BL;

class Program
{
    static async Task Main(string[] args)
    {
        string runMode = args.FirstOrDefault() ?? string.Empty;
        var tempList = args.ToList();
        tempList.Remove(runMode);
        args = tempList.ToArray();

        await new VirtualBox(args, RunMode.MENU).Run();
    }
}