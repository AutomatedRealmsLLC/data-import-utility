using System.Globalization;

namespace AutomatedRealms.DataImportUtility.Abstractions.Helpers;

internal static class NumberHelpers
{
    public static bool IsNumericType(this object? value)
        => value is not null
            && (
                value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal);

    public static bool CanConvertToDateTime(this object? value, out DateTime result)
    {
        result = default;
        if (value is null) return false;
        if (value is DateTime dt)
        {
            result = dt;
            return true;
        }
        if (value is DateTimeOffset dto)
        {
            result = dto.UtcDateTime; // Ensure comparison in UTC
            return true;
        }
        var stringValue = value.ToString();
        return !string.IsNullOrEmpty(stringValue) && DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out result);
    }

}
