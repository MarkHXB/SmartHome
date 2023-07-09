using Serilog.Events;
using Serilog.Formatting;

namespace Saturn.BL.Logging
{
    public class SerilogTextFormatter : ITextFormatter
    {
        void ITextFormatter.Format(LogEvent logEvent, TextWriter output)
        {
            output.Write("{0} [{1}] {3} {4}", logEvent.Timestamp,logEvent.Level, logEvent.MessageTemplate, output.NewLine);

            if (logEvent.Exception != null)
            {
                output.WriteLine("----- Exception -----");
                output.Write("\tException - {0}", logEvent.Exception);
                output.Write(output.NewLine);
            }
        }

       public string GetOutputTemplate()
        {
            return "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";
        }
    }
}
