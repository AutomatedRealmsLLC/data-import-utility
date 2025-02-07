using DataImportUtility.Abstractions;
using DataImportUtility.Models;
using DataImportUtility.Tests.TestHelpers;

namespace DataImportUtility.Tests.MappingRuleTests;

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

        Assert.False(result.WasFailure);
    }
}
