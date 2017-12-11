using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Seed {
	public class Program {

		public static void Main(string[] args) {

			// Configure logging before anything so that we can ensure that
			// application configuration and startup errors are logged
			// unfortunately the logger requires configuration so we will take the 
			// hit of loading the config twice but at this time the overhead is minimal
			Log.Logger = CreateLogger();

			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) {
			return WebHost.CreateDefaultBuilder(args)
				.CaptureStartupErrors(true)
				.UseStartup<Startup>()
				.UseSerilog()
				.Build();
		}

		/// <summary>
		/// Creates the logger for the application
		/// </summary>
		private static ILogger CreateLogger() {

			var loggerConfig = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Async(c => c.File(
						"../Logs/appLog_.txt",
						outputTemplate: GetLoggingTemplate(),
						rollingInterval: RollingInterval.Day
					)
				);

#if DEBUG
			//we will only use the console logger when debugging
			loggerConfig.WriteTo.Console(outputTemplate: GetLoggingTemplate());
#endif
			return loggerConfig.CreateLogger();
		}

		/// <summary>
		/// Gets the logging template to use for log messages
		/// </summary>
		private static string GetLoggingTemplate() {
			return "[{Timestamp:HH:mm:ss} {Level:u3}] [{RequestId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
		}

	}
}