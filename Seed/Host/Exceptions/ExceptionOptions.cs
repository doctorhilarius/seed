namespace Seed.Host.Exceptions {
	/// <summary>
	/// Options for exception handling middleware
	/// </summary>
	public class ExceptionOptions {

		/// <summary>
		/// Gets or sets a value indicating whether developer exceptions should be enabled or not.
		/// Developer exceptions will show detailed error pages
		/// </summary>
		public bool EnableDeveloperErrors { get; set; }

		/// <summary>
		/// Gets or sets the SMTP server to send error emails to.
		/// </summary>
		public string SmtpServer { get; set; }

		/// <summary>
		/// Gets or sets the email address to send exception emails. Set to null
		/// to not send error emails
		/// </summary>
		public string MailTo { get; set; }

		/// <summary>
		/// Gets or sets the email address to send error emails from.
		/// </summary>
		public string MailFrom { get; set; }

	}
}