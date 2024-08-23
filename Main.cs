using DnsClient;
using System.Text.RegularExpressions;

namespace EmailDnsChecking;

public static partial class EmailDnsChecker {
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

	[GeneratedRegex(@"^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})$")]
	private static partial Regex DomainRegex();
}

public sealed class DnsResult(string message, int code) {
	public string Message { get; } = message;
	public int Code { get; } = code;
}