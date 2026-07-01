using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShodanClient;
using ShodanClient.Application.Exceptions;
using ShodanClient.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Binds the "Shodan" section of appsettings.json (or the Shodan__ApiKey environment variable)
// and validates it at startup.
builder.Services.AddShodanClient(builder.Configuration);

using var host = builder.Build();
var shodan = host.Services.GetRequiredService<IShodanClient>();

// InternetDB requires no API key, so this call always works out of the box.
Console.WriteLine("--- InternetDB lookup (no API key required) ---");
var summary = await shodan.InternetDb.GetAsync("8.8.8.8");
Console.WriteLine($"{summary.Ip}: ports={string.Join(',', summary.Ports)} tags={string.Join(',', summary.Tags)}");

// Everything else needs a real API key from https://account.shodan.io.
Console.WriteLine();
Console.WriteLine("--- Account profile (requires a real API key in appsettings.json) ---");
try
{
	var profile = await shodan.Account.GetProfileAsync();
	Console.WriteLine($"Member={profile.Member} Credits={profile.Credits} DisplayName={profile.DisplayName}");
}
catch (ShodanAuthenticationException)
{
	Console.WriteLine("Set a real Shodan API key in appsettings.json (Shodan:ApiKey) to run this call.");
}
