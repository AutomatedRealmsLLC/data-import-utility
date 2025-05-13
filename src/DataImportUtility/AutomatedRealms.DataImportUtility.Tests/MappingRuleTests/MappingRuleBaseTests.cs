using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Abstractions.Models;
using AutomatedRealms.DataImportUtility.Tests.TestHelpers;

namespace AutomatedRealms.DataImportUtility.Tests.MappingRuleTests;

public class MappingRuleBaseTests : MappingRuleBaseTestContext
{
    /// <summary>
    /// The main test method for the <see cref="MappingRuleBase.Apply(IEnumerable{TransformationResult})" />
    /// </summary>
    /// <param name="rule">Expects the concrete implementation of the <see cref="MappingRuleBase"/> to test.</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [Theory]
    [MemberData(nameof(MappingRuleTestData))]
    public async Task MappingRule_WithValidInput_ShouldNotProduceFailure(MappingRuleBase rule, TransformationResult input)
    {
        var result = await rule.Apply(input);

        Assert.NotNull(result); // Add null check for result
        Assert.False(result.WasFailure, $"Expected no failure for rule {rule.GetType().Name} with input {input}. Error Message: {result.ErrorMessage ?? "Error message was null."}");
    }
}
