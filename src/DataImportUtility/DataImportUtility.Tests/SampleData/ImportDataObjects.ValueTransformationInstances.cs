using System.Diagnostics.CodeAnalysis;

using DataImportUtility.Abstractions;
using DataImportUtility.TransformOperations;

namespace DataImportUtility.Tests.SampleData;

internal static partial class ImportDataObjects
{
    private static List<ValueTransformationBase>? _valueTransformations;
    /// <summary>
    /// The list of value transformations to use for testing.
    /// </summary>
    internal static List<ValueTransformationBase> ValueTransformations
    {
        get
        {
            PrepareValueTransformationInstances();
            return _valueTransformations;
        }
    }

    /// <summary>
    /// The calculate operation instance to use for testing.
    /// </summary>
    internal static CalculateOperation CalculateOperation => ValueTransformations.OfType<CalculateOperation>().First().Clone<CalculateOperation>();
    /// <summary>
    /// The combine fields operation instance to use for testing.
    /// </summary>
    internal static CombineFieldsOperation CombineFieldsOperation => ValueTransformations.OfType<CombineFieldsOperation>().First().Clone<CombineFieldsOperation>();
    /// <summary>
    /// The interpolate operation instance to use for testing.
    /// </summary>
    internal static InterpolateOperation InterpolateOperation => ValueTransformations.OfType<InterpolateOperation>().First().Clone<InterpolateOperation>();
    /// <summary>
    /// The map operation instance to use for testing.
    /// </summary>
    internal static MapOperation MapOperation => ValueTransformations.OfType<MapOperation>().First().Clone<MapOperation>();
    /// <summary>
    /// The regex match operation instance to use for testing.
    /// </summary>
    internal static RegexMatchOperation RegexMatchOperation => ValueTransformations.OfType<RegexMatchOperation>().First().Clone<RegexMatchOperation>();
    /// <summary>
    /// The substring operation instance to use for testing.
    /// </summary>
    internal static SubstringOperation SubstringOperation => ValueTransformations.OfType<SubstringOperation>().First().Clone<SubstringOperation>();

    private static readonly object _valueTransformationPreparationLock = new();

    [MemberNotNull(nameof(_valueTransformations))]
    private static void PrepareValueTransformationInstances()
    {
        lock (_valueTransformationPreparationLock)
        {
            if (_valueTransformations is { Count: >0 }) { return; }

            _valueTransformations = Enum.GetValues<ValueTransformationType>().Select(x => x.CreateNewInstance()!).ToList();

            foreach (var ruleType in Enum.GetValues<ValueTransformationType>())
            {
                var newRule = ruleType.CreateNewInstance();
                // Setup the default parameters for each operation
                switch (newRule)
                {
                    case CalculateOperation calculateOperation:
                        calculateOperation.OperationDetail = "${0} + .01";
                        break;
                    case CombineFieldsOperation combineFieldsOperation:

                        break;
                    case InterpolateOperation interpolationOperation:

                        break;
                    case MapOperation mapOperation:

                        break;
                    case RegexMatchOperation regexMatchOperation:

                        break;
                    case SubstringOperation substringOperation:

                        break;
                }
            }
        }
    }
}
