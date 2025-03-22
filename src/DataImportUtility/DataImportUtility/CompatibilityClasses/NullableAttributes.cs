// From https://github.com/dotnet/roslyn/blob/features/compiler/src/Compilers/Core/Portable/InternalUtilities/NullableAttributes.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This was copied from https://github.com/dotnet/runtime/blob/39b9607807f29e48cae4652cd74735182b31182e/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs
// and updated to have the scope of the attributes be internal.
namespace System.Diagnostics.CodeAnalysis;

#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER

/// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
internal sealed class MyMaybeNullAttribute : Attribute { }

/// <summary>Specifies that an output will not be null even if the corresponding type allows it.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
internal sealed class MyNotNullAttribute : Attribute { }

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class MyMaybeNullWhenAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified return value condition.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter may be null.
    /// </param>
    public MyMaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }
}

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class MyNotNullWhenAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified return value condition.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    public MyNotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }
}

/// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
internal sealed class MyNotNullIfNotNullAttribute : Attribute
{
    /// <summary>Initializes the attribute with the associated parameter name.</summary>
    /// <param name="parameterName">
    /// The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
    /// </param>
    public MyNotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

    /// <summary>Gets the associated parameter name.</summary>
    public string ParameterName { get; }
}

/// <summary>Applied to a method that will never return under any circumstance.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class MyDoesNotReturnAttribute : Attribute { }

/// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class MyDoesNotReturnIfAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified parameter value.</summary>
    /// <param name="parameterValue">
    /// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
    /// the associated parameter matches this value.
    /// </param>
    public MyDoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

    /// <summary>Gets the condition parameter value.</summary>
    public bool ParameterValue { get; }
}

#endif

#if !NET5_0_OR_GREATER

/// <summary>Specifies that the method or property will ensure that the listed field and property members have not-null values.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
internal sealed class MyMemberNotNullAttribute : Attribute
{
    /// <summary>Initializes the attribute with a field or property member.</summary>
    /// <param name="member">
    /// The field or property member that is promised to be not-null.
    /// </param>
    public MyMemberNotNullAttribute(string member) => Members = new[] { member };

    /// <summary>Initializes the attribute with the list of field and property members.</summary>
    /// <param name="members">
    /// The list of field and property members that are promised to be not-null.
    /// </param>
    public MyMemberNotNullAttribute(params string[] members) => Members = members;

    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }
}

/// <summary>Specifies that the method or property will ensure that the listed field and property members have not-null values when returning with the specified return value condition.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
internal sealed class MyMemberNotNullWhenAttribute : Attribute
{
    /// <summary>Initializes the attribute with the specified return value condition and a field or property member.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    /// <param name="member">
    /// The field or property member that is promised to be not-null.
    /// </param>
    public MyMemberNotNullWhenAttribute(bool returnValue, string member)
    {
        ReturnValue = returnValue;
        Members = new[] { member };
    }

    /// <summary>Initializes the attribute with the specified return value condition and list of field and property members.</summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    /// <param name="members">
    /// The list of field and property members that are promised to be not-null.
    /// </param>
    public MyMemberNotNullWhenAttribute(bool returnValue, params string[] members)
    {
        ReturnValue = returnValue;
        Members = members;
    }

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }

    /// <summary>Gets field or property member names.</summary>
    public string[] Members { get; }
}

#endif
