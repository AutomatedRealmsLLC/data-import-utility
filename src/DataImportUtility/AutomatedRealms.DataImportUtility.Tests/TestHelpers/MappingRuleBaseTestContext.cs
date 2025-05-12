using DataImportUtility.Tests.SampleData;

namespace AutomatedRealms.DataImportUtility.Tests.TestHelpers;

public class MappingRuleBaseTestContext
{
    /// <summary>
    /// The data for tests for the <see cref="MappingRuleBase.Apply(IEnumerable{TransformationResult})" /> method.
    /// </summary>
    public static IEnumerable<object[]> MappingRuleTestData { get; } = ImportDataObjects.MappingRules.Select(x => new object[] { x.Clone(), ImportDataObjects.TransformResultForRuleInput });

}