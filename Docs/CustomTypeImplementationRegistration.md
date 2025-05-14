# Using Type Registration for Custom Implementations in AutomatedRealms.DataImportUtility

This guide explains how to register and use custom implementations of `MappingRuleBase`, `ValueTransformationBase`, and `ComparisonOperationBase` within the AutomatedRealms.DataImportUtility framework.

## Overview

The type registration system allows you to create custom implementations of core components and register them with unique identifiers. These registered types can later be resolved by their identifiers.

## Creating Custom Implementations

### 1. Create a custom MappingRule

```csharp
public class CustomMappingRule : MappingRuleBase
{
    public CustomMappingRule() : base("Custom.MappingRule") { }
    
    public override string DisplayName => "Custom Mapping Rule";
    public override string Description => "A custom mapping rule implementation";
    public override string ShortName => "Custom";
    
    // Implement required abstract methods
    public override Task<IEnumerable<TransformationResult?>> Apply() { /* ... */ }
    public override Task<IEnumerable<TransformationResult?>> Apply(DataTable data) { /* ... */ }
    public override Task<TransformationResult?> Apply(DataRow dataRow) { /* ... */ }
    public override Task<TransformationResult?> Apply(ITransformationContext context) { /* ... */ }
    public override MappingRuleBase Clone() { /* ... */ }
    public override TransformationResult GetValue(List<ImportedRecordFieldDescriptor> sourceRecord, ImportedRecordFieldDescriptor targetField) { /* ... */ }
}
```

### 2. Create a custom ValueTransformation

```csharp
public class CustomValueTransformation : ValueTransformationBase
{
    public CustomValueTransformation() : base("Custom.ValueTransformation") { }
    
    public override string DisplayName => "Custom Value Transformation";
    public override string Description => "A custom value transformation implementation";
    public override bool IsEmpty => false;
    public override Type OutputType => typeof(string);
    
    public override Task<TransformationResult> ApplyTransformationAsync(TransformationResult previousResult) { /* ... */ }
    public override Task<TransformationResult> Transform(object? value, Type targetType) { /* ... */ }
    public override ValueTransformationBase Clone() { /* ... */ }
}
```

### 3. Create a custom ComparisonOperation

```csharp
public class CustomComparisonOperation : ComparisonOperationBase
{
    public CustomComparisonOperation() : base("Custom.ComparisonOperation") { }
    
    public override string DisplayName => "Custom Comparison Operation";
    public override string Description => "A custom comparison operation implementation";
    
    public override Task<bool> Evaluate(TransformationResult result) { /* ... */ }
}
```

## Registering Custom Implementations

Register your custom implementations during application startup:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register the ITypeRegistryService first
        services.AddSingleton<ITypeRegistryService, TypeRegistryService>();
        
        // Register your custom implementations
        services.AddMappingRule<CustomMappingRule>("Custom.MappingRule");
        services.AddValueTransformation<CustomValueTransformation>("Custom.ValueTransformation");
        services.AddComparisonOperation<CustomComparisonOperation>("Custom.ComparisonOperation");
    }
}
```

## Using Registered Types

After registration, you can use the `ITypeRegistryService` to resolve types:

```csharp
public class MyService
{
    private readonly ITypeRegistryService _typeRegistry;
    
    public MyService(ITypeRegistryService typeRegistry)
    {
        _typeRegistry = typeRegistry;
    }
    
    public void ProcessWithCustomTypes()
    {
        // Resolve type
        Type? mappingRuleType = _typeRegistry.ResolveType("Custom.MappingRule");
        
        // Or try to resolve
        if (_typeRegistry.TryResolveType("Custom.ValueTransformation", out Type? transformationType))
        {
            // Use transformationType
        }
        
        // For comparison operations, you can directly get an instance
        ComparisonOperationBase? comparisonOperation = 
            _typeRegistry.ResolveComparisonOperation("Custom.ComparisonOperation");
        
        if (comparisonOperation != null)
        {
            // Use the operation
        }
    }
}
```

## Best Practices

1. **TypeId Naming**:
   - Use namespaced formats like `Category.Operation` or `Domain.OperationType`
   - Make identifiers descriptive and unique
   - Consider using constants for TypeIds to avoid string typos

2. **Registration**:
   - Register types early in application startup
   - Avoid registering the same TypeId multiple times
   - Handle potential registration exceptions

3. **Resolution**:
   - Use TryResolveType when the type might not be registered
   - Check for null when using ResolveComparisonOperation