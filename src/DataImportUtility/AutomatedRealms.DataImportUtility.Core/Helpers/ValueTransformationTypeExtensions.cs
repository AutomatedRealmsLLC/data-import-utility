using System;
using AutomatedRealms.DataImportUtility.Abstractions.Enums;
using AutomatedRealms.DataImportUtility.Core.ValueTransformations;

namespace AutomatedRealms.DataImportUtility.Core.Helpers;

/// <summary>
/// Extension methods for the <see cref="ValueTransformationType"/> enum.
/// </summary>
public static class ValueTransformationTypeExtensions
{
    /// <summary>
    /// Gets the corresponding class <see cref="Type"/> for a given <see cref="ValueTransformationType"/>.
    /// </summary>
    /// <param name="transformationType">The transformation type.</param>
    /// <returns>The <see cref="Type"/> of the transformation implementation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the transformation type is not recognized.</exception>
    public static Type GetClassType(this ValueTransformationType transformationType)
    {
        return transformationType switch
        {
            ValueTransformationType.CalculateTransformation => typeof(CalculateTransformation),
            ValueTransformationType.CombineFieldsTransformation => typeof(CombineFieldsTransformation),
            ValueTransformationType.ConditionalTransformation => typeof(ConditionalTransformation),
            ValueTransformationType.InterpolateTransformation => typeof(InterpolateTransformation),
            ValueTransformationType.MapTransformation => typeof(MapTransformation),
            ValueTransformationType.RegexMatchTransformation => typeof(RegexMatchTransformation),
            ValueTransformationType.SubstringTransformation => typeof(SubstringTransformation),
            _ => throw new ArgumentOutOfRangeException(nameof(transformationType), $"Unknown transformation type: {transformationType}")
        };
    }
}
