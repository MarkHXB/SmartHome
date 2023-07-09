using Saturn.BL.FeatureUtils;

namespace Saturn.BL
{
    public static class CommandHandler 
    {
        public static async Task Parse(FeatureHandler featureHandler, params string[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception("You should pass at least one command to arguments!");
            }

            string command = args[0];
            string value = args.Length > 1 ? args[1] : string.Empty;

            await HandleCommand(featureHandler ,command, value);
        }
        public static async Task Parse(FeatureHandler featureHandler, string? command, string? value = null)
        {
            await HandleCommand(featureHandler, command, value);
        }
        private static async Task HandleCommand(FeatureHandler featureHandler, string? command, string? value)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(nameof(command));
            }

            Commands parsedCommand = (Commands)Enum.Parse(typeof(Commands), command, ignoreCase: true);

            switch (parsedCommand)
            {
                case Commands.RUN: await featureHandler.TryToRun(value); break;
                case Commands.RUNALL: await featureHandler.TryToRunAll(); break;
                case Commands.RUNWHEN: await featureHandler.ScheduleRun(value); break;
                case Commands.ADDFEATURE: featureHandler.AddFeature(value); break;
                case Commands.ENABLE: featureHandler.EnableFeature(value); break;
                case Commands.DISABLE: featureHandler.DisableFeature(value); break;
                default:
                    break;
            }
        }
    }
}
