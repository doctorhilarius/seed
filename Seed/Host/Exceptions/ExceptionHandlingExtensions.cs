#region Using Statements

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

#endregion

namespace Seed.Host.Exceptions {
	/// <summary>
	/// Configures Exception handling middleware for the application
	/// </summary>
	public static class ExceptionHandlingExtensions {
		/// <summary>
		/// Uses the configured exception handling.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <param name="options">options for exception handling</param>
		/// <returns>the current application builder</returns>
		public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, ExceptionOptions options) {

			if (options == null) {
				throw new ArgumentNullException(nameof(options));
			}

			if (options.EnableDeveloperErrors) {
				//informative exception page for devs only
				app.UseDeveloperExceptionPage();

				//we can throw an error in dev by going to /throw
				app.Use(async (context, next) => {
					if (context.Request.Path.HasValue && context.Request.Path.StartsWithSegments(new PathString("/throw"))) {
						throw new Exception("Exception Triggered");
					}
					await next();
				});

			} else {

				//this will display the server error page for any 500 error, all other status codes 
				//will be displayed below with the default status pages
				//errors will be logged by the logging framework
				app.UseExceptionHandler("/static/serverError.html");

				//this will display a very simply status code page for any status
				//code that is not a 500 error code - this should be fine since we 
				//arent hardly running any code through the server, if we do any server processing
				//we will probably want to use custom error pages through UseStatusCodePagesWithReExecute
				app.UseStatusCodePages();
			}

			return app;
		}
	}
}
