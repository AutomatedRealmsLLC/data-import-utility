using AutomatedRealms.DataImportUtility.Abstractions;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AutomatedRealms.DataImportUtility.Tests.SampleData;

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
            return _valueTransformations ?? throw new InvalidOperationException("Value transformations failed to initialize. Please check the test setup.");
        }
    }

    // Updated Clone calls to be non-generic with a cast
    internal static CalculateTransformation CalculateOperation => (CalculateTransformation)ValueTransformations.OfType<CalculateTransformation>().First().Clone();
    internal static CombineFieldsTransformation CombineFieldsOperation => (CombineFieldsTransformation)ValueTransformations.OfType<CombineFieldsTransformation>().First().Clone();
    internal static InterpolateTransformation InterpolateOperation => (InterpolateTransformation)ValueTransformations.OfType<InterpolateTransformation>().First().Clone();
    internal static MapTransformation MapOperation => (MapTransformation)ValueTransformations.OfType<MapTransformation>().First().Clone();
    internal static RegexMatchTransformation RegexMatchOperation => (RegexMatchTransformation)ValueTransformations.OfType<RegexMatchTransformation>().First().Clone();
    internal static SubstringTransformation SubstringOperation => (SubstringTransformation)ValueTransformations.OfType<SubstringTransformation>().First().Clone();

    private static readonly object _valueTransformationPreparationLock = new();

    private static void PrepareValueTransformationInstances()
    {
        lock (_valueTransformationPreparationLock)
        {
            if (_valueTransformations is { Count: >0 }) { return; }

            _valueTransformations = new List<ValueTransformationBase>
            {
                new CalculateTransformation(),
                new CombineFieldsTransformation(),
                new InterpolateTransformation(),
                new MapTransformation(),
                new RegexMatchTransformation(),
                new SubstringTransformation()
            };

            foreach (var transformation in _valueTransformations)
            {
                switch (transformation)
                {
                    case CalculateTransformation calculateOperation:
                        calculateOperation.TransformationDetail = "{0} + 0.01"; // Correct property is TransformationDetail
                        break;
                    case CombineFieldsTransformation combineFieldsOperation:
                        // Default setup if any
                        combineFieldsOperation.TransformationDetail = "{0}-{1}"; // Use TransformationDetail
                        // Assuming CombineFieldsTransformation now uses a list of field names or similar simple config
                        // If it needs ConfiguredInputField, that would require Abstractions.Models
                        break;
                    case InterpolateTransformation interpolationOperation:
                        // Default setup if any
                        break;
                    case MapTransformation mapOperation:
                        mapOperation.Mappings.Add("OriginalValueA", "MappedValueA"); // Mappings is Dictionary<string, string>
                        break;
                    case RegexMatchTransformation regexMatchOperation:
                        regexMatchOperation.TransformationDetail = @"\d+"; // Use TransformationDetail
                        break;
                    case SubstringTransformation substringOperation:
                        substringOperation.StartIndex = 0;
                        substringOperation.MaxLength = 4; // TransformationDetail is set internally
                        break;
                }
            }
        }
    }
}
