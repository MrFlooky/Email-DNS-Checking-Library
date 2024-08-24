using DnsClient;
using System.Text.RegularExpressions;

namespace EmailDnsChecking;

public static partial class EmailDnsChecker {
	/// <summary>
	/// Asynchronously checks the DNS records for the email domain to determine its validity.
	/// </summary>
	/// <param name="email">The email address to validate.</param>
	/// <param name="timeout">Optional. The DNS query timeout in seconds. Default is 5 seconds.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a <see cref="DnsResult"/> object
	/// which indicates whether the email domain is valid and provides a status message.
	/// </returns>
	/// <remarks>
	/// The method first validates the email format and domain. It then performs a DNS MX record query
	/// to check if the domain has valid mail exchange (MX) records. The result indicates if the domain is valid or if there was an error.
	/// </remarks>
	/// <exception cref="DnsResponseException">Thrown when the DNS query fails.</exception>
	/// <exception cref="Exception">Thrown when an unexpected error occurs.</exception>
	public async static Task<DnsResult> CheckEmailAsync(string email, int timeout = 5) {
		if (string.IsNullOrEmpty(email) || !email.Contains('@'))
			return new DnsResult("Invalid email address.", 1);
		string domain = email.Split('@')[1];
		if (!IsValidDomain(domain))
			return new DnsResult("Invalid email address.", 1);
		try {
			LookupClient client = CreateLookupClient(timeout);
			var result = await client.QueryAsync(domain, QueryType.MX);

			bool hasMxRecords = result.Answers.MxRecords().Any();
			return hasMxRecords
				? new DnsResult("Email domain is valid.", 0)
				: new DnsResult("Email domain does not exist.", 2);
		} catch (DnsResponseException ex) {
			return new DnsResult("DNS query failed: " + ex.Message, 3);
		} catch (Exception ex) {
			return new DnsResult("Unexpected error: " + ex.Message, 4);
		}
	}

	private static bool IsValidDomain(string domain) => DomainRegex().IsMatch(domain);

	private static LookupClient CreateLookupClient(int timeout) =>
		new(new LookupClientOptions {
			UseCache = true,
			UseTcpOnly = true,
			Timeout = TimeSpan.FromSeconds(timeout)
		});

	[GeneratedRegex(@"^(?:xn--[a-zA-Z0-9-]+[a-zA-Z0-9]|(?!.{64,})[A-Za-z0-9]+(?:(?!.*-{2,})[A-Za-z0-9-]+[A-Za-z0-9])?)(?:\.xn--[a-zA-Z0-9-]+[a-zA-Z0-9]|\.[A-Za-z]{2,18})*$")]
	private static partial Regex DomainRegex();
}

public sealed class DnsResult(string message, int code) {
	public string Message { get; } = message;
	public int Code { get; } = code;
}