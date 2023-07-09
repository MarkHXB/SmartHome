using Microsoft.Extensions.Configuration;
using Saturn.API.Services;
using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Saturn.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.     

            var logger = new LoggerConfiguration()
             .WriteTo.File(AppInfo.LogFilePath_CLI, outputTemplate: new SerilogTextFormatter().GetOutputTemplate())
             .Enrich.FromLogContext()
            .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Logging.AddFile(
                pathFormat: AppInfo.LogFilePath_API,
                minimumLevel: LogLevel.Information,
                outputTemplate: new SerilogTextFormatter().GetOutputTemplate());

            builder.Logging.AddSerilog();

            // Configure api.
            ConfigureApi(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        private static void ConfigureApi(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
             });
        }

        private static Logger CreateLogger()
        {
            var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.File(formatter: new SerilogTextFormatter(), AppInfo.LogFilePath_CLI);

            return loggerConfiguration.CreateLogger();
        }
    }
}