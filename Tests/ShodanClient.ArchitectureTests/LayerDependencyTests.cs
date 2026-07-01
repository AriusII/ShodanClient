using NetArchTest.Rules;
using ShodanClient.Application.Configuration;
using ShodanClient.Domain.Search;
using TestResult = NetArchTest.Rules.TestResult;

namespace ShodanClient.ArchitectureTests;

/// <summary>
///     Enforces the Clean Architecture dependency rule at build/test time: dependencies only ever point
///     inward (Infrastructure → Application → Domain), never the other way.
/// </summary>
public sealed class LayerDependencyTests
{
	private const string ApplicationNamespace = "ShodanClient.Application";
	private const string InfrastructureNamespace = "ShodanClient.Infrastructure";

	[Fact]
	public void Domain_should_not_depend_on_application_or_infrastructure()
	{
		var result = Types.InAssembly(typeof(Host).Assembly)
			.ShouldNot()
			.HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
			.GetResult();

		Assert.True(result.IsSuccessful, Describe(result));
	}

	[Fact]
	public void Application_should_not_depend_on_infrastructure()
	{
		var result = Types.InAssembly(typeof(ShodanClientOptions).Assembly)
			.ShouldNot()
			.HaveDependencyOn(InfrastructureNamespace)
			.GetResult();

		Assert.True(result.IsSuccessful, Describe(result));
	}

	private static string Describe(TestResult result) =>
		result.IsSuccessful
			? "OK"
			: "Offending types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
