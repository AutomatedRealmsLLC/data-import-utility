using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DataImportUtility.Helpers.ModelValidation;

/// <summary>
/// Helper methods for validating models.
/// </summary>
/// <remarks>
/// The model validation methods in this class are designed to be used in conjunction with the <see cref="System.ComponentModel.DataAnnotations"/> namespace.
/// </remarks>
public static class ModelValidatorHelpers
{
    #region Context Cache
    private static readonly Dictionary<object, ValidationContext> ContextCache = [];

    private static ValidationContext GetOrCreateContext<TObjectType>(this TObjectType obj)
    {
        if (obj is null) { throw new ArgumentNullException(nameof(obj)); }

        if (!ContextCache.TryGetValue(obj, out var context))
        {
            context = new ValidationContext(obj);
            ContextCache[obj] = context;
        }
        // Clear any previous member names to ensure the correct property is validated
        context.MemberName = null;
        return context;
    }

    /// <summary>
    /// Clears the cache of <see cref="ValidationContext"/> objects.
    /// </summary>
    public static void ClearValidationContextCache()
    {
        ContextCache.Clear();
    }
    #endregion Context Cache

    /// <summary>
    /// Validates the specified object and returns a collection of validation results.
    /// </summary>
    /// <typeparam name="TObjectType">The type of object to validate.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <param name="errorResults">
    /// When this method returns false, contains a collection of validation results if the object is not valid; 
    /// otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the object is valid; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    public static bool TryValidateObject<TObjectType>(this TObjectType obj, [NotNullWhen(false)] out ImmutableArray<ValidationResult>? errorResults)
    {
        var context = obj.GetOrCreateContext();
        return obj.TryValidateObject(context, out errorResults);
    }

    



    /// <summary>
    /// Validates the specified object and returns a collection of validation results.
    /// </summary>
    /// <typeparam name="TObjectType">The type of object to validate.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <param name="context">The validation context to use.</param>
    /// <param name="errorResults">
    /// When this method returns false, contains a collection of validation results if the object is not valid;
    /// otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the object is valid; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    public static bool TryValidateObject<TObjectType>(this TObjectType obj, ValidationContext context, [NotNullWhen(false)] out ImmutableArray<ValidationResult>? errorResults)
    {
        if (obj is null) { throw new ArgumentNullException(nameof(obj)); }
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(obj, context, results, true);
        errorResults = isValid ? null : results.ToImmutableArray();
        return isValid;
    }

    /// <summary>
    /// Validates the specified property of the object and returns a collection of validation results.
    /// </summary>
    /// <typeparam name="TObjectType">The type of object to validate.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <param name="errorResults">
    /// When this method returns false, contains a collection of validation results if the property is not valid;
    /// otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the property is valid; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    public static bool TryValidateProperty<TObjectType>(this TObjectType obj, string propertyName, [NotNullWhen(false)] out ImmutableArray<ValidationResult>? errorResults)
    {
        var context = obj.GetOrCreateContext();
        return obj.TryValidateProperty(propertyName, context, out errorResults);
    }

    /// <summary>
    /// Validates the specified property of the object and returns a collection of validation results.
    /// </summary>
    /// <typeparam name="TObjectType">The type of object to validate.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <param name="context">The validation context to use.</param>
    /// <param name="errorResults">
    /// When this method returns false, contains a collection of validation results if the property is not valid;
    /// otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the property is valid; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    public static bool TryValidateProperty<TObjectType>(this TObjectType obj, string propertyName, ValidationContext context, [NotNullWhen(false)] out ImmutableArray<ValidationResult>? errorResults)
    {
        var results = new List<ValidationResult>();
        context.MemberName = propertyName;
        var isValid = Validator.TryValidateProperty(obj, context, results);
        errorResults = isValid ? null : results.ToImmutableArray();
        return isValid;
    }
}
