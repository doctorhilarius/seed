using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Seed.Host.Exceptions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;

namespace Seed {
	public class Program {

		/// <summary>
		/// The Main entry point into the application
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static void Main(string[] args) {

			// Configure logging before anything so that we can ensure that
			// application configuration and startup errors are logged
			// unfortunately the logger requires configuration so we will take the 
			// hit of loading the config twice but at this time the overhead is minimal
			Log.Logger = CreateLogger(AddConfig(new ConfigurationBuilder()).Build());

			try {
				Log.Information("Building Web Host");
				IWebHost host = BuildWebHost(args);
				Log.Information("Starting Web Host");
				host.Run();

			} catch (Exception error) {
				Log.Fatal(error, "Web Host terminated unexpectedly");
			} finally {
				Log.CloseAndFlush();
			}
		}

		/// <summary>
		/// Builds the web host.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		public static IWebHost BuildWebHost(string[] args) {
			return WebHost.CreateDefaultBuilder(args)
				.CaptureStartupErrors(true)
				.ConfigureAppConfiguration((hostContext, config) => AddConfig(config, args))
				.UseStartup<Startup>().UseSerilog()
				.Build();
		}

		#region Private Methods

		/// <summary>
		/// Creates the logger for the application
		/// </summary>
		private static ILogger CreateLogger(IConfigurationRoot configuration) {

			var loggerConfig = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Async(c => c.File(
					"../Logs/appLog_.txt",
					outputTemplate: GetLoggingTemplate(),
					rollingInterval: RollingInterval.Day
				));

			ExceptionOptions errorSettings = configuration.GetSection("exceptions").Get<ExceptionOptions>();

			if (String.IsNullOrEmpty(errorSettings.MailTo) == false) {
				EmailConnectionInfo emailInfo = new EmailConnectionInfo {
					EmailSubject = $"GameFly Retail UI: Unhandled Exception occurred on machine {Environment.MachineName}",
					MailServer = errorSettings.SmtpServer,
					FromEmail = errorSettings.MailFrom,
					ToEmail = errorSettings.MailTo
				};
				loggerConfig.WriteTo.Email(emailInfo, GetLoggingTemplate(), LogEventLevel.Error);
			}

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

		/// <summary>
		/// Adds the configuration for the specified environment.
		/// </summary>
		/// <param name="configBuilder">The configuration builder.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>the config builder</returns>
		private static IConfigurationBuilder AddConfig(IConfigurationBuilder configBuilder, string[] args = null) {

			string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development";

			Log.Information("Loading configuration for environment: {environment}", environmentName);

			configBuilder.Sources.Clear();
			configBuilder.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("host/appsettings.json", false, true)
				.AddJsonFile($"host/environment/appsettings.{environmentName}.json", true, true);

			if (args != null) {
				configBuilder.AddCommandLine(args);
			}

			return configBuilder;
		}

		#endregion

	}
}