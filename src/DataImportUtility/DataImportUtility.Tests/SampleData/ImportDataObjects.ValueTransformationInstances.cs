using System.Diagnostics.CodeAnalysis;

using DataImportUtility.Abstractions;
using DataImportUtility.ValueTransformations;

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
    internal static CalculateTransformation CalculateOperation => ValueTransformations.OfType<CalculateTransformation>().First().Clone<CalculateTransformation>();
    /// <summary>
    /// The combine fields operation instance to use for testing.
    /// </summary>
    internal static CombineFieldsTransformation CombineFieldsOperation => ValueTransformations.OfType<CombineFieldsTransformation>().First().Clone<CombineFieldsTransformation>();
    /// <summary>
    /// The interpolate operation instance to use for testing.
    /// </summary>
    internal static InterpolateTransformation InterpolateOperation => ValueTransformations.OfType<InterpolateTransformation>().First().Clone<InterpolateTransformation>();
    /// <summary>
    /// The map operation instance to use for testing.
    /// </summary>
    internal static MapTransformation MapOperation => ValueTransformations.OfType<MapTransformation>().First().Clone<MapTransformation>();
    /// <summary>
    /// The regex match operation instance to use for testing.
    /// </summary>
    internal static RegexMatchTransformation RegexMatchOperation => ValueTransformations.OfType<RegexMatchTransformation>().First().Clone<RegexMatchTransformation>();
    /// <summary>
    /// The substring operation instance to use for testing.
    /// </summary>
    internal static SubstringTransformation SubstringOperation => ValueTransformations.OfType<SubstringTransformation>().First().Clone<SubstringTransformation>();

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
                    case CalculateTransformation calculateOperation:
                        calculateOperation.TransformationDetail = "${0} + .01";
                        break;
                    case CombineFieldsTransformation combineFieldsOperation:

                        break;
                    case InterpolateTransformation interpolationOperation:

                        break;
                    case MapTransformation mapOperation:

                        break;
                    case RegexMatchTransformation regexMatchOperation:

                        break;
                    case SubstringTransformation substringOperation:

                        break;
                }
            }
        }
    }
}
