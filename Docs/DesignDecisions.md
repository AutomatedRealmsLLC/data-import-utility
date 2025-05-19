# Design Decisions for Component Library Refactoring

This document outlines the detailed design decisions for refactoring the `AutomatedRealms.DataImportUtility.Components` library away from enum discriminators toward a more extensible interface-based approach.

## 1. Core Interface Design

### 1.1 File Type Handling

Instead of a simple `DataFileType` enum, we'll implement a "smart enum" pattern with extensibility:

```csharp
public abstract class FileType
{
    // Built-in types as static instances
    public static readonly FileType Excel = new ExcelFileType();
    public static readonly FileType Csv = new CsvFileType();
    
    // Properties that must be implemented
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract string[] SupportedExtensions { get; }
    public abstract string[] MimeTypes { get; }
    
    // Helper methods
    public virtual bool CanHandleFile(string fileName, string? mimeType = null)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
            
        return SupportedExtensions.Any(ext => 
            fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
    
    // Registry functionality
    private static readonly List<FileType> _knownTypes = [Excel, Csv];
    
    public static IReadOnlyList<FileType> KnownTypes => _knownTypes.AsReadOnly();
    
    public static void Register(FileType fileType)
    {
        if (fileType == null)
            throw new ArgumentNullException(nameof(fileType));
            
        if (!_knownTypes.Contains(fileType))
            _knownTypes.Add(fileType);
    }
    
    public static FileType? GetByName(string name) => 
        KnownTypes.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
```

### 1.2 Import Process Flow

Replace `FileImportType` enum with a workflow pattern that supports extensibility:

```csharp
public abstract class ImportWorkflow
{
    // Built-in workflow types
    public static readonly ImportWorkflow Standard = new StandardImportWorkflow();
    public static readonly ImportWorkflow Advanced = new AdvancedImportWorkflow();
    
    // Properties
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract int StepCount { get; }
    public abstract Type ComponentType { get; } // The component to render for this workflow
    
    // Methods
    public abstract bool SupportsFileType(FileType fileType);
    
    // Registry functionality
    private static readonly List<ImportWorkflow> _knownWorkflows = [Standard, Advanced];
    
    public static IReadOnlyList<ImportWorkflow> KnownWorkflows => _knownWorkflows.AsReadOnly();
    
    public static void Register(ImportWorkflow workflow)
    {
        if (workflow == null)
            throw new ArgumentNullException(nameof(workflow));
            
        if (!_knownWorkflows.Contains(workflow))
            _knownWorkflows.Add(workflow);
    }
    
    public static ImportWorkflow? GetByName(string name) =>
        KnownWorkflows.FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
```

### 1.3 Mapping Strategy

Similar smart enum pattern for mapping strategies:

```csharp
public abstract class MappingStrategy
{
    // Built-in strategies
    public static readonly MappingStrategy HeaderBased = new HeaderBasedMappingStrategy();
    public static readonly MappingStrategy PositionBased = new PositionBasedMappingStrategy();
    
    // Properties
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract Type ComponentType { get; }
    
    // Methods
    public abstract bool SupportsFileType(FileType fileType);
    
    // Registry
    private static readonly List<MappingStrategy> _knownStrategies = [HeaderBased, PositionBased];
    
    public static IReadOnlyList<MappingStrategy> KnownStrategies => _knownStrategies.AsReadOnly();
    
    public static void Register(MappingStrategy strategy)
    {
        if (strategy == null)
            throw new ArgumentNullException(nameof(strategy));
            
        if (!_knownStrategies.Contains(strategy))
            _knownStrategies.Add(strategy);
    }
    
    public static MappingStrategy? GetByName(string name) =>
        KnownStrategies.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
```

### 1.4 Validation Framework

For validation, we'll use a hybrid approach with enums where appropriate and extensibility where needed:

```csharp
// Keep enum for simple cases where extensibility isn't needed
public enum ValidationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

// Use a constant class for JavaScript interop values
public static class ValidationSeverityNames
{
    public const string Info = "info";
    public const string Warning = "warning";
    public const string Error = "error";
    public const string Critical = "critical";
    
    public static string GetName(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Info => Info,
        ValidationSeverity.Warning => Warning,
        ValidationSeverity.Error => Error,
        ValidationSeverity.Critical => Critical,
        _ => Error // Default
    };
}

// For validation rules that need to be extensible
public abstract class ValidationRule
{
    // Properties
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract ValidationSeverity DefaultSeverity { get; }
    
    // Methods
    public abstract bool IsApplicableToProperty(Type modelType, string propertyName);
    public abstract bool Validate(object? value, out string? errorMessage);
    
    // Registry
    private static readonly List<ValidationRule> _knownRules = [];
    
    public static IReadOnlyList<ValidationRule> KnownRules => _knownRules.AsReadOnly();
    
    public static void Register(ValidationRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
            
        if (!_knownRules.Contains(rule))
            _knownRules.Add(rule);
    }
}
```

## 2. Factory Pattern Implementation

### 2.1 Component Factory

```csharp
public interface IComponentFactory
{
    RenderFragment CreateComponent(Type componentType, Dictionary<string, object> parameters);
    RenderFragment<TItem> CreateComponent<TItem>(Type componentType, Dictionary<string, object> parameters);
}
```

### 2.2 Dynamic Component Registration

```csharp
public interface IComponentResolver 
{ 
    Type? ResolveComponentType(string componentKey); 
    void RegisterComponent(string componentKey, Type componentType); 
}
```

## 3. JavaScript Interop Architecture

For JavaScript interop, we'll use static classes with constants for string values:

```csharp
public static class ImportStatus
{
    public const string Idle = "idle";
    public const string Uploading = "uploading";
    public const string Processing = "processing";
    public const string Mapping = "mapping";
    public const string Validating = "validating";
    public const string Completed = "completed";
    public const string Error = "error";
}

public static class FileValidation
{
    public const string Success = "success";
    public const string InvalidFormat = "invalid-format";
    public const string TooLarge = "too-large";
    public const string Empty = "empty";
}
```

## 4. Class Relationships Diagram

```txt
                               ┌────────────┐
                               │            │
                        ┌─────►│  FileType  │
                        │      │            │
                        │      └────────────┘
                        │             ▲
                   uses │             │extends
┌──────────────┐        │      ┌─────────────┐
│              │        │      │             │
│ FileUploader ├────────┘      │ExcelFileType│
│  Component   │               │             │
│              │               └─────────────┘
└──────────────┘                      ▲
       ▲                              │extends
       │implements           ┌────────────┐
┌──────────────┐             │            │
│              │             │CsvFileType │
│ ImportWizard ├─────┐       │            │
│  Component   │     │       └────────────┘
│              │     │
└──────────────┘     │       ┌───────────────┐
                     │       │               │
                     └──────►│ImportWorkflow │
                             │               │
                             └───────────────┘
                                     ▲
                                     │extends
                             ┌──────────────────┐
                             │                  │
                             │StandardImportWork│
                             │      flow        │
                             └──────────────────┘
```

## 5. Generic Type Parameters

Use the new class hierarchy in component parameters:

```csharp
// Before:
public class FileUploader
{
    [Parameter] public DataFileType[] AllowedFileTypes { get; set; } = [DataFileType.Excel, DataFileType.Csv];
    // ...
}

// After:
public class FileUploader
{
    [Parameter] public FileType[] AllowedFileTypes { get; set; } = [FileType.Excel, FileType.Csv];
    // ...
}
```

## 6. Extension Points for Consumers

### 6.1 Registration Extensions

```csharp
public static class DataImportComponentsServiceCollectionExtensions
{
    public static IServiceCollection AddDataImportComponents(this IServiceCollection services)
    { 
        // Register core services
        services.AddSingleton<IComponentFactory, ComponentFactory>(); 
        services.AddSingleton<IComponentResolver, ComponentResolver>();
        
        // Register built-in types
        FileType.Register(new CustomExcelType());
        ImportWorkflow.Register(new CustomWorkflow());
        
        return services;
    }
}
```

### 6.2 Runtime Component Registration

```csharp
// Allow registration of components at runtime 
public interface IComponentRegistration 
{ 
    void RegisterComponent<TComponent>(string componentKey)
        where TComponent : IComponent;
    void RegisterValidator<TValidator>(string validatorKey)
        where TValidator : IComponent;
    void RegisterTransformer<TTransformer>(string transformerKey)
        where TTransformer : IComponent;
}
```

## 7. Backward Compatibility Layer

For a transitional period, provide adapter extensions for the old enum types:

```csharp
public static class LegacyFileTypeExtensions 
{ 
    // Convert from old enum to new class 
    public static FileType ToFileType(this DataFileType fileType) 
        => fileType switch 
        { 
            DataFileType.Excel => FileType.Excel,
            DataFileType.Csv => FileType.Csv, 
            _ => throw new ArgumentOutOfRangeException(nameof(fileType))
        };

    // Convert from new class to old enum
    public static DataFileType ToLegacyEnum(this FileType fileType) 
    {
        if (fileType == FileType.Excel) return DataFileType.Excel;
        if (fileType == FileType.Csv) return DataFileType.Csv;
        return DataFileType.Unknown;
    }
}
```

## 8. Core Enhancement Strategy

For the core base classes (`MappingRuleBase`, `ValueTransformationBase`, `ComparisonOperationBase`), we'll maintain their existing approach using string TypeId properties and extensibility features, as they already support the extensibility pattern we're aiming for.

## 9. Conclusion

This hybrid design provides a flexible, extensible foundation by:

1. Using smart enum patterns with static properties for common enumerations where extensibility is needed
2. Retaining true enums for simple cases with limited values where extensibility isn't required
3. Using string constants for JavaScript interop values
4. Leveraging the existing extensible design of core base classes
5. Providing backward compatibility for legacy enum usages

This approach balances the need for extensibility with type safety and code clarity while minimizing the need for refactoring.