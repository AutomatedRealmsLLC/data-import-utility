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

    //public static bool IsNumericType(this object value)
    //    => value is sbyte
    //        || value is byte
    //        || value is short
    //        || value is ushort
    //        || value is int
    //        || value is uint
    //        || value is long
    //        || value is ulong
    //        || value is float
    //        || value is double
    //        || value is decimal;
}
