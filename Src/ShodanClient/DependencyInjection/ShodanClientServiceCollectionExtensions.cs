using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShodanClient.Application.Configuration;
using ShodanClient.Configuration;
using ShodanClient.Infrastructure.DependencyInjection;

namespace ShodanClient.DependencyInjection;

/// <summary>
///     Registration entry points for ShodanClient. Call one of the <c>AddShodanClient</c> overloads on
///     your <see cref="IServiceCollection" />, then resolve <see cref="IShodanClient" />.
/// </summary>
public static class ShodanClientServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		/// <summary>
		///     Registers ShodanClient, binding <see cref="ShodanClientOptions" /> from the given configuration
		///     section (default <c>Shodan</c>). Options are validated at application start.
		/// </summary>
		public IServiceCollection AddShodanClient(IConfiguration configuration,
			string sectionName = ShodanClientOptions.SectionName)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(configuration);

			services.AddOptions<ShodanClientOptions>()
				.Bind(configuration.GetSection(sectionName))
				.ValidateOnStart();

			return services.AddShodanCore();
		}

		/// <summary>
		///     Registers ShodanClient, configuring <see cref="ShodanClientOptions" /> in code. Options are
		///     validated at application start.
		/// </summary>
		public IServiceCollection AddShodanClient(Action<ShodanClientOptions> configure)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(configure);

			services.AddOptions<ShodanClientOptions>()
				.Configure(configure)
				.ValidateOnStart();

			return services.AddShodanCore();
		}

		private IServiceCollection AddShodanCore()
		{
			services.AddSingleton<IValidateOptions<ShodanClientOptions>, ValidateShodanClientOptions>();
			services.AddShodanInfrastructure();
			services.AddTransient<IShodanClient, ShodanApiClient>();
			return services;
		}
	}
}
